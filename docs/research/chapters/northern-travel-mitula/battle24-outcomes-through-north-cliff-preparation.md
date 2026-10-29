# Battle24 outcomes through North Cliff preparation

Evidence date: 2026-10-14. **Confirmed:** Battle24 has no leader-cleanup
victory. Its empty outcome scripts still execute shared wrappers; victory
returns a member-derived Map72 position, and the main loop enters exploration
without an immediate battle scan. A later admitted cave-zone callback checks
Cannon then Dynamite. Either missing-item result runs the same warning scene,
then sets temporary F256; both branches name Dynamite. **Inferred:** the usual
cast stages a reprimand before preparing the cave entrance. **Unknown:** natural
travel/outcome, supplied state, completed services, presentation, usable control
and save persistence.

## Agreement and evidence boundary

Standard rigor under [Issue #981](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/981)
and [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
outcome selection, map return, cave admission, current actors and scene completion
are distinct boundaries. Accepted base is `5d707f60739cd5554806ace001e69f602cd98715`,
including corrected #977/PR979. This is the sole owned file. No shared index,
fixture, schema, tooling, design, engine or UI change; #982 synthesis and UI
work are independent, with accepted sources only.

Consume [Battle24 admission/roster](./new-granseal-departure-through-battle24-admission.md),
[Battle23 outcomes](../southern-travel-creed/battle23-outcomes-and-burrow-return.md),
[Battle19 outcome consumers](../southern-travel-creed/battle19-outcomes-through-elven-village-entry.md),
[battle loop](../../battle/battle-loop.md), [cutscenes](../../battle/battle-cutscenes.md),
[inventory predicate](./new-granseal-inventor-and-dynamite-gift.md),
[Map72/Luke](../eastern-parmecia/polca-departure-through-luke-recruitment.md),
[Caravan/movement](../southern-travel-creed/caravan-boarding-and-southwest-parmecia-travel.md),
[request state](../../world/map-event-request-state.md) and
[request consumption](../../world/map-event-request-consumption.md).
Stop at the complete missing-item scene/local caller return and the admitted
both-item branch's next-scene boundary. No removal-callee audit, cs_4FFDA blast,
Rohde join, cave interior/Battle25, later journey, combat AI/damage simulation,
acquisition, extraction regeneration or runtime run is included.

Source paths are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The retained read-only checkout is clean at that pin. Retained fixtures/payloads
identify USA retail SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Fnnn means flag nnn; indices are zero-based and extents half-open. Constants
come from `sf2enums.asm`, RAM from `sf2const.asm`. Short battle filenames below
are under `code/gameflow/battle/`; map-script consumers under
`code/common/scripting/map/`; entity consumers under `code/common/scripting/entity/`.

Minimum evidence is selected pinned source, existing public fixtures and the
unchanged 18,285,979-byte retained map-init/script pair. Linear comparison is
budgeted below one minute/512 MiB, with small receipts/prose; the author pair
comparison completed in 0.081s. No new comparison framework or installation.
Replan material contradiction or expansion; Unknowns are not a runtime queue.

The [initialization/first-control account](./battle24-initialization-and-first-control.md)
owns preceding regions, repeat selection, AI/spell/item custody and conditional
dispatch. This owner retains outcome selection, return and cave preparation.

## Actual end checks and Battle24 discriminators

**Confirmed:** `battleloop_1.asm:@IsBattleEnd` executes the defeated hook,
ProcessKilledCombatants and CountRemainingCombatants, then tests zero allies
before zero enemies. With both counts nonzero, it runs ProcessAfterTurnEffects,
processes killed combatants again and repeats the same defeat-before-victory
checks. This second check does not execute the defeated hook.

| Selected source/table entry | Battle24 value and consumer |
| --- | --- |
| `data/battles/cutscenes/battleendcutscenes.asm`, slot24/byte48 at $47C18 | Relative ms_Empty pointer, read by ExecuteBattleCutscene_Defeated. |
| `data/battles/global/enemyleaderpresence.asm`, slot24 at $47CA6 | 0, read by the defeated wrapper's loc_47C48 tail. |
| `data/battles/cutscenes/afterbattlecutscenes.asm`, slot24/byte48 at $47D24 | Relative ms_Empty pointer, read by ExecuteAfterBattleCutscene. |
| `data/battles/cutscenes/afterbattlejoins.asm`, slot24 at $47D82 | 0, read by EndAfterBattleCutscene and passed to JoinForce. |
| `data/battles/global/afterbattlepositions.asm` | Only Battle05 has a placement override; none for24. |

`layout/sf2-07-0x044000-0x064000.asm` selects the **cutscenes** join table,
not its similarly named global counterpart. The complete `ms_empty.asm` target
is the word $FFFF at $47A4E..$47A50. Empty scripts do not cancel their wrappers.

The actual fresh roster from `data/battles/spritesets/spriteset24.asm` assigns
Dark Bishops128/135, Dark Snipers129/132/141, Skeletons130/133/134/136/139/140
and Lesser Demons131/137/138/142/143. Slots144–159 start HP0/X=-1. Bishop128's
AI=LEADER is an AI operand, not the outcome table's leader flag. Completion524
can change copied enemy definitions through the accepted upgrade path; F88
suspension uses current state instead of promising this fresh roster.

`cutscenes/battleendcutscenesstart.asm` requires Bowie HP nonzero and enemy128
HP zero before reaching dispatch/tail. F524 clear then executes ms_Empty;
F524 set skips it. Both reach `battleendcutscenesend.asm:loc_47C48`, whose zero
leader byte returns without appending living enemies. Killing Bishop128 alone
does not remove the other fifteen; Bishop135 has no special hook selector.
F474 is the intro flag, distinct from outcome524 and unlock424.

`battleloop/countremainingcombatants.asm` scans all30 ally and32 enemy slots:
nonnegative X byte and nonzero HP word count, regardless of joined/active
flags or type. Bowie HP0 forces ally count0; defeat wins simultaneous loss.
With Bowie alive and a positive ally count, zero counted enemies suffices.
Ordinary fresh Battle24 thus requires removing all counted enemies, not a
specific boss; supplied off-map/nonmember slots change counts. Bowie alive
alone does not repair zero placed allies.

`battleloop/processkilledcombatants.asm` uses the current length-delimited dead
list, not a fresh HP scan. Presentation precedes defeat/kill bookkeeping,
X/Y=-1, status0, stat recomputation and mapped entity position $7000/$7000.
It neither sets listed HP0 nor clears list length. Coherent list/mapping state
and completed spin/sound/VInt services remain premises; stale or duplicate
entries are not repaired by these outcome handlers.

## Ordered outcomes and return tuple

**Confirmed:** `battleloop_2.asm:BattleLoop_Victory` performs:

1. Heal all30 living allies plus Peter7/Lemon28 even when dead: HP/MP maxima,
   existing status masked to STUN/POISON/CURSE, then recomputed stats. Other
   dead allies stay dead. Membership is not the healing predicate; equipment
   effects still apply: UpdateCombatantStats removes incoming CURSE, then
   re-derives final ally CURSE from current equipped cursed gear. The intermediate
   mask is not final status. Battle44's timer special does not apply.
2. PARAM2 receives CURRENT_MAP. UpdateForceAndGetFirstBattlePartyMemberIndex
   rebuilds membership and chooses the first joined-and-active ID ascending,
   **not first living**. Its X/Y plus BATTLE_AREA_X/Y are added as bytes into
   PARAM3/4; its actual ENTITY_INDEX_LIST mapping supplies facing for PARAM5.
   PARAM1=0. This seed does not write MAP_EVENT_TYPE.
3. ExecuteAfterBattleCutscene tests524: clear dispatches ms_Empty, set skips it.
   Both reach EndAfterBattleCutscene and JoinForce(0). Bowie becomes joined;
   below twelve active members JoinBattleParty also sets active32. No member
   is evicted from a full party; this later join cannot repair the prior seed.
   It is not Rohde recruitment.
4. Clear424, set524; zero d0–d3 words and load PARAM2–5 bytes into them;
   return d4=1. Intro474 is unchanged.

For the selected Map72 area `(1,32)`, local `(x,y)` returns `(x+1,y+32)` modulo256
with current mapped facing, not initial placement or a fixed cave tile. Empty
active membership leaves a stale first-list byte; dead/off-map first members,
changed area/map/mappings or intervening parameter writes alter the seed.
There is no empty-list or facing repair. The script terminator can wait for
scrolling if dialogue remains open, then clears VIEW_SCROLLING_SPEED. An
unfinished after-script prevents join/flag writes; an unfinished join prevents
the later clear/set. Completed prefixes are not rolled back.

Defeat calls `battlefunctions/battlefunctions_1.asm:UpdateBattleUnlockedFlag`,
clears DIALOGUE_NAME_INDEX_1, requests sad music/text363, closes text, restores
Bowie's HP maximum, halves unsigned gold, calls GetEgressPositionForBattle and
returns d4=-1. Battle04's special does not apply. Admitted Egress deducts spell
MP; Angel Wing removes its selected item slot. Their shared text/sound/flash
tail updates unlock, unwinds the action frame, obtains Egress coordinates and
returns d4=0. Escape does not halve gold or perform defeat's Bowie restoration.
Entered handlers do not prove menu availability or natural spell/item custody.

UpdateBattleUnlockedFlag preserves424 when524 is clear and clears424 when524
is set. Defeat/escape do not set524 or run victory's after/join; CheckBattle
may already have cleared424 on completed admission. Neither changes474.
GetEgressPositionForBattle has no24 special: it passes **EGRESS_MAP**, not prior
XYZ/facing, to `code/common/maps/egressinit.asm`. With399 set,
`data/maps/global/savepointmapcoords.asm` maps supplied6 to New Granseal
`(11,22)`, Up; supplied72 has no row and retains72 with `(1,1)`, Up. Other
selectors use their row or that default. No automatic New Granseal selector
follows from the warning text. Direct399-clear calls return initial Map3
`(56,3)`, Down before raft reset; BattleLoop sets399 on fresh/resumed entry.

With399/F64 set, `raftresetmapcoords.asm` resets the raft from the savepoint:
Polca/Bedoe select Map72 `(43,48)`, Hassan Map69 `(28,55)`, otherwise default
Map69 `(42,15)`, including supplied6/72. F64 clear preserves raft fields.
This is saved raft positioning, not boarding.

All outcomes reach `code/gameflow/mainloop.asm:alt_MainLoopEntry`: SwitchMap,
then ExplorationLoop, **without immediate CheckBattle** or a branch on d4's
sign. Map72 has no flag-switched replacement. Winning neither invokes the cave
callback nor immediately repeats the battle.

## Returned Map72 and later event admission

**Confirmed:** ordinary victory returns explicit72. In
`code/gameflow/exploration/explorationfunctions_2.asm`, ExplorationLoop clears
pending, subtracts20000 from word STEP_COUNTER/clamps a negative result to0,
heals eligible allies, fades out, sets current map72/current battle255, loads
tiles and rebuilds entities/mapsprites. It clears temporary F256–383 and sets80
before map load, base VInts and init. BattleLoop cleared PLAYER_TYPE at entry;
the empty outcome hooks do not replace it. Ordinary mode0 return is distinct
from supplied/direct mode1/2. Persistent424/474/524/story/party flags survive.

Map72 has only the default setup; `s1_entities.asm` is msEntitiesEnd.
`mapfunctions.asm:InitializeMapEntities` clears old battle actors/mappings,
constructs flag followers from physical1 upward, then main physical0. Empty
residents do not imply no followers. `followersfunctions_2.asm:sub_44404`
handles mode2 by changing main's sprite/standing followers and declaring the
separate raft at `(64,64)`; otherwise F64/current-map=RAFT_MAP can declare the
stored raft at physical31, lookup offset63, Left, coordinates masked by$7F.
This is distinct from FOLLOWER_B's lookup31. No raft is guaranteed.

`ms_map72_InitFunction` at $4FF2E has six operations: test716, return if clear;
test joined-Luke10, return if set; run accepted cs_4FF5A; return. Earlier Luke
recruitment remains an alternate supplied-history init. No750/805/524/items
replace this guard, and it does not invoke the missing-item scene. The palette
comparison conditionally requests map music/fade-in before WaitForEvent.
Successful load/init remains separate from usable-control evidence.

A no-battle CURRENT255 reload instead updates main/declares the raft and retains
current actors/temp flags; it still loads/runs init. Its raft consumer returns
for mode2, skips ordinary follower relocation for mode1, otherwise relocates
the current exploration follower list before the F64/map gate. Border scrolling
retains actors and skips init/temp reset. F88 suspension is a battle-entry
alternative, not fresh exploration; follow its supplied current state through
the outcome rather than assuming the fresh roster/area.

For a later produced event6, ProcessMapEvent clears pending type; its zone
consumer installs physical0's eas_Init and supplies coordinate words.
`mapsetupsfunctions_1.asm:RunMapSetupZoneEvent` compares low bytes and selects
the first matching row. The complete Map72 table is:

| Rows | Coordinate predicate | Callback |
| --- | --- | --- |
|0–2|X32,33,34; any Y|Map72_ZoneEvent0 → CheckRandomBattle26|
|3–8|X25..30 respectively; Y37|Map72_ZoneEvent3|
|9/default|$FD marker|F523 clear → CheckRandomBattle8; set →24|

X25..30 are coordinates, not table rows25..30. A declined cave/specific26
callback never falls through to default24. Accepted walking production needs
the actual masked layout kind$1400; pending can be written before a blocking
check. Coordinates alone do not prove a step or event. Numeric mode1 uses
eas_Raft/opcode7 and mode2 uses eas_Caravan/opcode8; neither is this walking
producer, despite the source names. A supplied event can still call the zone.

Reuse the accepted helper/admission boundary: F524 clear requests24 immediately;
set524/nonzero counter declines; set524/counter0 draws range8, zero requests,
nonzero draws range4+2 for a counter2–5 cooldown. Requests write424 and partial
longword type1/CURRENT255 plus counter30000 before flash services; residual
position/facing parameters survive. A later no-scroll consumption reaches the
ordinary CheckBattle: Map72 scans8/unlock408,24/424,26/426. First unlocked row
wins, not rectangle containment. Completion clears that selected unlock after
selection without continuing the scan. Thus408 can intercept a24 request and426
can remain for later. F523 chooses the default helper; F524 governs its repeat
cadence/upgrades. Postbattle bypass does not abolish these later paths.

## Cave guard, inventory predicate and branch boundary

**Confirmed:** complete `s3_zoneevents.asm:Map72_ZoneEvent3`,
$4FE98..$4FF06, has29 source operations:

| Block | Ordered work |
| --- | --- |
|Entry|chkFlg750/bne return_4FF04; chkFlg805/beq return_4FF04; chkFlg256/bne return_4FF04; clrFlg256.|
|Cannon test|moveq ITEM_CANNON,d1; jsr j_GetItemInventoryLocation; cmpi.w -1,d0; beq loc_4FEF4.|
|Dynamite test|moveq ITEM_DYNAMITE,d1; same lookup; cmpi.w -1,d0; beq loc_4FEE6.|
|Both found|moveq Cannon,d0; RemoveItemFromInventory; moveq Dynamite,d0; RemoveItemFromInventory; script cs_4FFDA; setFlg750; bra loc_4FEF2.|
|loc_4FEE6|move.w ITEM_DYNAMITE,DIALOGUE_NAME_INDEX_1; script cs_5023E.|
|loc_4FEF2|bra byte_4FF00.|
|loc_4FEF4|move.w ITEM_DYNAMITE,DIALOGUE_NAME_INDEX_1; script cs_5023E.|
|byte_4FF00 / return_4FF04|setFlg256; rts.|

Guards are750 first,805 second,256 third. No524/474/806, actual Astral, mode,
HP/class or party-capacity guard exists. Cannon absent skips Dynamite search;
both absent follows loc_4FEF4. Comparisons use full word$FFFF, not only a byte.
Both missing branches load Dynamite=$74 even if only Cannon=$72 is missing.
Inventory search is read-only; missing paths consume neither item/recruit nobody.

The selected `j_GetItemInventoryLocation` at$81D0 resolves to
`code/common/stats/itemstats.asm:GetItemInventoryLocation` at$9146, as joined
in accepted #969. Reuse that algorithm: UpdateForce includes joined IDs
ascending, including reserve/dead; count non-NOTHING entries across four slots,
then search **count-1 down to0**, comparing entry&$7F to the unmasked requested
word. Highest examined slot of the first matching member wins. Returned member
byte does not normalize the rest of d0; exhaustion writes d0.w/d1.w=-1. Gaps
can hide slot3 when count<4; unjoined carriers are excluded. Empty joined lists
lack a safe initial loop guard. Gift-history761/804 or follower identity do not
replace current inventory.

Both-found removal calls → cs_4FFDA → set750 → set256 is the **next handoff only**.
The caller does not check removal return values. This note neither audits
removal nor proves consumption/blast or describes Rohde's scene/join. Set750
follows that scene's return; missing paths never set750. Returning admitted
paths set256; early guards leave state unchanged.

## Current cast and complete missing-item program

**Confirmed:** `scripts.asm:142–183`, cs_5023E at $5023E..$50312, contains
40 map-script records/208 encoded bytes plus two inline records/4 bytes,
212 bytes total. A=FOLLOWER_A30, B=FOLLOWER_B31, P=ALLY_PETER7, Bowie=0 below.
These are logical indices, not physical slots or identity guarantees. This
table exhausts source through its terminator; opcodes are hexadecimal.

| Record(s) | Macro/operand order | Opcode / encoding |
| --- | --- | --- |
|0|textCursor3357|$04, word index;4 bytes|
|1–3|setActscriptWait P,Init; B,Init; A,Init|$15, actor/wait$FF bytes and pointer;8 each|
|4|setCamDest21,33|$32, X/Y words;6|
|5–8|setDest A+$8000,27,39; B+$8000,28,37; P+$8000,25,37; Bowie,26,37|$29, actor/X/Y words;8 each|
|9–11|waitIdle A; B; P|$16, actor word;4 each|
|12–14|setFacing Bowie,Up; P,Up; B,Up|$23, actor/direction bytes;4 each|
|15–18|csWait50; setFacing Bowie,Right; csWait30; setFacing B,Left|sleep$8032/2; $23/4; sleep$801E/2; $23/4|
|19–22|nod Bowie; shiver B; setFacing P,Right; csWait20|$26/4; $2A/4; $23/4; sleep$8014/2|
|23|nextSingleText$0,B →3357|$00, context word$001F;4|
|24–29|stopEntity Bowie; setSize Bowie,22; csWait10; setSize Bowie,20; csWait10; setSize Bowie,18|$1C/4; $50/6; $800A/2; $50/6; $800A/2; $50/6|
|30 + inline|entityActionsWait B; moveLeft1; endActions|$2D/B/$FF header4; bytes$02,$01; word$8080|
|31–33|setActscriptWait B,Jump; B,Jump; nextSingleText$0,B →3358|$15/8 twice; $00/4|
|34–35|setActscriptWait Bowie,Init; startEntity Bowie|$15/8; $1B/4|
|36–38|followEntity A,Bowie,2; P,A,1; B,A,3|$2C, three words;8 each|
|39|csc_end|$FFFF;2|

`data/scripting/entity/followers.asm` and InitializeFollowerEntities give complete
order: optional overworld65/A/Caravan, then66/Sarah,67/Peter,72/Kazin,73/Slade,
74/B/Astral,66/Chester,68/B/Oddler,75/Luke,69/C/Fairy,70/B/Astral,
74/A/King,71/Lemon,74/D/Minister,76/Zynk,77/A/Old Man,82/Frayja,
83/A/King,83/D/Minister,84/Rohde. Caravan priority0, ordinary rows1. Flags admit
rows independent of membership/HP. Enabled rows allocate contiguous physical
actors and append their physical IDs to EXPLORATION_ENTITIES. Later declarations
can overwrite a lookup while leaving prior actors/list entries. F805 does not
set up A/B/P or require65/67/70.

With only65/67/70 enabled on fresh Map72/mode0, A maps physical1 Caravan(sprite62),
P2 and B3 Astral(sprite209); main0. With67/70 but no65, A remains lookup0 and
aliases main; P1/B2 remain. Without relevant followers, A/B/P can all map0.
Later74/77/83 A declarations replace Caravan mapping; the last enabled B row
among74/68/70 selects Astral/Oddler/Astral. An earlier hidden$FF mapping is not
a safe absence sentinel in GetEntityAddressFromCharacter. The scene creates
no actors, checks no capacity and repairs no duplicate/self mappings.

GetEntityAddressFromCharacter masks to$FF, reads the lookup and scales by32.
Thus destination$8000 controls waiting, not identity. `getallymapsprite.asm`
selects Blue Flame for out-of-battle HP0; otherwise class/table logic gives
Bowie HERO1/SDMN0 and Peter PHNX19/PHNK18 from `data/stats/allies/allymapsprites.asm`.
Other supplied classes follow the shared algorithm. F67 can declare Peter
regardless of joined/active state; ordinary outcome healing restores immortal
Peter. CURRENT255 can retain changed sprites/mappings instead of these examples.

### Movement, animation and text consumers

**Confirmed:** csc15 installs the action pointer and writes the resolved physical
index into ACTSCRIPTWAITTIMER; its wait flag polls pointer==eas_Idle.
`data/scripting/entity/eas_actions.asm:eas_Init` sets acceleration factors0,
speed32/32, disables acceleration/deceleration, enables obstruction/collision
and auto-facing, faces Up, disables immersion, sets size24, refreshes sprite
and jumps Idle. It does not reset all position/destination/travel fields.
These calls have no HP guard. csc32 sets camera target-1, requests tile
destination `(21,33)` and waits scrolling.

csc29 scales word destinations by384 and writes travel distances and signed
velocities of magnitude32. Bit15 skips the position wait for A/B/P; Bowie's
unmarked command calls WaitForEntityToStopMoving, comparing both coordinates
against destinations with VInt retries. csc16 only polls Idle; it does not join
all three arrivals. Aliases can overwrite earlier destinations/scripts. Even
with distinct actors, waitIdle is not a completed-cast-settling observation.

csc23 guards its packed actor byte: dead Bowie/Peter can skip facing changes;
A/B bypass this HP test. csc26 nod is unguarded: animation counter-1, Sleep10,
transformed mapsprite/DMA, Sleep20, sprite update, Sleep10, counter0. csc2A
shiver saves global SPRITE_SIZE/animation counter, uses size21/counter-1,
performs three resize-flag on/off cycles with Sleep5 per half/sprite refresh,
then restores saved counter/global size. It leaves resize cleared rather than
restoring an arbitrary prior flag value.

stop/startEntity change animation counter (-1/0), not movement enablement;
their word-actor HP guard skips dead Bowie. Unguarded csc50 temporarily uses
each global size22/20/18, sets the actor's resize flag, updates sprite/waits
a VInt, then restores global size. It does not store permanent actor height18.
Final Bowie's Init applies size24/refresh before startEntity: requested
restoration, not observed presentation.

csc2D reads B as a byte, writes its physical index to the timer and masks
FLAGS_A with$9F. moveLeft1 compiles opcode4 relative displacement(-1,0), then
opcode1 wait-until-destination. endActions appends jump$34/eas_Idle; generated
scratch advances14 bytes (8 movement/wait +6 jump), distinct from the4-byte
source inline pair. ExecuteMapScript resets this scratch to FF9004_LOADING_SPACE.
esc04 adds `(-384,0)` to current position and installs destination/travel/velocity;
opcode1 waits arrival. Its shared other-entity obstruction check is bypassed
while FLAGS_A bit5 remains clear from the compiler mask. Nominal B28,37 →27,37
therefore assumes staging arrived before the relative move. entityActionsWait
polls generated-script Idle; incomplete action/movement services can stall.

Both eas_Jump installations disable auto-facing, request relative moves
`(0,-1)`, wait5, `(0,1)`, wait5, `(0,0)`, enable auto-facing and jump Idle.
ac_moveRel also encodes opcode4 and scales its offsets by384 from the then-current
position. Unlike the compiled moveLeft, these moves have fixed ac_wait5 delays
rather than inserted opcode1 destination joins; the final `(0,0)` is relative
to current position, not a saved starting point. Idle therefore proves neither
a rendered jump nor exact restoration of the initial position.

The literal `data/scripting/text/gamescript.txt` entries are:

- 3357/0D1D: `What?!  You forgot to bring{N}the {ITEM} with you?{N}{LEADER}, are you serious?{W2}`
- 3358/0D1E: `You fool!  Go back and{N}bring it here!{W1}`

Non-skipped csc00 uses context$001F to select B through
`getentityportaitandspeechsfx.asm` and the **current sprite** table
`data/spritedialogproperties.asm`: Astral209 → Astral portrait/bleep6,
Oddler213 → Oddler/bleep4, otherwise its actual row or default portrait/bleep6.
An open portrait can bypass portrait opening; speech lookup still occurs.
The command waits scrolling, displays cursor, increments it, closes portrait/
text and sleeps10. DisplayText resets the substitution pointer: ITEM consumes
INDEX_1=$74 through GetItemName → Dynamite. LEADER separately rebuilds force
and names its first active member, not necessarily mapped physical Bowie.
Changed/empty active lists therefore affect that name independently of B.

N is newline. W1 accepts held direction/A/B/C after a VInt wait, not Start;
W2 follows continuation/input and validation-sound handling. No YesNoPrompt,
warp, item transaction, map reload or automatic town trip occurs here.
ExecuteMapScript clears SKIP_CUTSCENE_TEXT; P2 Start with debug enabled can
latch it. Skipped csc00 advances operands without displaying **or incrementing**
cursor; csWait sleeps skip. Movement, nod/shiver's internal sleeps, action waits,
size changes and follows still execute. Skip is neither cancellation nor proof
of instantaneous completion. No explicit music/fade/flash command belongs to
the missing program; actual text/input/speech/sprite/DMA/VInt/scroll/action
service completion remains unobserved.

### Final following, latch and local return

**Confirmed:** csc2C first reads the **high byte of its follower word** into
the alive-status guard. All selected words ($001E,$0007,$001F) therefore test
Bowie0's HP, not Peter's or the mapped follower's. With Bowie HP0 they skip six
operand bytes and install nothing, after earlier unguarded scene changes.
With Bowie alive, actual mappings produce A→Bowie slot2 offset(-24,0), P→A
slot1 `(0,-24)`, B→A slot3 `(0,24)` from table_FollowerPositions: a branching
chain, not three direct followers of Bowie.

`entityfunctions_2.asm:AddFollower` copies42 bytes of eas_Follower1 into
ENTITY_WALKING_PARAMETERS, advances42 and writes target physical ID/offsets
at$1E/$20/$22. It installs the script even if the follower is already in
EXPLORATION_ENTITIES; only the append is deduplicated. Three admitted commands
allocate126 bytes without capacity guard or settling wait. Aliases can overwrite
an earlier install or self-follow; unrelated followers remain. Repeated direct
scene calls allocate again. Fresh ClearEntities resets the walking buffer to
FF5600_LOADING_SPACE; resetting map-script scratch does not reclaim it.

The terminator conditionally waits scrolling if dialogue is open and clears
VIEW_SCROLLING_SPEED. Only after cs_5023E returns does its zone caller set256;
750 stays clear. RunMapSetupZoneEvent closes portrait/text, waits a VInt and
joins physical0's **position**, not the entire cast. WaitForEvent restores
camera0/mode-selected controller only when pending is zero. Pending overwrites,
mode/mapping and unfinished services can change continuation. Local return
is not a universal usable-control endpoint.

F256 is a temporary encounter latch, not cave-cleared750. Ordinary movement
and this map's default/specific callbacks do not clear it. Moving away/back or
acquiring an item without fresh entry does not alone re-admit the callback.
CURRENT255 no-battle reload retains it; fresh explicit entry clears256–383.
With750 clear/805 set, fresh entry can retry current inventory. Set750 suppresses
even after temporary reset. Direct cs_5023E bypasses guards and itself writes
neither256/750 nor the item-name word; ITEM can inherit another index. Interruption
before the caller's set256 leaves the latch clear and partial actor changes,
not rollback.

## Changed-state acceptance and reproduction

| Supplied change / interruption | Required distinction |
| --- | --- |
|Bishop128 dies; another counted enemy survives|Empty hook/no leader append; battle can continue.|
|All enemies uncounted; Bowie HP0/no counted allies|Defeat wins the simultaneous check.|
|524 already set|Skip empty dispatch; leader byte stays0; victory still joins Bowie/clears424/sets524; defeat/escape clear424.|
|Different first active/dead member, area or mapping|Victory seed follows live state, not a fixed cave position.|
|408/426 retained after victory|No immediate scan; a later no-scroll return can select another row.|
|750 set,805 clear or256 set|Ordered early return; no lookup/name change/scene/default fallthrough.|
|Cannon absent, Dynamite present; both absent|First-search failure, Dynamite name, same scene and later256.|
|Cannon present, Dynamite absent|Second-search failure; no Cannon consumption.|
|Reserve/dead joined carrier, unjoined carrier or gapped slot3|Joined/count-limited search controls presence; gift flags do not.|
|65 absent/conflicting A/B flags/missing or hidden mappings|Actual cast/aliases differ;805 does not repair it.|
|Peter dead/Bowie alive|Peter facing may skip; Init/destination/follow admission does not universally test Peter HP.|
|Bowie dead at direct scene call|Facing/animation/follows may skip; destinations/nod/shiver/size/actions still execute.|
|Repeated direct scene or manually cleared256|No scene-owned guard; follower-buffer copies accumulate, with no unlimited-capacity claim.|
|P2 debug text skip|No warning/cursor advance/csWait delay; other work and caller latch still depend on completion.|
|Interrupted scene/after wrapper/position wait|Preserve reached prefix/pending; no later flag/control claim.|
|Fresh72 versus CURRENT255/scroll|Fresh population/temp reset versus retained actors/latch; reload runs init, scroll does not.|

Reproduce from the owning checkout after loading ignored `local/private-inputs.ps1`
in the same process. Pass the registered upstream read-only and verify clean
HEAD. Reuse the [retained-pair recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init equality, _verify_map_script_engine_fixture and _cutscene_facts, with
size/mtime unchanged. No extraction regeneration is necessary.

1. Join end/after slot24, leader24, after-join24, empty target and include/wrapper
   consumers. Join accepted roster to actual end checks, dead-list/counting,
   victory membership/mapping seed, defeat/Egress, savepoint/raft tables and
   alt_MainLoopEntry. Preserve completed/suspended/supplied-state differences.
2. Join Map72 setup/empty residents/init, complete zone rows and all three
   bodies to map-init and map-events routing/zone fixtures. Compare every
   selected operation, guard polarity, sentinel, target and write. Reuse #969's
   inventory algorithm and #977's request/cooldown/admission consumers.
3. Select programCorpus.programs id cs_5023E from retained script payload.
   Compare all40 records against source142–183; include inline moveLeft/endActions
   in the212-byte extent. Join macros/dispatch, action templates, declarations/
   mappings/HP/class, text/control/name/context and scratch/list consumers.
4. Review changed states and every new claim against its direct consumer.
   Aggregate equality guards its corpus, not every prose interpretation.
   Local direct-check expectations were corrected for literal dc.w $FFFF
   versus macro spelling, and zone sourceMnemonic/operandTexts versus map-init
   fields; retain original completed receipts. Direct macro review also corrected
   the draft jump interpretation: ac_moveRel uses the same opcode4, without
   moveLeft's inserted arrival wait; the compiler mask bypasses entity obstruction.
5. Check prose/diff, links, UTF-8, sole-path/private boundary and `git diff --check`.
   On the clean committed head run `uv run sf2 verify plan --base origin/main --head HEAD`;
   record actual CI/exact identities in the Issue handoff. Documentation-only
   scope does not launch blanket normal/full/engine/Godot/H3/H4 suites.

**Unknown:** natural outcome/route/layout/event production; supplied flags,
inventory/gaps, membership, HP/class/mode/current actors/buffer capacity;
completed input/text/sound/palette/DMA/action/follower/scroll services; visible
cast settling, usable control and save/load persistence. These are separate
from the Confirmed static conditional behavior and do not authorize acquisition
or the excluded both-item scene.

Preserve [#977 correction handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/977#issuecomment-6180317175)
and [PR979 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/979#issuecomment-6180326745):
final1a58b934/tree583dfb93/base27bedb07/CI38650800988 (scope12s/research29s),
original1b4b1409/CI38650285331, intermediate32f9c13d/CI38650658600, both distinct
0.080s pair receipts, independently found Map73 south Map69→68 P2 and reproduced
correction. Preserve linked #973/#969/#965/#954/#962 results/failures/Unknowns.
Issue981 preflight's guessed Battle23 owner was resolved to the actual linked
owner. Narrowed reads supersede truncated inspection output, not completed
evidence. Leave a clean pushed Draft PR, frozen writer/completed processes and
Project Review / Verify for independent main-gate acceptance. No merge, cleanup,
archive or next Issue is authorized.
