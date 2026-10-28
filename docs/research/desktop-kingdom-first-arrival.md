# Desktop Kingdom first arrival and conditional control

Evidence date: 2026-10-14. **Confirmed:** admitted fresh Map22 exploration
with F522/F776 clear builds the baseline population and runs the complete
`cs_59656`. Its 136 commands stage the arrival, discussion and farewell, then
install Peter-following-Bowie and B-following-Peter. F776 is written by the init
caller only after that script returns. NPC128 is the baseline Caravan
mapsprite, not a WORKER sprite carried over from the mansion. **Inferred:**
the dialogue and staging portray Goliath placing the small party on his desk.
**Unknown:** natural arrival, actual supplied state, visible performance,
completed services/actions, usable control and persistence.

## Agreement and provenance

Standard rigor under [Issue #900](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/900)
and [content epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
The uncertainty is in complete scene/population/consumer joins. This one
research document supports later story design; no shared/index/registry,
contract, schema, fixture, code, remake or design change is included. Starting
accepted base: `73366b7a557fe03399a675a4103d89b89c9f9806`, with no stack.

Consume accepted [magic-door transfer and admission](./creed-magic-door-and-desktop-transfer.md),
[mansion entry](./creed-mansion-entry-and-first-goliath-scene.md),
[map inventory](./map-data-inventory.md),
[setup selection](./map-event-flag-route-selection.md) and
[request consumption](./map-event-request-consumption.md). Begin at the
predecessor's fresh exploration endpoint with F422 clear, before population.
F522/F776 clear selects the first-arrival scope. Stop after its conditional
local return/repeat boundary, before interactions, `cs_5996E`, or Battle22
opening/roster/outcomes. No runtime acquisition, map-wide audit, general
rendering research or new verification machinery is admitted.

Paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained evidence binds USA retail
ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
This is static source/retained-evidence review, not a new extraction or observed
playthrough. Short setup paths mean `data/maps/entries/map22/mapsetups/`;
map and entity consumers mean `code/common/scripting/map/` and
`code/common/scripting/entity/`. Fnnn is flag nnn. Command and row indices are
zero-based. Speeds, coordinates and waits are source operands, not measured
motion, presentation or elapsed time.

The unchanged map-init/map-script-engine pair totals 18,285,979 bytes. Linear
comparisons are budgeted below 512 MiB and one minute, plus bounded source
reads and small prose/receipts. The comparison completed in 0.081 seconds.
No new payload, environment or capture is needed; reassess material subsystem
or resource expansion before proceeding.

## Fresh population and setup selection

**Confirmed:** the predecessor's no-scroll `(22,0,0,Down)` request is consumed
before MainLoop's SwitchMap/CheckBattle. Map22 is not switched. F422 clear
avoids the sole Battle22 row; F422 set admits that battle even at `(0,0)` and
is outside this exploration premise. F522 set does not cancel an already
selected battle. Collection dialogue does not establish these flags.

`code/gameflow/exploration/explorationfunctions_2.asm:ExplorationLoop` clears
MAP_EVENT_TYPE, adjusts STEP_COUNTER, heals eligible allies, starts fade-out
and disables camera following. For explicit map 22 it writes CURRENT_MAP=22
and CURRENT_BATTLE=255, loads tilesets, waits fading, clears VInts, selects
entities, initializes them and loads mapsprites. It clears temporary flags
256–383, sets F80, initializes sprite-frame state, targets physical 0, loads
the map, installs base VInts, then calls RunMapSetupInitFunction. Thus the
mansion's parked/shrunk physical entities are not preserved by this fresh
entry. Healing does not imply revival of every dead ally.

`data/maps/mapsetups.asm:80–82` has just default `ms_map22`, F522 override
`ms_map22_flag522`, then msMapEnd. Each accessor runs GetCurrentMapSetup at
invocation. `pointertable.asm` and `pointertable_522.asm` share EntityEvents,
ZoneEvents, AreaDescriptions, Section5 and InitFunction; only their entity
pointer differs. F776 is not a population selector. The F522 alternate list
is a separate supplied-state boundary, not this baseline roster.

`mapfunctions.asm:InitializeMapEntities` clears entity records and lookup
bytes, then allocates selected followers beginning at physical 1. It consumes
setup rows in source order, and declares Bowie at physical 0 **after** those
rows with the requested `(0,0)`, Down, eas_Idle. It writes shared SPRITE_SIZE=16,
terminates the follower list and calls `followersfunctions_2.asm:sub_44404` for
mode/raft handling. Main is not the first newly declared record in this order.

All 30 baseline rows in `s1_entities.asm:4–33` follow. Physical column P assumes
only F67/F68 selected, no existing Eric/Karna lookup, and no raft overwrite.
`Init` means eas_Init; `Fixed` means eas_InitFixedSprite. NPC numbers are
logical script identities, not physical slots or battle combatants.

| Row | Logical identity | P | Mapsprite / action | Position, facing |
| --- | --- | --- | --- | --- |
| 0 | Eric 14 | 3 | Ally lookup; walking origin `(15,14)`, range 1 | `(15,14)`, Down |
| 1 | Karna 15 | 4 | Ally lookup; Init | `(5,15)`, Down |
| 2 | NPC128 | 5 | CARAVAN=62; Init | `(3,6)`, Down |
| 3 | NPC129 | 6 | YOUNG_MAN=196; Init | `(29,21)`, Right |
| 4 | NPC130 | 7 | YOUNG_WOMAN=197; Init | `(30,21)`, Left |
| 5 | NPC131 | 8 | OLD_MAN=198; walking origin `(24,22)`, range 1 | `(24,22)`, Down |
| 6 | NPC132 | 9 | PRIEST=192; Init | `(5,6)`, Down |
| 7 | NPC133 | 10 | MAID=208; Init | `(14,3)`, Down |
| 8 | NPC134 | 11 | KING_PONPEI=188; Init | `(15,2)`, Down |
| 9 | NPC135 | 12 | GUARD=206; Init | `(8,7)`, Left |
| 10 | NPC136 | 13 | GUARD=206; Init | `(33,24)`, Left |
| 11 | NPC137 | 14 | KING=153; Init | `(24,5)`, Down |
| 12 | NPC138 | 15 | QUEEN=135; Init | `(26,5)`, Down |
| 13 | NPC139 | 16 | BISHOP=144; Init | `(22,5)`, Down |
| 14 | NPC140 | 17 | BISHOP=144; Init | `(28,5)`, Down |
| 15 | NPC141 | 18 | KNIGHT=78; Init | `(20,5)`, Down |
| 16 | NPC142 | 19 | KNIGHT=78; Init | `(30,5)`, Down |
| 17 | NPC143 | 20 | ROOK=125; Init | `(18,5)`, Down |
| 18 | NPC144 | 21 | ROOK=125; Init | `(32,5)`, Down |
| 19 | NPC145 | 22 | QUEEN=135; Init | `(24,6)`, Down |
| 20 | NPC146 | 23 | QUEEN=135; Init | `(26,6)`, Down |
| 21 | NPC147 | 24 | PAWN=77; Init | `(22,6)`, Down |
| 22 | NPC148 | 25 | PAWN=77; Init | `(28,6)`, Down |
| 23 | NPC149 | 26 | PAWN=77; Init | `(20,6)`, Down |
| 24 | NPC150 | 27 | PAWN=77; Init | `(30,6)`, Down |
| 25 | NPC151 | 28 | PAWN=77; Init | `(18,6)`, Down |
| 26 | NPC152 | 29 | PAWN=77; Init | `(32,6)`, Down |
| 27 | NPC153 | 30 | ORB=172; Fixed | `(63,63)`, Down |
| 28 | NPC154 | 31 | ORB=172; Fixed | `(63,63)`, Down |
| 29 | NPC155 | 32 | ORB=172; Fixed | `(63,63)`, Down |

The complete table ends at msEntitiesEnd. `sf2mapsetupmacros.asm` encodes each
fixed row as four bytes and an action pointer, each walking row as four bytes
and `$FF,originX,originY,range`. The allocator masks X/Y by `$3F` and multiplies
by MAP_TILE_SIZE=384. Sprite operands below COMBATANT_ALLIES_NUMBER=30 use
ally lookup slots: an already nonzero slot skips the entire row and consumes
no physical slot. There is no joined/active membership admission test for Eric
or Karna. The current follower table contains neither, so the fresh declared
table does not itself create that skip case.

Other rows here are ordinary sprites below MAPSPRITES_SPECIALS_START=240.
They append physical indices at lookup offset 32 onward; the script resolver
subtracts 96 from logical 128–155. Eric/Karna do not consume that NPC cursor.
Consequently NPC128 is row 2, not Eric, Karna or a mansion WORKER. No selected
arrival command replaces NPC128's sprite or position. The dialogue calls the
speaker Goliath, but this establishes narrative attribution, not a different
mapsprite declaration or a physically present giant.

`entityfunctions_1.asm:DeclareNewEntity` initializes current/destination,
velocity/travel, facing, physical ENTNUM, sprite, flags and action/timer state.
For the two `$FF` walking descriptors it calls
`entityfunctions_2.asm:SetWalkingActscript`, copies 50 bytes of eas_Walking
and patches origin/range. `data/scripting/entity/eas_main.asm:eas_Walking`
waits 30, initializes speed/acceleration/collision state, then repeats random
walk, destination wait and wait 20. A declaration does not predict the actual
random position when later player interaction begins.

`followersfunctions_1.asm`, `data/scripting/entity/followers.asm` and the
complete `data/maps/global/overworldmaps.asm` establish that Map22 is not
an overworld map. F65 adds no Caravan/A prefix here. Ordinary follower rows
still allocate independently in this exact flag/logical order:
F66/Sarah, F67/Peter, F72/Kazin, F73/Slade, F74/B(Astral), F66/Chester,
F68/B(Oddler), F75/Luke, F69/C(Fairy), F70/B(Astral), F74/A(King Granseal),
F71/Lemon, F74/D(Minister), F76/Zynk, F77/A(Old Man), F82/Frayja,
F83/A(King Granseal), F83/D(Minister), F84/Rohde. Action pointers advance
only for selected rows. The normal only-F67/F68 example supplies Peter 7 at
physical 1/eas_Follower1 and B 31 at physical 2/eas_Follower2, both initially
at the requested position/facing.

Repeated logical B/A/D declarations are not deduplicated: later lookup writes
replace earlier mappings while old physical records remain. Missing Peter/B
lookups can resolve to physical 0; party membership alone does not provide
these followers. Additional flags alter allocations and action ownership;
the baseline physical column is not a universal roster. GetAllyMapsprite
uses class and HP, including BLUE_FLAME for a dead ally outside battle.

No selected entry/scene writes PLAYER_TYPE or saved raft location. Walking
mode 0 is the coherent predecessor premise, not a consequence of being on a
desk. `sub_44404` can declare physical 31 as RAFT when F64 is set and
RAFT_MAP=22, using saved X/Y masked by `$7F`; that overwrites the example's
ORB154 physical record while its NPC lookup still names 31. Mode 2 instead
sets main's sprite to RAFT, parks followers via MakeFollowersStand and declares
that raft slot at `(64,64)` without requiring the saved-map check. Thus mode
and raft state can change even the scene's ORB attribution. The initial
LoadEntityMapsprites includes nonempty physical 32; later ChangeEntityMapsprite
skips sprite transformation for ENTNUM=32. These are distinct consumers.

## Init dispatch and complete arrival scene

**Confirmed:** the complete `s6_initfunction.asm` is 218 lines. Its dispatcher
at lines 7–25 first checks F776: clear runs cs_59656 and only its return permits
setFlg 776. Independently afterward, F522 clear returns; F522 set checks F778,
returning when set or calling cs_5996E then setting F778 when clear. The latter
body is excluded. F776 already set bypasses all arrival commands; it is not
proof they previously completed. Both setup pointers use this dispatcher.

The complete selected cs_59656 occupies lines 29–218, including all seven
custom bodies, thirteen inline sequences and csc_end. The retained corpus
contains 136 top-level commands. In the table, B means FOLLOWER_B=31; triples
are ordered Bowie, Peter, B unless explicitly stated. Every inline body ends
with endActions and every custom body with ac_end.

| Commands | Complete ordered actions |
| --- | --- |
| 0–10 | Text cursor 1748; reloadMap `0,19`; waited Init for the triple; stopEntity for the triple; three waited custom bodies, each `ac_setSpeed 64,64; ac_jump eas_Idle`. |
| 11–20 | setPos Bowie `(4,8)`, Peter `(3,7)`, B `(5,7)`, ORB153 `(12,16)`, ORB154 `(11,15)`, ORB155 `(13,15)`, all Down; removeShadow for the triple; fadeInB. |
| 21–29 | Unwaited inline Down 16 for Bowie, Peter, B, then DownLeft 8 for ORB153 and ORB154; waited DownLeft 8 for ORB155; hide 153, 154, 155 in order. |
| 30–39 | Three waited custom bodies: `ac_motion OFF; ac_orientLeft` for Bowie / `ac_orientRight` for Peter and B, then `ac_updateSprite; ac_jump eas_Idle` each. Wait 100; startEntity triple; waited Init triple. |
| 40–51 | Headshake Bowie, Peter, B; face them Up, Right, Left; wait 20; B texts 1748–1750, Peter texts 1751–1752. |
| 52–54 | Waited B custom `ac_setSpeed 48,48; ac_jump eas_Idle`; waited B inline Left 1; B text 1753. |
| 55–66 | Wait 5; unawaited Peter RightLeft; wait 100; waited Peter inline Down 2 then Right 1; wait 5; unawaited Peter RightLeft; wait 130; face Peter Up, Bowie Down, B Down; Peter text 1754; NPC128 text 1755. |
| 67–76 | Unwaited Bowie inline Left 1; waited Peter inline Right 1; wait 5; unawaited Bowie RightLeft; wait 10; wait 5; unawaited Peter RightLeft; wait 5; unawaited B RightLeft; wait 130. |
| 77–89 | Face Peter, Bowie, B Down; waited Bowie Jump, unawaited Peter Jump; shiver Bowie then Peter; waited DeactivateAutoFacing for Bowie then Peter; unwaited Bowie inline Up 1, waited Peter inline Up 1; waited Init Bowie then Peter. |
| 90–96 | Peter continuing text 1756; two consecutive waited Peter Jumps; wait 10; Peter single text 1757; NPC128 continuing 1758 then single 1759. |
| 97–109 | Five repetitions of wait 20 then playSound DIALOG_BLEEP_6; face Bowie, Peter, B Right. |
| 110–120 | Five repetitions of wait 20 then playSound DIALOG_BLEEP_8; wait 50. |
| 121–129 | Waited Peter inline Left 2; face Bowie Down, Peter Up, B Down; Peter continuing 1760 then single 1761; face Bowie Right, B Left; B single text 1762. |
| 130–135 | Waited Init Bowie, Peter, B; followEntity Peter/Bowie/2, B/Peter/2; csc_end. |

### Reload, placement and concurrent staging

`sf2cutscenemacros.asm:reloadMap` encodes opcode $46 and two camera words.
`mapscriptengine_1.asm:csc46_reloadMap` disables view following, deactivates
VInts, sets d1=-1 and packs `(cameraX<<8 | cameraY&255)*3` into d0. Here it is
$0039. It calls `code/common/maps/mapload.asm:LoadMap`, enables display and
interrupts, reactivates VInts and waits one VInt. It does not request map 0,
move main to `(0,19)`, select another setup or initialize another population.

LoadMap's negative-d1 branch uses CURRENT_MAP=22 and existing map blocks/layout,
selects an area from that map's area data and establishes camera/plane state.
For explicit packed camera coordinates it splits the bytes and shifts each
by seven; this supplies `(0,7296)` before area parallax/offset handling, i.e.
the specified tile Y19 in 384-unit coordinates. Its complete body has no
InitializeMapEntities or RunMapSetupInitFunction call. Its InitializeDisplay
callee clears display/sprite-table state, not the logical entity population;
LoadMapArea assigns area, parallax and animation state. This differs from the
fresh ExplorationLoop path and cannot recursively rerun the arrival dispatcher.
CURRENT_BATTLE=255 uses full-map loading, not the Battle22 crop.

`csc19` writes current **and destination** X/Y, multiplied by 384, writes
facing and requests a sprite update. stopEntity/startEntity write animation
counter -1/0; they do not freeze movement or globally disable input. Custom
`ac_motion OFF` also writes animation counter -1 via esc14, despite its name;
it is not a velocity-zero or destination-cancel command. esc1B changes the low
orientation bits of FLAGS_B, which sprite transformation consumes independently
of facing. `ac_setSpeed` writes X/Y speed fields through esc10.

`csc30_removeEntityShadow` loads the mapped sprite into FF6802_LOADING_SPACE,
passes it through `entityfunctions_4.asm:sub_45A8C`, DMA-writes it and waits one
VInt. That helper rearranges sprite data through a 576-byte temporary buffer
for both frames; it creates no lasting shadow-status field. Later sprite
updates can reload the source pixels. fadeInB calls FadeInFromBlack;
`code/common/tech/interrupts/fadingcommands.asm:ExecuteFading` selects all
palettes, initializes counters, waits for FADING_SETTING to clear and another
VInt. Completion of this service is not an observation of the visible scene.

`csc2D` compiles each move to esc04 relative motion followed by esc01 destination
wait, appending an idle jump. Its installer first applies `andi.b #$9F` to
FLAGS_A, clearing bits 5 and 6. Down 16 is `(0,+16)` and DownLeft 8 is `(-8,+8)`;
esc04 scales signed deltas by 384 from actual current positions and calls
`entityscriptengine_2.asm:HasSameDestinationAsOtherEntity`. Its initial bit-5
check branches directly to its `clr.w d4` return when clear, leaving Z set; esc04's conflict
branch (`bne`) is not taken. These unmodified compiled moves therefore bypass
the competing-destination scan, including for the deliberately coincident
trio/ORB targets. Under the stated starting positions and completed unmodified
actions, the paired targets are Bowie/153 `(4,24)`, Peter/154 `(3,23)`,
B/155 `(5,23)`. This arithmetic is a **conditional** target join, not evidence
that bodies were carried or that all six arrived together. Only 155's inline
action is waited before the three hide calls. Alias, replacement or stalled
service can still break that nominal timing.

`csc2E` calls `entityfunctions_2.asm:HideEntity`: current and destination become
`$7000,$7000`, all matching lookup bytes become $FF, and a matching follower
entry is removed. This is stronger than parking at `(63,63)` in the mansion;
it does not merely toggle a visibility bit and does not clear the action
pointer. With the raft overwrite above, hiding 154 can hide that physical raft
and invalidate its other lookup too. Actual visible hiding remains unobserved.

### Action resets, look, jump, headshake and shiver

`data/scripting/entity/eas_actions.asm:eas_Init` resets acceleration factors
0/0, speed 32/32, acceleration/deceleration off, obstructability on,
entity/map uncollidable off, autofacing on, orientation Up, immersion off,
shared size 24, then updates the sprite and jumps idle. It does not place an
actor, set HP, or define persistent world scale. InitFixedSprite has the same
setup plus autofacing off and animation off before size/update/idle. The
custom speed 64 and later B speed 48 are therefore temporary; final Init
restores the selected actors' speed 32. Init itself has no animation-on command;
the earlier startEntity and later headshake restoration are separate writes.

RightLeft is eas_2xRightLeft: Right/Left/Right/Left, each with sprite update and
wait 30, then idle. Jump disables autofacing, moves relative `(0,-1)`, waits 5,
moves `(0,1)`, waits 5, moves `(0,0)`, restores autofacing and jumps idle.
DeactivateAutoFacing is only autofacing off then idle. An unawaited Jump or
RightLeft is installed, not joined; a subsequent action installation replaces
that actor's pointer even if the prior sequence has not finished.

`csc14/csc15` wait variants poll only the resolved actor's action pointer for
eas_Idle. `csc2D`'s waited variant uses the same predicate. Each installer seeds
the action wait timer from the physical index; esc00 consumes that timer.
There is no all-actor barrier or timeout. No sum of source waits proves exact
duration, completed displacement or all services returning.

`csc27_entityShakeHead` executes synchronously for each of the three calls:
sets animation counter -1, runs seven iterations (d7=6 with DBF), alternates
LoadMapsprite/sub_45D1C/DMA and LoadMapsprite/sub_45D46/DMA, with two VInts,
sprite update, then one VInt after each half; finally sets counter 0. The two
helpers rearrange selected sprite pixels; the operation does not turn the
actor's stored facing or move its position.

`csc2A_entityShiver` saves shared SPRITE_SIZE and the actor's animation counter,
writes size 21 and animation -1, then runs three iterations (d7=2): set resize
bit 3, update, Sleep 5; clear resize bit, update, Sleep 5. It restores the saved
size and animation counter, **not** an earlier resize-bit value. It is not a
new scale assigned permanently to Bowie/Peter. The source waits and calls
establish requested work, not actual rendered shaking.

esc0B writes a shared word, not per-actor size. ChangeEntityMapsprite consumes
that word only when the actor's resize bit is set; it also applies orientation
and other selected sprite effects. It skips special sprites and ENTNUM=32.
esc0A's sprite-load capacity gate can defer its action's progress. Initial
size 16, later Init size 24, temporary shiver size 21 and the predecessor's
size 18 therefore describe different writes to shared scratch, not a persistent
shrunk party or changed geometry, statistics, inventory or pickup attachment.

## Complete dialogue, speaker and sound joins

**Confirmed:** `data/scripting/text/gamescript.txt` keys 06D4–06E2 are decimal
1748–1762 and match all selected scene comments exactly, including controls.
C means nextText (continuing); S means nextSingleText (closes the text/portrait
and sleeps 10 after display). No selected text command is a Yes/No prompt.

| ID | Form; modifier; speaker | Exact literal |
| --- | --- | --- |
| 1748 | C; $C0; B | `What happened?{W2}` |
| 1749 | C; $C0; B | `A strange feeling swept over{N}me when I entered that door.{W2}` |
| 1750 | S; $C0; B | `Then, it felt like...I was{N}lifted upward!{W1}` |
| 1751 | C; $0; Peter | `It's very difficult to explain{N}what's happened to us....{W2}` |
| 1752 | S; $0; Peter | `Anyway, we're very small now.{W1}` |
| 1753 | S; $C0; B | `Small?  How small?{W1}` |
| 1754 | S; $0; Peter | `Let's see...now we're on a{N}desk...and it's very large,{N}like a village.{W1}` |
| 1755 | S; $0; 128 | `Exactly.{W1}` |
| 1756 | C; $0; Peter | `Goliath!{W2}` |
| 1757 | S; $0; Peter | `Hey, what did you do to us?{W1}` |
| 1758 | C; $0; 128 | `I told you not to go in{N}there.  You didn't listen{N}to my warning.{W2}` |
| 1759 | S; $0; 128 | `You guys look so cute!{N}Ha, ha!{N}Bye, little kiddies!{W1}` |
| 1760 | C; $0; Peter | `{LEADER}, cheer up!{W2}` |
| 1761 | S; $0; Peter | `Let's look around this{N}Desktop Kingdom.{W1}` |
| 1762 | S; $0; B | `I agree!  We can meet Creed{N}later.{W1}` |

`sf2cutscenemacros.asm` packs modifier/speaker into a word. csc00/csc02 in
mapscriptengine_2 and csc1D in mapscriptengine_1 resolve portrait and speech
from the **actual mapped sprite**, via GetEntityPortaitAndSpeechSfx and
`data/spritedialogproperties.asm`. $C0 sets both right-side and mirrored
portrait bits; $0 sets neither. An already open portrait is not reopened.
For B mapped to ODDLER, the table selects Oddler's portrait and bleep 4;
Peter's base/promoted sprites select their own portraits and bleep 4/5.
The complete table has no CARAVAN entry, so baseline NPC128 falls back to
PORTRAIT_DEFAULT=$FFFF (no new portrait window) and bleep 6. The scene does
not exchange it for a Goliath sprite. Other flags/HP/class/aliases can change
these lookups. **Inferred:** B is intended as Oddler and NPC128's lines are
Goliath's voice; neither inference replaces the literal entity identity.

`code/common/scripting/text/textfunctions_1.asm` consumes N as line advance;
W1/W2 wait on the directional/A/B/C mask, with W2 also using the arrow and
validation-sound path. LEADER calls UpdateForce and resolves the first battle
party member's name. It is not an immutable string "Bowie". ExecuteMapScript
can latch SKIP_CUTSCENE_TEXT under debug mode plus P2 Start; this bypasses
selected text commands and csWait sleeps. The nominal text sequence assumes
that skip is not latched. Source wait values do not establish user input or
displayed duration.

The ten explicit playSound calls are separate from per-character speech SFX.
Five bleep 6 requests (constant 74), each preceded by csWait 20, occur before
the trio face Right. Five bleep 8 requests (76), each preceded by csWait 20,
follow, then csWait 50. `csc05_playSound` passes its operand through
SOUND_COMMAND_GET_D0_PARAMETER; it does not wait for audible playback to finish.
There is no actor movement for NPC128 in this passage. Treating the calls as
heard giant footsteps or a completed visible departure would exceed the source
and consumer evidence.

## Final following and conditional local return

**Confirmed:** csc2C resolves follower and target to physical indices, uses
operand 2 to select signed offsets `(-24,0)` from table_FollowerPositions and
calls `entityfunctions_2.asm:AddFollower`. That copies the 42-byte eas_Follower1
program into walking-parameter storage, patches target/offsets, installs the
pointer and adds the physical follower to EXPLORATION_ENTITIES only if absent.
It neither recruits an ally nor teleports them into a final formation. The
copied action configures speed/collision behavior and repeatedly follows its
target/destination; command return is not a completed following observation.

The alive guard needs its actual encoding, not the macro's English name.
csc19 position, csc23 facing and csc1B/1C animation commands can skip a dead
ally through AdjustScriptPointerByCharacterAliveStatus. Action installation,
headshake, shiver, removeShadow and hide do not use that guard. csc2C reads
`move.b (a6),d0` **before** reading the word follower operand: for encoded
$0007/$001F that byte is zero, so the two selected follow calls test Bowie's HP,
not Peter's or B's. `code/common/stats/combatantstats_1.asm:GetCurrentHp`
reaches `combatantstats_3.asm:GetCombatantEntryAddress`, which masks the
combatant index to its low byte. The resolver afterward reads the full operands.
Thus altered HP/mappings can leave different partial state; do not summarize
all these handlers as sharing one alive or missing-entity policy.

`csc_end` conditionally waits for view scrolling when a dialogue window exists,
clears VIEW_SCROLLING_SPEED and restores caller registers. It does not join all
concurrent actors. RunMapSetupInitFunction merely selects/calls the callback
and restores registers; unlike the zone wrapper, it adds no physical-0
coordinate wait or portrait/text closure. F776 is set only after cs_59656's
return, then the independent F522/F778 dispatcher runs as described above.
Inside cs_59656 there is no story-flag, F422/F522/F776, party-membership,
PLAYER_TYPE, saved-raft, warp or battle-admission write. F80/temp-flag changes
belong to the earlier exploration caller, not the arrival scene.

After the init callback returns, ExplorationLoop compares palette words and
conditionally plays map music/fades in, then calls SetMoveSfx and WaitForEvent.
If MAP_EVENT_TYPE is zero at entry, WaitForEvent selects camera target physical
0 and installs the current-mode controller there. Mode 0 uses
eas_ControlledCharacter; mode 1 uses the source-named eas_Raft (Caravan control),
otherwise eas_Caravan (raft control). It does not validate an arbitrary mode.
A nonzero pending request skips that controller installation and takes priority
over A/C input; replacement work follows its current type/parameters. Scene
return therefore grants no unconditional useful-control or stable-event claim.

The fresh map-index path did not run UpdateMainEntityProperties or
DeclareRaftEntity, which belong to ExplorationLoop's map-index-255 branch.
Neither reloadMap nor init return goes through that alternate entry, rebuilds
the population, or restores main to the original `(0,0)`. The supplied walking
mode and actual scene mutations remain relevant. Conditional return stops here,
before any local interaction or Chessboard admission.

## Counterexamples and reproduction boundary

| Changed premise | Source discriminator |
| --- | --- |
| F776 independently set, F522 clear | Baseline population still loads, but cs_59656 and its follow/placement work are bypassed. No prior completion may be inferred. |
| F522 set, F778 clear/set | Alternate population is selected. After the independent F776 check, the dispatcher respectively calls the excluded postbattle scene or returns. F776 clear does not suppress this second check. |
| F422 set at admission | The predecessor can enter Battle22 instead of this fresh exploration path; desk coordinates do not exclude it. |
| Different follower selectors or missing/duplicate aliases | Physical slots, sprite/speaker identity and replaced action pointers change; logical names do not guarantee distinct actors. |
| Altered HP/class | Sprite selection and the differing alive guards above change work, including Bowie's high-byte guard on final following. |
| Mode 2, or F64 with RAFT_MAP=22 | Main/follower/slot31 behavior changes; ORB154 in the ordinary two-follower example can actually address the raft. Map22 is not an overworld exemption. |
| Shared size changed or a sprite update deferred/skipped | A later rendered projection need not match an assumed persistent miniature scale or simultaneous action completion. |
| An unfinished/replaced concurrent action, stalled text/fade/motion service | Some mutations may already exist, but later dialogue, F776 write or control installation need not occur; there is no rollback/all-actor barrier. |
| Pending request supplied/replaced before WaitForEvent | Pending work wins over controller installation; this local return is not a protected completed transfer. |

Load `. ./local/private-inputs.ps1` in the same process as controlled commands,
verify clean pinned source, then use the existing
[retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init equality, _verify_map_script_engine_fixture and source _cutscene_facts
equality. Do not regenerate the retained pair. For selected joins, compare
programCorpus.programs entry `id=cs_59656` in
`local/derived/map-script-engine-static.json` with all source lines/macros/
arguments through its terminator, then separately read the seven custom and
thirteen inline bodies. Join the fifteen gamescript keys to comments and the
literal table above. Check all 30 baseline rows and constants, both pointer
tables, complete init dispatcher and the named immediate consumers.

The fixtures do not independently guard every prose claim. Complete bounded
source/consumer review, whole-owned-diff review, links/provenance/private
boundary and whitespace checks supplement the retained comparisons. Run the
configured committed `uv run sf2 verify plan --base origin/main --head HEAD`
and record actual final-head CI. Documentation scope does not select local
blanket normal/full/engine/Godot/H3/H4 suites. Exact identity/check results and
inspection failures belong in Issue900's handoff.

Preserve [Issue896's original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/896#issuecomment-6173732542),
[rebased receipts](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/896#issuecomment-6173857962)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/897#issuecomment-6173871815),
including their original/rebased identities, unchanged retained pair, linked
predecessor results and completed inspection failures.

**Unknown:** natural arrival and supplied flags/setup/party/HP/class/mapping,
mode/raft/pending state, service/concurrent-action completion, rendered ORB,
shadow, size, pose and NPC performance, heard sounds, actual useful control and
save persistence. Counterexamples above are static discriminators, not executed
scenarios or an automatic runtime queue under
[ADR0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../decisions/0016-remake-start-evidence-deferral.md).
