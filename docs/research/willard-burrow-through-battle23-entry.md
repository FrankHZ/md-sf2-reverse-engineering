# Willard Burrow exploration through Battle23 admission

Evidence date: 2026-10-14. **Confirmed:** Map27 has three baseline residents,
an empty F523 population, eight no-scroll warp records and one Y19 zone
whose F781-clear callback runs the complete confrontation before setting
F781. The scene sets F423 and requests an explicit Map27 warp; the normal
consumer selects Battle23. **Inferred:** the residents threaten to eat the
party. **Unknown:** natural traversal, supplied state, completed presentation
and actions, usable control and persistence. Admission ends at the call to
BattleLoop, before battle initialization or activation.

## Agreement and provenance

Standard rigor under [Issue #928](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/928)
and [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
The uncertainty is in population/alias, zone/action and request/admission
joins. This sole document supports the next English story/design outcome;
no shared index, registry, schema, fixture, tooling, design or remake change
is required. Accepted starting base is
`05a60658ec87cd023854e388badeaa241063a314`, including corrected #924/PR927.
There is no stacked dependency on Floor World design work.

Begin at the accepted [Floor World outbound admission](./floor-world-interactions-and-exits.md),
using [first-arrival](./floor-world-first-arrival.md),
[setup selection](./map-event-flag-route-selection.md),
[interaction state](./map-event-interaction-state.md) and
[request consumption](./map-event-request-consumption.md) as dependencies.
End before Battle23 before/start hooks, roster, AI, rounds or outcomes, and
before destination Floor World/Creed initialization. No maze solution,
general movement/renderer expansion, runtime acquisition or new verification
machinery is admitted.

Source paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained evidence identifies USA
ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Short setup paths mean `data/maps/entries/map27/mapsetups/`; script consumers
are in `code/common/scripting/map/` and `code/common/scripting/entity/`.
Fnnn denotes flag nnn; indices are zero-based. Coordinates, waits and speeds
are source operands, not measured timing or observed positions.

Reuse the unchanged 18,285,979-byte map-init/map-script-engine pair and
selected existing fixtures. Comparisons are linear, budgeted below 512 MiB
and one minute, plus prose and small receipts; no regeneration, installation,
new environment or capture. Reassess material expansion. Aggregate equality
cannot establish a naturally connected route.

## Admission, setup and the complete population

**Confirmed:** the predecessor's ordinary request is Map27 `(2,7)`, Up.
`code/gameflow/mainloop.asm:MainLoop` calls SwitchMap then CheckBattle.
There is no Map27 substitution. With F423 clear, the only Map27 battle row
is skipped and exploration loads; with F423 set, Battle23 is selected before
any exploration population/init, even if F781 is clear. The supplied request
coordinates assume the coherent nonnegative-coordinate premise described
below; a battle row's wildcard trigger does not repair abnormal coordinates.

`explorationfunctions_2.asm:ExplorationLoop` clears the pending type,
adjusts STEP_COUNTER, heals eligible living/immortal allies, requests fade-out
and disables camera following. Explicit Map27 writes CURRENT_MAP=27 and
CURRENT_BATTLE=255, loads tilesets, waits fading, clears VInts, selects and
initializes entities, loads sprites, clears temporary flags256–383 and sets
F80. F423/F523/F781 lie outside that temporary range. Shared entry initializes
sprite-frame state, targets physical0, calls LoadMap, installs base VInts and
runs the selected init. Conditional map music/fade follows; a returning init
alone does not establish playable control.

`data/maps/mapsetups.asm:92–94` selects ms_map27 by default and
ms_map27_flag523 when F523 is set. `GetCurrentMapSetup` scans the current
map's flag records and returns the selected pointer table each time an
accessor runs. Both six-pointer tables share EntityEvents, ZoneEvents,
AreaDescriptions, Section5 and InitFunction. Only their entity-list pointer
differs. The complete `s6_initfunction.asm` at `$59FB6..$59FB8` is rts:
there is no local scene, EGRESS write, flag repair or F781 initialization.

The three eight-byte rows at `$59F72..$59F8A` in `s1_entities.asm` have no
terminator in that file. Actual `data/maps/mapsetupsstorage.asm:380–389`
includes both pointer tables, then s1_entities.asm, immediately followed by
s1_entities_523.asm, then the event files. The latter's
ms_map27_flag523_Entities label at `$59F8A` is the shared msEntitiesEnd.
Thus the baseline consumes all three rows and that terminator; the alternate
starts at the terminator and consumes zero rows. This is an include-order
join, not a missing-terminator assumption or a filename-derived flag rule.

| Baseline row | Logical identity | Position | Facing / sprite / action | Example physical slot |
| --- | --- | --- | --- | --- |
| 0 | 128 | `(43,20)` | Left2 / RAT79 / eas_Init | 3 |
| 1 | 129 | `(43,21)` | Left2 / WILLARD154 / eas_Init | 4 |
| 2 | 130 | `(43,22)` | Left2 / RAT79 / eas_Init | 5 |

The example assumes only F67/Peter and F68/B(Oddler), coherent distinct
mappings and no overwrite. `mapfunctions.asm:InitializeMapEntities` clears
49 physical records and 64 lookup bytes, allocates followers from physical1,
then these population rows, and finally Bowie at physical0 at the requested
position/facing with eas_Idle. It sets shared SPRITE_SIZE=16, terminates the
follower list and calls sub_44404 for raft handling. DeclareNewEntity writes
current/destination, velocity/travel, sprite, facing, physical ENTNUM,
action/timer and flags; the listed positions are declaration values.

All three sprites are regular non-ally values between30 and239. They append
lookup bytes32–34, with no joined-force, HP, class or ally-row duplicate skip.
GetEntityAddressFromCharacter masks the logical ID to a byte and subtracts96
for IDs128–255:128–130 therefore resolve through32–34, not through a battle
enemy roster. Logical129 is Willard in this population; it is not Floor
World's young man retained across the explicit load. Population is not
recruitment, enemy activation or a battle roster declaration.

`followersfunctions_1.asm` and `data/scripting/entity/followers.asm` select
followers by flags, in this order: F66/Sarah, F67/Peter, F72/Kazin,
F73/Slade, F74/B(Astral), F66/Chester, F68/B(Oddler), F75/Luke,
F69/C(Fairy), F70/B(Astral), F74/A(King), F71/Lemon, F74/D(Minister),
F76/Zynk, F77/A(Old Man), F82/Frayja, F83/A(King), F83/D(Minister),
F84/Rohde. Map27 is neither a no-follower map nor in the complete overworld
list, so F65 adds no Caravan/A prefix. Selected rows alone advance the action
pointer cursor. Only F67/F68 allocates Peter7 at physical1/eas_Follower1 and
B31 at physical2/eas_Follower2. Additional selections shift resident slots;
later B/A/D declarations overwrite the corresponding logical lookup while
earlier physical actors remain. Arbitrarily broad supplied flag combinations
are not a proven supported follower/action-table configuration.

Ally membership alone does not allocate Peter; ally HP/class affects its
mapsprite (dead exploration allies use BLUE_FLAME). B's actual sprite depends
on the last selected B declaration. A fresh F523-set load leaves resident
lookups32–34 zero. Zero resolves to physical0, not an absent-actor no-op;
`$FF` and other invalid lookups have no safe missing-actor guard either.
The confrontation does not create or validate its actors. Supplied F523 set
with F781 clear can therefore run it against aliases of main.

Fresh-load sub_44404, in followersfunctions_2.asm, makes main a RAFT in mode2,
parks followers at raw `$6000,$6000` with eas_Standing, and declares physical31
at tile `(64,64)`. Other modes require F64 and RAFT_MAP=27 for a saved raft,
using saved X/Y masked by `$7F`. Lookup `$3F` points to physical31; supplied
state can overwrite that physical record. FOLLOWER_C=158 resolves through
lookup62; FOLLOWER_D=159 shares lookup63 with the raft, so raft declaration
can replace the D mapping as well. Neither shares resident lookups32–34.
No selected local scene changes
PLAYER_TYPE, raft state or follower membership.

A no-battle MAP_CURRENT=255 entry is different: it preserves CURRENT_MAP,
existing population/lookups and working layout, skips explicit tilesets,
allocation, temporary-flag clearing and F80, and uses UpdateMainEntityProperties
and DeclareRaftEntity before common LoadMap/init work. It updates main's
position/facing and sprite; DeclareRaftEntity returns for mode2, skips follower
repositioning for mode1, otherwise repositions listed followers, then handles
F64/saved raft presence or absence. Changing F523 alone does not remove already
allocated actors on this path. The shared return-only Map27 init does not
rebuild them. This is conditional re-entry, not a fresh-map reset.

## Silent local callbacks and scoped empty lists

**Confirmed:** `s2_entityevents.asm` at `$59F8C..$59F92` contains only the
default record (flags0) and Map27_DefaultEntityEvent's rts. An activated
resident reaches the ordinary selector/wrapper described by the interaction
owner; there is no explicit Willard conversation or battle trigger in this
table. RunMapSetupEntityEvent still resolves actual sprite portrait/speech,
disables entity VInt work around the callback and closes windows/reactivates
it afterward. Flags0 requests neither facing change nor restoration. No
local text, recruitment, inventory, flag or request mutation is performed.

`s4_descriptions.asm` is only rts (`$59FAC..$59FAE`);
RunMapSetupAreaDescription begins d7=0, so it reports no handled local
description. `s5_itemevents.asm` at `$59FAE..$59FB6` has only the default
record and rts. RunMapSetupItemEvent starts d6=0; the local callback supplies
no handled item action, while its wrapper still clears speech, stores caller
Y in EVENT_RELATIVE_POSITION, closes windows and waits for physical0.
`7-chest-items.asm` and `8-other-items.asm` each contain only endWord.
These are empty local callback/data surfaces, not proof that global item
behavior, search responses or all possible acquisition routes are absent.
Neither direct resident activation nor item use calls the confrontation here.

## All eight warps and their admission boundaries

**Confirmed:** `data/maps/entries/map27/6-warp-events.asm` at
`$A93F6..$A9438` contains these eight complete records followed by endWord.
Each has type0/warpNoScroll and a zero padding byte after destination facing.

| Row | Trigger X/Y | Destination map | Destination X/Y | Facing |
| --- | --- | --- | --- | --- |
| 0 | `(2,8)` | FLOOR_WORLD26 | `(59,4)` | Down3 |
| 1 | `(11,10)` | CURRENT255 | `(30,8)` | Right0 |
| 2 | `(30,8)` | CURRENT255 | `(11,10)` | Left2 |
| 3 | `(33,13)` | CURRENT255 | `(12,15)` | Left2 |
| 4 | `(12,15)` | CURRENT255 | `(33,13)` | Right0 |
| 5 | `(54,2)` | CURRENT255 | `(3,3)` | Right0 |
| 6 | `(255,0)` | CREED_MANSION33 | `(15,9)` | Right0 |
| 7 | `(21,255)` | CREED_MANSION33 | `(15,9)` | Right0 |

`sf2mapmacros.asm:mWarp/warpNoScroll/warpMap/warpDest/warpFacing` encode
trigger X/Y, type, map, destination X/Y, facing and padding. Facing is an
output, not an approach restriction. Rows0–5 use exact triggers; row6 permits
any X at Y0, row7 any Y at X21. There is no reverse `(3,3)` record. If both
final wildcard rows match, the first wins with the same destination.

In ordinary esc02_controlCharacter, candidate layout class `$1000` under
mask `$3C00` calls `code/gameflow/exploration/exploration.asm:WarpIfSetAtPoint`.
It signed-divides supplied raw candidate X/Y by384, selects CURRENT_MAP's
warp data, scans in order, stops at word -1 and treats a negative trigger
byte as wildcard. It writes event type1, copies type/map/X/Y into PARAM1–4
and facing/padding into PARAM5–6, and writes WARP_SFX=SFX_WARP. A matching
table coordinate alone does not prove a move, a layout trigger or a walkable
route. Other vehicle callers share the matcher; their legality is not expanded.

The no-scroll consumer and MainLoop join below apply to all eight records.
SwitchMap has no26/27/33/255 substitution; the complete battle coordinate
table has no26 or33 row. Thus ordinary Floor World and Creed requests admit
exploration, stopping before destination population/init. Current-map requests
resolve CURRENT_MAP=27 for CheckBattle: F423 clear preserves the original255
request and takes the retained-state exploration path; F423 set admits
Battle23 instead of local traversal. No selected warp row/matcher tests
F781 or F523 or consumes an item. F523 can affect later fresh population or
clear F423 during an already selected battle, but is not an exit prerequisite.
This does not establish post-battle reach or permission to bypass geometry.

## Y19 zone, scene entry and the complete confrontation

**Confirmed:** `s3_zoneevents.asm` at `$59F92..$59FAC` contains one specific
row, `msZoneEvent 255,19,Map27_ZoneEvent0`, plus the default return target.
RunMapSetupZoneEvent compares low d1/d2 bytes, treating `$FF` as wildcard:
any X/Y19 selects the specific callback. It tests F781; set returns directly,
clear invokes cs_59FB8, then sets781 only after the script returns. A nonmatch
returns through the default. Neither F423 nor F523 nor party/HP is tested.

The input registers come from the actual zone request, not an actor-relative
position. In entityscriptengine_2.asm:esc02, current coordinates seed a
candidate; direction, area/slope and entity checks precede loc_5220's layout
classification. At loc_5294 the masked class must be `$1400` to write event6.
Its parameter words derive from `(XDEST+d4)/384` and `(YDEST+d5)/384`, using
the selected step deltas. This precedes remaining terrain/debug checks and
movement assignment. Y19 therefore does not prove current Y19 or a completed
step. Working layout, supplied mode/action, destination and pending state
remain necessary premises.

WaitForEvent prioritizes a nonzero pending type over A/C input and skips
controller installation when one is pending. ProcessMapEvent clears its type
before dispatching the sampled event. Type6 installs eas_Init on physical0
through j_ApplyInitActscript without waiting, loads d1/d2 from PARAM1/PARAM3,
and calls the zone wrapper. Trap6 activates VInt_UpdateEntities and invokes
ExecuteMapScript. No actor lookup or story-state repair is added at entry.

`scripts.asm:4–44`, `$59FB8..$5A09E`, has exactly 41 top-level commands.
B means FOLLOWER_B=31; Peter means ALLY_PETER=7. Every row below is in source
order; grouped actors elsewhere in this document do not imply simultaneous
execution.

| Index | Command and operands |
| --- | --- |
| 0 | textCursor1733 |
| 1 | setActscriptWait B,eas_Init |
| 2 | setActscriptWait Peter,eas_Init |
| 3 | cameraSpeed `$30` (48) |
| 4 | setCamDest22,15 |
| 5 | setDest B,26,20 |
| 6 | csWait5 |
| 7 | setActscript B,eas_2xRightLeft |
| 8 | csWait120 |
| 9 | nextSingleText modifier0,B (1733) |
| 10 | setDest Peter,27,20 |
| 11 | setFacing B,Right |
| 12 | shiver Peter |
| 13 | nextSingleText modifier0,Peter (1734) |
| 14 | setCamDest36,17 |
| 15 | setDest129,41,21 |
| 16 | nextSingleText modifier`$C0`,129 (1735) |
| 17 | setActscript128,eas_RotateRightHighSpeed |
| 18 | setActscript129,eas_RotateRightHighSpeed |
| 19 | setActscript130,eas_RotateRightHighSpeed |
| 20 | csWait40 |
| 21 | setActscriptWait128,eas_Init |
| 22 | setActscriptWait129,eas_Init |
| 23 | setActscriptWait130,eas_Init |
| 24 | setFacing128,Left |
| 25 | setFacing129,Left |
| 26 | setFacing130,Left |
| 27 | setActscript128,eas_Jump |
| 28 | setActscript129,eas_Jump |
| 29 | setActscriptWait130,eas_Jump |
| 30 | startEntity128 |
| 31 | startEntity129 |
| 32 | startEntity130 |
| 33 | nextSingleText modifier0,Peter (1736) |
| 34 | nextText modifier`$C0`,129 (1737) |
| 35 | nextSingleText modifier`$C0`,129 (1738) |
| 36 | setActscript128,eas_Jump |
| 37 | setActscriptWait130,eas_Jump |
| 38 | setStoryFlag23 |
| 39 | warp WILLARD_BURROW27,0,0,Right |
| 40 | csc_end |

### Named actions, placement and wait semantics

`mapscriptengine_1.asm:csc15` resolves the actor, seeds its action-read timer
with the physical index, replaces the action pointer and, only for the `$FF`
wait form, polls that same record's pointer until eas_Idle. It has no HP guard,
all-actor join or timeout. It does not itself clear collision bits. An alias
can replace another logical actor's action; a later replacement can change
what the wait proves. VInt_UpdateEntities skips raw X at or above `$7000`,
so parked/invalid state can prevent progress.

The complete named bodies in `data/scripting/entity/eas_actions.asm` are:

| Body / source lines | Entire ordered operations |
| --- | --- |
| eas_Init,168–180 | accelFactors0,0; setSpeed32,32; acceleration OFF,OFF; deceleration OFF,OFF; entityObstructable ON; entityUncollidable OFF; mapUncollidable OFF; autoFacing ON; orientUp; immersed OFF; setSize24; updateSprite; jump eas_Idle |
| eas_2xRightLeft,225–237 | setFacing Right; updateSprite; wait30; setFacing Left; updateSprite; wait30; setFacing Right; updateSprite; wait30; setFacing Left; updateSprite; wait30; jump eas_Idle |
| eas_RotateRightHighSpeed,102–118 | motion OFF; autoFacing OFF; setFacing Left; updateSprite; wait1; setFacing Up; updateSprite; wait1; setFacing Right; updateSprite; wait1; setFacing Down; updateSprite; wait1; branch back to eas_RotateRightHighSpeed |
| eas_Jump,4–11 | autoFacing OFF; moveRel0,-1; wait5; moveRel0,1; wait5; moveRel0,0; autoFacing ON; jump eas_Idle |
| eas_Idle, eas_main.asm:301–303 | wait1; branch back to eas_Idle |

`sf2cutscenemacros.asm` and entityscriptengine_2.asm resolve these operations.
Init sets acceleration factors0 and speed bytes32, clears acceleration and
deceleration controls, sets FLAGS_A bit7, clears bits5/6, enables auto-facing,
sets orientation bits to0, clears immersed, sets shared sprite size24 and
updates the sprite. orientUp is orientation, not a write of facing Up. Init
contains no position reset, destination wait or motion-ON command. The two
turning scripts change facing/sprite, not tile position. `ac_motion OFF`
writes ANIMCOUNTER=-1; it is not a general position/velocity halt.
The rotate body loops until replaced; wait40 then three waited Init commands
replace it in order. Those Init calls do not restart its animation counter.
The later startEntity commands write ANIMCOUNTER=0 and do not allocate actors.

`csc29_setEntityDest` uses absolute tile words, multiplied by384, writes
destination/travel and signed velocity magnitude32 on each axis, then waits
for current X/Y to equal that actor's current destination unless actor bit15
is set. All three selected operands have bit15 clear, so B `(26,20)`, Peter
`(27,20)` and129 `(41,21)` are waited absolute destinations. This consumer has
no HP guard and does not change current position instantaneously or reset
the action pointer. A destination replacement changes its wait target.

`csc23_setEntityFacing` writes facing and refreshes the sprite; csc1B resets
the animation counter. These two consumers use AdjustScriptPointerByCharacterAliveStatus:
only true allies below30 without enemy bit are skipped for zero HP. B31 and
NPC128–130 bypass that guard, including every selected facing/start command.
The Init/action, destination and shiver calls do not acquire this guard merely
because Peter7 is their operand; a dead or absent Peter is not automatically
skipped. The scene performs no heal, revival or recruitment.

Jump's moveRel is esc04, adding signed tile deltas to the actor's **current**
position at each execution, not to its population row or previous destination.
It writes travel and velocities using current speed bytes at loc_55C8. The
preceding completed Init supplies32/32 and clears FLAGS_A bit5; with bit5
clear, HasSameDestinationAsOtherEntity returns Z set and esc04 bypasses the
destination-conflict branch. If changed state sets that bit, the scan matters.
This source-specific condition is not a general collision/physics exemption.
There is no ac_waitDest in eas_Jump: its two wait5 commands use the action
read timer, and moveRel0,0 targets its then-current position. It need not
complete a full tile up/down or return to its original Y. esc00 uses the timer,
esc34 jumps to the named action, and the idle pointer is the csc15 wait's
criterion. Only130 is waited in either jump group; no command joins128/129.
Equal script bodies do not establish simultaneous completion, duration or
final positions, especially with aliases, replacement or interrupted VInts.

`csc2A_entityShiver` resolves Peter, saves shared SPRITE_SIZE and its animation
counter, uses size21/counter-1, then three times sets/clears FLAGS_B bit3,
updates the sprite and sleeps5 after each change. It restores the saved size
and counter. This is a sprite effect, with no smell sensor or damage rule.
`csc45` writes VIEW_SCROLLING_SPEED=48; csc32 disables target-following, calls
j_SetCameraDestination and waits scrolling. That effective consumer in
battlefunctions_0.asm scales tile operands by384 and calls display.asm's
SetViewDestination, applying area parallax/offsets and autoscroll guards.
The camera destinations are requests, not actor positions or observed framing.

### Six texts and actual speaker selection

`data/scripting/text/gamescript.txt` keys06C5–06CA are IDs1733–1738.
With no debug text skip, cursor1733 advances through exactly these commands:

| ID | Speaker / modifier | Literal content, with line breaks omitted | Control |
| --- | --- | --- | --- |
| 1733 | B / 0 | `(Sniff, sniff)... It smells like a ditch!` | W1 |
| 1734 | Peter / 0 | `Is this the smell of the devils that they were talking about?` | W1 |
| 1735 | 129 / `$C0` | `Hee, hee!` | W1 |
| 1736 | Peter / 0 | `What huge rats!` | W1 |
| 1737 | 129 / `$C0` | `Welcome!` | W2 |
| 1738 | 129 / `$C0` | `It's dinner time!` | W1 |

The first two literals contain `{N}` line breaks after the ellipsis, and
after `the` and `were`, respectively. These words establish dialogue, not
implemented smell detection, creature growth, food consumption or party harm.
The threat interpretation is **Inferred** from text plus the impending battle.

csc00/02 resolve actual logical-to-physical mapping and mapsprite through
GetEntityPortaitAndSpeechSfx. Baseline B/Oddler has portrait40/bleep4;
B/Astral instead has31/bleep6. Peter base has7/bleep4, promoted48/bleep5;
class/HP or altered mapping can change these. WILLARD154 has no entry in the
complete spritedialogproperties table, yielding DEFAULT(-1)/bleep6, not a
Willard portrait. Modifier`$C0` encodes right-side and mirrored requests in
bits15/14 (word `$C081` for129); it cannot create an absent portrait. csc1D
also preserves an already open portrait. The no-speaker sentinel is `$FFFF`,
which none of these commands uses; changing or missing lookups can change
speaker properties rather than safely suppressing the command.

csc00 waits scrolling, displays the cursor text, increments it, closes
portrait/text and sleeps10. csc02/nextText for1737 waits scrolling, displays
and increments but leaves windows open for the following csc00. In
textfunctions_1.asm, W1/symbol_wait1 and W2/@wait2 poll directional/A/B/C input
with VInt waits; W2 also updates a prompt marker and requests SFX_VALIDATION
on acceptance. W1 lacks that W2 sound request. Literal controls are not
proof of actual input or completed audible/visible presentation.

ExecuteMapScript clears SKIP_CUTSCENE_TEXT at entry; debug mode plus P2 Start
can latch it during dispatch. Skipped csc00/02 consumes operands without
display or cursor increment; explicit csWait5/120/40 sleeps are bypassed.
Other commands, internal shiver sleeps, action-script timers and waits,
flag writes and the warp still run. A mid-scene latch changes which cursor
values later text commands see; all six displayed lines are conditional.

## Ordered flag/request return and BattleLoop admission

**Confirmed:** csc13 reads23 and adds BATTLE_UNLOCKED_FLAGS_START=400 before
j_SetFlag, setting F423. It does not set F523 or F781. csc07 then writes event
type1 and PARAM1–5=`0,27,0,0,Right0`. It does not immediately load the map,
clear parameters beyond that range, or set WARP_SFX. Unlike table warps,
its later sound consumer uses the previously supplied WARP_SFX value.

csc_end conditionally waits scrolling if a dialogue window remains, clears
VIEW_SCROLLING_SPEED and restores registers. The caller then sets F781 and
returns. RunMapSetupZoneEvent closes portrait/text, waits one VInt, then waits
for **physical0** current X/Y to equal its current destination before restoring
registers. This separate wait is neither the scene's130 idle wait nor an
all-actor join. Missing resident aliases can make the scene itself move or
replace physical0's action, so the wrapper's wait is still conditional.

Only after these returns does ExplorationLoop reach SetMoveSfx/WaitForEvent.
If the warp remains pending, it wins over input and skips installing a new
controller or returning camera target to0. ProcessMapEvent clears the sampled
type, then the no-scroll arm requests stored WARP_SFX, clears that sound,
idles physical0, removes its caller return address to leave ExplorationLoop,
and zeroes d0–d4. It loads map27, samples actual main position/facing through
UpdatePlayerPosFromMapEvent and applies the explicit destination bytes/facing.
With coherent nonnegative local coordinates, it returns d0=27, d1=0, d2=0,
d3=Right0, d4=0 to MainLoop. It does not run destination init itself.

Preserve the accepted low-byte limitation: signed raw X/Y divided by384 first
fills coordinate words; a nonnegative destination byte replaces only the low
byte. Supplied raw X=-384 yields quotient `$FFFF`, then destination0 yields
`$FF00`, not `$0000`. This applies equally to the eight table warps. For
MAP_CURRENT exploration, UpdateMainEntityProperties tests low-byte negativity
but multiplies the whole word by384 and stores the low product word; it does
not normalize an abnormal high byte. No ordinary tuple claim covers such
supplied states without that qualification.

MainLoop's SwitchMap leaves27 unchanged. CheckBattle scans the complete
battlemapcoords.asm and selects row23 only when F423 is set. That row is
`27,23,14,27,16,255,255`: map27, area origin23/14, width27/height16,
wildcard trigger X/Y. It writes those area bytes. If F523 is set, it clears
F423 but still returns the already selected battle index23. MainLoop moves
that index to d1 and calls j_BattleLoop with d0=27. This is the stopping
boundary: no claim about before/start scripts, initialization, activation,
combat, victory or defeat follows.

| Changed state at the named boundary | Structural consequence |
| --- | --- |
| Initial F423 set, either F781 | Direct Battle23 admission; no exploration confrontation required |
| Fresh F423 clear / F523 clear | Three-row population, shared return-only init |
| Fresh F423 clear / F523 set | Empty resident population; F781-clear scene still admissible, with missing lookups |
| Y19 request, F781 set | No scene, new F423 write or scene warp; callback/wrapper still return conditionally |
| F781 clear, coherent actors, completed scene and surviving request | F423 write, warp request, scene return, F781 write, wrapper return, consumption, Battle23 admission |
| F781 set / F423 clear | Scene suppressed and ordinary Map27 admissions remain exploration; no local repair |
| Current-map traversal with F523 changed | Existing actors retained; event accessors reselect shared tables; no automatic population rebuild |
| F423 and F523 both set at CheckBattle | F423 cleared, current Battle23 selection retained |
| Scene interrupted before return | Earlier action/text/camera/F423/request effects may remain while F781 is clear |
| Request overwritten or supplied between producer and consumer | Shared current parameters govern; original scene/table tuple is not guaranteed |

There is no rollback transaction across F423, request storage, F781 and the
wrapper. Failure after F781 but before consumption can suppress a later scene
without proving admission. Pending storage is mutable, not an immutable
queue. These are order/state distinctions, not reproduced interruptions.
No selected local command writes EGRESS_MAP, grants an item, recruits a
character, sets completed523 or guarantees a post-battle route to Creed.

## Reproduction and remaining limits

Load `. ./local/private-inputs.ps1` in the same process as controlled commands
and verify the clean pinned read-only source. Reuse the existing
[comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
for map-init equality, _verify_map_script_engine_fixture and _cutscene_facts;
do not regenerate either payload. The retained pair comparison completed in
0.080s with size/mtime unchanged. Select programCorpus.programs entry
`id=cs_59FB8` from `local/derived/map-script-engine-static.json`; compare all
41 command indices, source lines, macro names and operand arrays to the pinned
source, then read all named action bodies and immediate consumers above.

Under `tests/fixtures/h2/map-events/`, routing-setup.json holds entity row640,
zone rows104/105 and item row39. Compare all four source macro/operand records
and both setups' shared-table joins. entity-programs.json program304 is the
return-only entity callback; zone-programs.json programs52/53 preserve the
five-operation guarded scene caller and default return. Compare every selected
operation/source line, including branch polarity and script-before-F781 order.
The request-consumption fixture
`tests/fixtures/h2/map-event-request-consumption-static-v1.json` retains
explorationLoop/waitForEvent/processMapEvent contexts. Separate preserved
consumer facts do not prove this scene naturally reaches them.

Direct checks cover both pointer tables, the baseline and shared terminator
include order, all eight full warp records and empty lists, enum/macro values,
all six texts and actual speaker lookup, action/timer/HP/alias consumers,
zone register derivation, camera consumer, low-byte override and the complete
switch/battle admission tables. Review the entire owned prose/diff, links,
provenance, English scope and private/generated boundary; run git diff --check
and clean committed configured `uv run sf2 verify plan --base origin/main --head HEAD`.
Record that plan and actual final-head CI on Issue928. Documentation scope
requires no local blanket normal/full/engine/Godot/H3/H4 suite or tests of
verification programs. Source equality alone does not guard every prose claim.

Preserve [#924 original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/924#issuecomment-6176361906),
[correction handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/924#issuecomment-6176448210),
[P2 review](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/927#issuecomment-6176402766)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/927#issuecomment-6176460244).
Original head `92fedfb20e9bcc05bcfb224034d2866d60657c6e` / tree
`4d54b7f351e28164d64cfb55829c86b947377852`, corrected head
`c0d52676ff607c81c089bf941eb7098f9b5d04d7` / tree
`e8cf4e7d1dc24598703e24ef043c46c599a565f5`, separate successful
CI38622136093/38622845061, pair0.082s, all inspection/transport failures and
linked #920/predecessor records remain evidence. This slice replaces none of
their completed results, original failures or correction boundaries.

**Unknown:** natural arrival, maze traversal, zone/exit reach and supplied
flags, positions/destinations, party/HP/class, lookup/alias, layout, mode and
pending state; completed input/text/camera/actions, visible/audible timing,
usable control and save/load persistence. No Battle23 initialization/outcome
or destination Floor World/Creed init is evaluated. These limits are not an
automatic runtime queue under [ADR0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../decisions/0016-remake-start-evidence-deferral.md).
