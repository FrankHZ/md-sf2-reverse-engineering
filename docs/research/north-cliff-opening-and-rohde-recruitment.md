# North Cliff opening and Rohde recruitment

Evidence date: 2026-10-14. **Confirmed:** the admitted Map72 cave callback
attempts Cannon removal, then Dynamite removal, without testing either result.
Its scene requests the cannon blast, copies the entrance layout, joins Rohde,
hides the newly allocated actors and installs a branching follower chain before
unlock425; only scene return reaches caller750 then256. **Inferred:** the usual
cast depicts Rohde opening the North Cliff passage and joining with the cannon.
**Unknown:** natural reach, supplied state, completed presentation/control and
save persistence. These are conditional static findings, not runtime acceptance.

## Agreement and provenance

Standard rigor for [Issue #985](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/985)
and [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
item custody/removal, scene actors, blast/map mutation, membership, caller flags
and local return are separate effects. Accepted base is
`e96f6faac779a3624506f900109ad974332cf707`. This document is the sole owned path;
no shared registries, schemas, fixtures, tools, design, engine or UI changes.
The concurrent #986 synthesis consumes accepted #981 independently.

Consume [Battle24 outcomes and cave preparation](./battle24-outcomes-through-north-cliff-preparation.md),
[Battle24 admission](./new-granseal-departure-through-battle24-admission.md),
[inventor and inventory search](./new-granseal-inventor-and-dynamite-gift.md),
[item transaction seams](./map-event-item-transactions.md),
[Caravan consumers](./caravan-boarding-and-southwest-parmecia-travel.md),
[request state](./map-event-request-state.md) and
[request consumption](./map-event-request-consumption.md).
Stop after complete cs_4FFDA, its caller/local return and the fresh Map72
consequences needed to explain it. The cave entrance warp is a next-route row
only; no cave interior, Battle25 admission/roster, later journey/combat,
runtime, acquisition, regeneration or installation is authorized.

Paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The retained read-only checkout is clean at that pin. Retained payloads/fixtures
identify USA retail SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Constants belong to `sf2enums.asm`, RAM to `sf2const.asm`, command encodings to
`sf2cutscenemacros.asm`. Fnnn means flag nnn; indices are zero-based; extents
are half-open. Map-script consumers are `code/common/scripting/map/`, entity
consumers `code/common/scripting/entity/`, stats `code/common/stats/`.

Minimum evidence is selected source, existing fixtures and the retained
18,285,979-byte init/script pair. Its read-only linear comparison is budgeted
under one minute/512 MiB, with small receipts and prose; this comparison took
0.082s with unchanged size/mtime. No new maintained checker or acquisition.
Replan material contradictions or supporting-tool expansion. Remaining Unknowns
are not an automatic runtime queue.

## Admission and actual removal

**Confirmed:** accepted `data/maps/entries/map72/mapsetups/s3_zoneevents.asm`
contains Map72_ZoneEvent3 at $4FE98..$4FF06. The specific cave rows are X25..30,
Y37, before the default random-battle callback. Ordered guards return for750 set,
805 clear or256 set. Otherwise clr256 precedes Cannon=$72 and Dynamite=$74
searches through j_GetItemInventoryLocation. Full-word d0==$FFFF selects a
missing branch; both missing branches name Dynamite, run cs_5023E and set256.
They do not remove either item. No524, HP, class, membership-capacity or actual
Astral guard supplements this admission; the selected callback does not fall
through to default on rejection. Actual zone-event production remains conditional
on the accepted walking/layout/request consumers, not coordinates alone.

With both searches successful, the exact sequence is Cannon in d0 →
RemoveItemFromInventory at callsite$4FECC; Dynamite in d0 → the same service
at$4FED4; cs_4FFDA; set750; branch to set256; rts. The caller ignores both
removal return values and never passes the earlier search's holder/slot forward.

The selected include chain is `layout/sf2-07-0x044000-0x064000.asm` →
`code/common/stats/iteminventory.asm:RemoveItemFromInventory` ($4F542); section02 selects
`itemstats.asm` and `battleparty.asm`. The similarly named
`stats/items/itemfunctions_s7_0.asm` is not the selected implementation.
The existing `tests/fixtures/h2/map-event-item-transactions-static-v1.json`
service entries/callsites anchor these aliases and addresses; callee semantics
come from the actual selected bodies, not that caller fixture alone.

1. RemoveItemFromInventory saves d1.w/d2.w, moves the requested d0.w to d1,
   calls j_GetItemInventoryLocation ($81D0 → GetItemInventoryLocation$9146),
   and returns d0.w=-1 if the full-word search fails. Search rebuilds joined
   force IDs ascending, including reserves/dead members. For each member it
   counts non-NOTHING across four slots, then examines count-1 down to0, comparing
   entry&$7F with the unmasked requested word. Thus gaps can hide high slots;
   an unjoined carrier is excluded; an empty joined list has no safe initial
   loop guard. The first matching member/highest examined slot wins. The holder
   is returned in d0.b (upper bits are not normalized), slot in d1.w.
2. A found result calls j_RemoveItemBySlot ($819C → RemoveItemBySlot$8E76 in itemstats.asm).
   It saves holder d0/a0, resolves the combatant, doubles the slot offset,
   reads the full item word into d1 and checks d1&$7F against NOTHING=$7F.
   NOTHING returns d2.w=3; otherwise RemoveAndArrangeItems shifts every following
   **whole word** left and fills the final slot with NOTHING, returning d2.w=0.
   Slot3 simply becomes NOTHING. There is no cursed-item/equippability veto.
3. Both paths restore holder d0/a0 and tail-call UpdateCombatantStats. That
   recalculates current stats from base/status and the remaining equipped items,
   including curse recomputation. Shifted item words retain equipped/broken
   bits; this is not a separate unequip/transfer to Rohde. Even the NOTHING path
   invokes the stat update. The wrapper maps d2==3 to d0.w=-1, otherwise0, and
   restores its saved d1.w/d2.w, so it does not return the removed holder/slot.

For unchanged packed inventories and valid force state, the fresh searches
remove one matching Cannon and one matching Dynamite, potentially from different
reserve/dead carriers. There is no uniqueness assertion: duplicate copies remain.
The second removal searches again **after** Cannon compaction. Changed inventory
between predicate and removal, a newly missing item, or a partial interrupted
prefix does not stop the later scene when the calls return. No transaction,
rollback, repair, atomic pair consumption or automatic reacquisition exists here.
The later narration naming LEADER as donor does not prove either actual carrier.

## Current cast and new physical entities

**Confirmed:** use A=FOLLOWER_A30, B=FOLLOWER_B31, P=Peter7, R=Rohde11,
O=object character128 and E=effect character129. These are character handles,
not necessarily physical indices. The scene does not load/reset Map72 or repair
its preexisting Bowie/A/B/P mappings. Under the ordinary selected overworld
followers65/67/70 and no conflicting follower flags, physical0 is the main
character, A maps to Caravan1, P to2, B to Astral3. Other enabled rows can replace
A/B mappings or add actors; 805 is not a guarantee of that cast.

GetEntityAddressFromCharacter masks to$FF, subtracts96 for enemy-bit handles
(128→lookup32,129→33), reads a physical byte and scales it by32. Missing entries
cleared to0 alias physical0; hidden entries$FF are not a safe null rejection.
Current mappings can alias one another, so later commands can overwrite the same
actor's script, destination or facing. Lookup31 is B, distinct from the separate
raft lookup63/physical31.

Commands1–3 always call csc2B/InitializeNewEntity for R/O/E at `(63,63)`;
there is no joined, existing-mapping, HP or capacity guard. The allocator scans
lookup entries0..62 for the largest **signed** physical byte, adds one, replaces
the selected handle's mapping, and declares that physical entry. Lookup63's raft
is outside this scan. Existing R/O/E mappings can therefore be replaced while old
physical entries remain; no deduplication, list insertion or reclaim is implied.
With the ordinary maximum3, the new physical indices are4/5/6. Additional or
malformed mappings change this; there is no unlimited repeated-call capacity claim.

DeclareNewEntity sets position/destination in units of384 per tile, zeroes
velocities/travel/acceleration, installs eas_Init, initializes flags and timer,
and requests property refresh. For nonnegative R, InitializeNewEntity resolves
GetAllyMapsprite regardless of the supplied sprite operand11. Outside battle,
HP0 selects Blue Flame first; otherwise joined11 clear selects NPC_Rohde170.
Joined11 set uses the actual class/table (Rohde base table26; BRGN22 yields26).
Other supplied classes follow that algorithm. O/E retain explicit sprites181
(OBJECT4) and178 (EFFECT3). Join itself later does not refresh R's current sprite;
subsequent dialogue still uses the sprite already present.

The asynchronous new eas_Init can precede later replacement scripts. Init sets
speed32/32, acceleration factors0, acceleration/deceleration off,
obstruction/collision and auto-facing on, Up, immersion off, size24, sprite
refresh then Idle. It does not establish every travel field for an existing
actor. Commands4–6 explicitly join P/B/A Init, while O/E scripts are replaced
by DeactivateAutoFacing (auto-facing off then Idle) after stopEntity. R later
receives eas_4536C. Actual entity scheduling and sprite service completion are
not observed by this source audit.

## Complete scene program

**Confirmed:** retained `programCorpus.programs[id=cs_4FFDA]` and
`data/maps/entries/map72/mapsetups/scripts.asm:4–141` agree on114 top-level
records, including the terminator. They encode558 bytes, plus42 bytes for the
nine inline action bodies (12 movements +9 endActions), plus12 for custom
speed/jump/ac_end: total612 bytes, $4FFDA..$5023E. The next symbol is the excluded
missing-item program cs_5023E. No branch or yes/no command occurs in this program.

The following ranges cover every top-level record exactly once. `wait action`
means entityActionsWait; `action` means the nonwaiting form. Directions and
counts in braces are the complete inline bodies, each followed by endActions.
`textN` refers to the literal table below, not an additional operation.

| Indices | Ordered commands and operands |
| --- | --- |
|0–3|textCursor3341; newEntity R(63,63,Up,R); O(63,63,Left,OBJECT4); E(63,63,Left,EFFECT3).|
|4–10|setActscriptWait P Init; B Init; A Init; stop O; stop E; setActscriptWait O DeactivateAutoFacing; E DeactivateAutoFacing.|
|11–17|setCamDest(21,33); setDest A+$8000(27,39), B+$8000(28,37), P+$8000(25,37), Bowie(26,37); csWait60; setPos O(27,36,Left).|
|18–24|face Bowie Up, P Up, B Up; csWait50; face B Down; nextSingleText3341(B),3342(R).|
|25–31|setPos R(27,39,Right); setActscriptWait R eas_4536C; csWait3; face R Up; nextText3343(B); nextSingleText3344(B),3345(R).|
|32–38|wait action R{Up1,Left2}; face Bowie Down, P Down, R Up; nod Bowie; nextSingleText3346(no speaker); wait action R{Right1,Up1}.|
|39–47|face Bowie Up,P Up,B Up; nextSingleText3347(R); setCamDest(21,32); setActscriptWait O BumpDown; setPos E(27,35,Left); customActscriptWait E{ac_setSpeed80,80; ac_jump eas_Idle; ac_end}; wait action E{Up2}.|
|48–57|setQuake3; setSprite E FLAME1; start E; setActscript E AnimSpeedx2; playSound BATTLEFIELD_DEATH; csWait40; setBlocks(0,0,1,2,27,33); hide E; setQuake0; csWait60.|
|58–62|wait action B{Up1}; nextSingleText3348(B); csWait40; setActscriptWait R Jump; nextSingleText3349(R).|
|63–68|face P Right,Bowie Right,B Left; setActscript P JumpLeft; Bowie JumpLeft; setActscriptWait B JumpRight.|
|69–80|setPos R(26,36,Right), wait25; R(27,37,Up), wait25; R(28,36,Left), wait25; R(27,37,Up), wait25; R(26,36,Right), wait25; R(27,37,Up), wait25.|
|81–87|nextSingleText3350(R); face R Left; nextText3351(R),3352(R); nextSingleText3353(R); join R; nextSingleText3354(R).|
|88–94|setActscriptWait O DeactivateAutoFacing; action O{Down2}; wait action R{Down1}; action R{Down1}; csWait3; setActscript R eas_452BA; csWait3.|
|95–101|action O{Down1}; csWait3; setActscript O eas_452BA; csWait3; hide R; hide O; wait action B{Down1,Left2}.|
|102–108|nextSingleText3355(B); face B Down; nextSingleText3356(B); face Bowie Down,P Down; setActscript Bowie Jump; setActscriptWait P Jump.|
|109–113|follow A→Bowie slot2; P→A slot1; B→A slot3; setStoryFlag25; csc_end.|

## Movement, blast and withdrawal consumers

**Confirmed:** csc15 and custom csc14 install pointers and write the resolved
physical index into ACTSCRIPTWAITTIMER. Waiting forms poll pointer==eas_Idle;
nonwaiting forms continue immediately. csc14 then scans the inline words to
$8080/ac_end. No HP guard protects these installations. They do not universally
wait for coordinates or rendered animation. stop/startEntity change the animation
counter (-1/0), not movement; their full-word guard bypasses HP for O/E.

csc29 writes tile destinations, travel and signed velocity magnitude32. The
$8000 bit skips the position wait for A/B/P; Bowie alone calls
WaitForEntityToStopMoving (both coordinates equal destinations with VInt retries).
csWait60 is a fixed script sleep, not a join of the other three arrivals.
csc32 sets camera target-1 and requests/waits its destination. Aliased actors
can overwrite earlier destinations, including before Bowie's wait.

csc19 setPos writes X/Y and X/Y destinations and facing, then refreshes sprite;
it does not clear all velocity/travel fields. Its packed actor-byte alive guard
can skip R while dead; object128/effect129 bypass it. csc23 similarly guards
Bowie/P/R facing but not B. Nod's csc26 is unguarded: counter-1, Sleep10,
transformed sprite/DMA, Sleep20, refresh, Sleep10, counter0. Thus a dead named
actor can skip some commands while still receiving unguarded actions/nods/join.

csc2D compiles each cardinal movement into opcode4 relative displacement and
opcode1 destination wait (8 generated bytes), then appends opcode$34/eas_Idle
(6 bytes per body). The nine bodies allocate150 bytes at the advancing
FF9004_LOADING_SPACE scratch cursor, distinct from their42 encoded source bytes.
All are unguarded. The compiler clears FLAGS_A bits5/6 with$9F, bypassing the
shared other-entity destination obstruction check while bit5 stays clear and
clearing map collision; opcode4 scales displacement by384 from **current**
position. Waiting bodies poll Idle after their destination waits; nonwaiting
bodies can be overwritten before completing. ExecuteMapScript resets this scratch
cursor on entry, not the separate follower buffer.

`vehiclefunctions.asm:eas_4536C` requests one tile along current facing with
ac_moveFacRelPos(0,1)/opcode9, enables resizing, requests SFX_WARP, then sets
sizes12,14,16,18,20,22, each followed by sprite refresh/wait1. It disables resizing,
sets global size16, refreshes, waits destination and branches Idle. Given the
admitted living R setPos(27,39,Right), this requests(28,39); after full completion,
Up1/Left2 requests(26,38), then Right1/Up1 requests(27,37). These are conditional
coordinate derivations; a skipped setPos or changed facing yields another route.
The size opcode writes global SPRITE_SIZE, not permanent per-actor dimensions.

O is positioned(27,36,Left). `eas_BumpDown` disables auto-facing, requests relative
(0,1), waits5, then(0,-1), waits5, then(0,0), restores auto-facing and jumps Idle.
The final zero displacement is relative to the then-current position, not a
saved original coordinate. Bump/Jump's timed waits do not prove full recoil or
exact restoration. Ordinary Jump uses(0,-1)/(0,1) with waits5; JumpLeft uses
(-1,-1)/(-1,1), JumpRight(1,-1)/(1,1), with waits6; each ends relative(0,0),
auto-facing on, Idle. Their opcode4 can also encounter the shared obstruction
logic according to the actor's current flags. Waiting B's JumpRight does not
join the nonwaiting P/Bowie JumpLeft animations.

E is set(27,35,Left); the custom action writes packed speed bytes80/80 then
jumps Idle (4+6 bytes, followed by the2-byte inline scanner terminator). The
following waiting Up2 body supplies an actual destination wait to(27,33) under
that starting position. It does not use a timed sleep as projectile arrival.
setQuake3 writes QUAKE_AMPLITUDE directly (neither gradual-mode bit is set);
setSprite changes E to FLAME1=171, waits a VInt and refreshes. start enables
animation counter0; the nonwaiting AnimSpeedx2 script sets its flag then Idle.
SFX_BATTLEFIELD_DEATH=116 is requested; csWait40 precedes the map copy, followed
by hide E, quake0 and wait60. These are resource/state requests, not evidence
that sound, flame, quake, sprite upload or the blast was presented successfully.
No explicit map fade or palette-flash command appears in this scene.

csc34 packs `(0,0)`, `(1,2)`, `(27,33)` into the three words passed to
`code/gameflow/exploration/exploration.asm:CopyMapBlocks`. It copies the current
RAM layout words at source(0,0)/(0,1) to destination(27,33)/(27,34), using64-column
word rows (128 bytes). This is a1-wide,2-high **layout-cell** copy, not direct
VRAM tiles, ROM mutation or proof of the exact artwork/collision contents.
csc34 then sets both view-plane update bits. Repeated calls copy the then-current
source cells; there is no rollback or content-identity test. Set750 is later,
so live map mutation and the persistent reconstruction predicate are separate.

After dialogue/join, the nonwaiting O Down2 overlaps R's waiting Down1, then
nonwaiting R Down1. Later O Down1 can replace its still-running Down2; csWait3
spacers do not establish arrival. The installs of `eas_452BA` on R and O are
also nonwaiting. That script waits6, enables resize, requests SFX_WARP, steps
global sizes22,20,18,16,14 (refresh/wait1 each), then12/refresh, waits destination,
clones position/destination/travel/facing from **physical ENTITY_CARAVAN=1**,
and branches Idle. It does not resolve logical FOLLOWER_A, and does not disable
resize. Altered physical1 can therefore change this effect independently of A.
There is no final join of either shrink script before hide. A hide can occur
while an action remains pending. Hide retains its pointer, but
entityscriptengine_2.asm:VInt_UpdateEntities skips both UpdateEntityData and
action dispatch when signed X >= $7000. The hidden actor therefore does not
continue shrinking/cloning under that ordinary update until another operation
makes its position eligible again. Retaining a pointer is not continued execution.
No exact shrinking, settled position, completed departure or restored global size
is promised by this sequence.

HideEntity sets the resolved physical X/Y and destinations to$7000/$7000,
changes **all** matching entries in the64-byte character lookup to$FF, and removes
the first matching physical entry from EXPLORATION_ENTITIES by shifting through
its sentinel. It does not clear actscript, travel/velocity, combatant HP,
membership, inventory or follower-buffer allocation. Thus aliases share the hide;
old entities orphaned by a new mapping are a different matter. E hides at55,
R/O at99/100. Later scripts using their hidden handles have no safe-null guard.

## Literal dialogue and recruitment

**Confirmed:** the complete ordinary cursor sequence is3341–3356, from
`data/scripting/text/gamescript.txt` keys0D0D–0D1C. All speaker contexts are$00
plus actor (B=$001F, R=$000B), except3346=$FFFF/no speaker. There is no ITEM
substitution; `{NAME;11}` explicitly selects Rohde's current name, while LEADER
rebuilds force and names its first active member, not the actual inventory donor
or necessarily mapped physical Bowie.

| ID | Speaker/form | Literal text |
| --- | --- | --- |
|3341|B/single|`{NAME;11}, {NAME;11}!{W1}`|
|3342|R/single|`Yes?{W1}`|
|3343|B/next|`You know about devices like{N}this, right?{W2}`|
|3344|B/single|`{LEADER} has a cannon and{N}some dynamite here.  Can you{N}remove those rocks?{W1}`|
|3345|R/single|`Ah...OK, let me try.{W1}`|
|3346|none/single|`{NAME;11} gets the dynamite{N}from {LEADER}.{W1}`|
|3347|R/single|`Stand back.  Ready?{W1}`|
|3348|B/single|`Perfect!  We can now go to{N}North Parmecia.{W1}`|
|3349|R/single|`Wow!  This cannon is great!{W1}`|
|3350|R/single|`I can attend the battles{N}with this weapon.{W1}`|
|3351|R/next|`{LEADER}, do you need{N}this any more?{W2}`|
|3352|R/next|`May I use it?  Huh?{W2}`|
|3353|R/single|`Wow, now I can fight along{N}side you!{W1}`|
|3354|R/single|`Please call me when you{N}need my help.{W1}`|
|3355|B/single|`{NAME;11} is hooked.{N}Ha, ha!{W1}`|
|3356|B/single|`OK, {LEADER}.  Let's go{N}to North Parmecia!{W1}`|

csc00/csc02 wait scrolling, display the cursor and increment it. The single
form also closes portrait/text and sleeps10; nextText leaves them open. Context
selects portrait/speech from the **current mapsprite**, via
GetEntityPortaitAndSpeechSfx and `data/spritedialogproperties.asm`: NPC_Rohde170
and RohdeSpecial26 select Rohde/bleep5; Astral209 selects Astral/bleep6; alternate
B or R sprites select their actual row/default. An already-open portrait may
bypass opening without bypassing speech lookup. Narration uses speech0. N is
newline; W1's wait accepts held directions/A/B/C after a VInt, not Start; W2
uses the continuation/input and validation-sound path. These waits advance
text; they are not a response to Rohde's questions. No YesNoPrompt or branch
on a player answer is present.

At86, csc08 waits scrolling, clears speech, reads R11 (bit15 clear → MUSIC_JOIN),
then j_JoinForce → battleparty.asm:JoinForce. It sets joined11 and rebuilds
force lists; if active count is below FORCE_MAX_SIZE12, it sets active43 through
JoinBattleParty. At12 or more, it skips that write. Existing active43 is not
cleared, so an already-active Rohde stays active even in an overfull supplied
state; already-joined reserve Rohde can become active if room exists. The helper
does not test HP/class, revive/heal, initialize inventory/class/equipment,
select a replacement member, transfer Cannon, set84 or alter current entity
mappings. UpdateForce precedes the active write; the helper does not rebuild
lists again afterward. A later UpdateForce refreshes those cached lists.

csc08 then gets R's actual class, puts11/class into DIALOGUE_NAME_INDEX_1/2,
and displays446/01BE: `{NAME} the {CLASS} {N}has joined the force.` It calls
j_FadeOut_WaitForP1Input: the selected body requests previous music after the
current one and waits player input (held directions/A/B/C/Start through CURRENT_PLAYER_INPUT),
rather than a map fade. This is not a new-edge or yes/no selection wait. It closes text,
sleeps10 and returns. This announcement/wait also occurs on repeated joins;
no already-joined suppression is present. The ordinary cursor remains3354.
Join announcement completion, membership write and future active-party use are
separate boundaries; dead/full/already-joined states do not justify a different
invented prompt or equipment transaction.

ExecuteMapScript clears SKIP_CUTSCENE_TEXT on entry. Debug-enabled P2 Start can
latch it: csc00/csc02 skip operands without displaying **or incrementing** cursor,
and csWait sleeps are skipped. csc08 has no such skip check: music, membership,
announcement446 and its input/Sleep10 still run. Internal nod/shrink/jump waits,
movement/destination/Idle waits, map mutation, follows and flags are not canceled.
Skipping csWait3 particularly cannot establish shrink completion. Partial text
skip can leave the cursor behind the nominal sequence; it is not a speed-only
presentation change or a universal skip-to-end operation.

## Final following, caller return and fresh Map72

**Confirmed:** the final csc2C guard reads the **high byte of its follower word**.
A=$001E/P=$0007/B=$001F all therefore test Bowie0 HP, not the actual follower.
Dead Bowie skips all three six-byte operand groups. With Bowie alive, commands
install A→Bowie slot2 offset(-24,0), P→A slot1(0,-24), B→A slot3(0,24), using
current physical mappings. This is a branching chain. Aliases can overwrite
prior scripts or self-follow; unrelated followers are not removed.

AddFollower always copies42 bytes of eas_Follower1 into ENTITY_WALKING_PARAMETERS,
advances42 and writes physical target/offsets at$1E/$20/$22. Only the exploration
list append is deduplicated; reinstalling an existing follower still consumes
buffer space. Three admitted commands allocate126 bytes without a capacity guard
or settling wait. This buffer starts at FF5600_LOADING_SPACE after ClearEntities;
ExecuteMapScript's separate action scratch reset does not reclaim it. Hiding
actors removes list entries but does not rewind either allocator. Repeated direct
scene calls therefore need explicit mapping/capacity assumptions, even though
membership and flags can already be set.

csc13 translates setStoryFlag25 by BATTLE_UNLOCKED_FLAGS_START400 to425. It sets
that flag only after the reached follows (or their dead-Bowie skips); it neither
starts Battle25 nor sets completion525. The terminator conditionally waits for
scrolling if a dialogue window is open, clears VIEW_SCROLLING_SPEED and restores
registers. Only then does Map72_ZoneEvent3 set750, then256 and return.

The zone wrapper closes portrait/text, waits a VInt, and joins physical0's
**position**, not every current actor/action. ProcessMapEvent has already cleared
the pending event type before dispatch. WaitForEvent restores camera0 and the
mode-selected controller only when pending is zero; pending overwrite and mode
can change continuation. A completed script is therefore not a demonstrated
usable-control endpoint. No save or persistence operation belongs to this scene
or its selected caller tail.

The ordering allows partial states: removed items before any actor allocation;
quake/flame before the map copy; copied layout before membership; joined11/possibly
active43 before withdrawals; unlock425 before caller750;750 before256. Interrupted
prefixes retain whatever has executed. In particular, the layout can be changed
with750 still clear, membership can be set while the event remains unlatched,
and750 can suppress later cave callbacks even if256 was never written. Neither
caller nor scene rolls these effects back or repairs a partial prefix.

### Reloaded layout and population

**Confirmed:** `data/maps/entries/map72/3-flag-events.asm`, $C1BF0..$C1BFA,
contains the single F750 block-copy record: source(0,0), size(1,2), dest(27,33),
then endWord. `code/common/maps/mapload.asm:LoadMapBlocksAndLayout` first loads
blocks/layout, then walks8-byte flag records; when750 is set it calls the same
CopyMapBlocks, otherwise skips the record. This reconstructs the two destination
layout words from the **newly loaded** source cells. The live csc34 instead reads
the **current mutable** source cells and explicitly sets both view update bits.
The fresh flag loader does not use those csc34 view-bit writes; normal LoadMap
continues its own display/plane initialization. Same coordinates do not prove
identical source contents after arbitrary live map edits.

Fresh explicit Map72 entry clears temporary256–383, clears current entities and
lookup/buffers, then repopulates followers/main/residents. Map72 has only its
default setup and an empty s1_entities resident list; it does not freshly declare
the scene O/E. Joined11 alone does **not** declare a Rohde follower. The actual
`data/scripting/entity/followers.asm` final row is `follower84, ALLY_ROHDE, $FF,1`;
InitializeFollowerEntities tests84, not11 or750/425. With84 set it declares R
using current HP/class/joined sprite selection even when supplied11 is clear;
with84 clear, joined Rohde is absent from this fresh follower population. This
scene/join/caller never sets or clears84.

F65 on an overworld map chooses table_OverworldFollowers, whose Caravan/A row
falls through the ordinary table; other rows (including67 Peter,70 Astral/B and84
Rohde) are tested in declaration order. Conflicting enabled A/B rows can override
lookups; membership does not replace their story guards. Physical1 onward/list
order and installed scripts follow the enabled rows; main physical0 is initialized
separately. The raft/mode exceptions remain as documented in the accepted Map72
owner. No arbitrary enabled-row count or action-buffer capacity is validated here.

Map72 init still tests716 then joined-Luke10, invoking cs_4FF5A only for716 set/
10 clear; it does not replay this blast or use750 as an actor-repair flag. An
ordinary no-battle CURRENT255 reload loads layout/init while retaining current
actors and temporary256; border scrolling retains actors/latch and skips init/
temporary reset. Thus a scroll, CURRENT reload and fresh explicit entry do not
share one lifecycle. Ordinary movement/acquisition does not by itself clear256.
Fresh entry clears256 but retained750 suppresses the cave callback. If750 remains
clear after interruption, a fresh entry may permit retry against **current**
remaining inventory; it does not restore consumed items or guarantee success.

`data/maps/entries/map72/6-warp-events.asm` first row at$C1BFE names
`mWarp27,33; warpNoScroll; warpMap MAP_DARK_CAVES48; warpDest55,29; warpFacingUp`.
That is the next static route only. This scene neither directly warps there nor
proves natural arrival/event production after the copied cells. The cave
interior, Battle25 admission/roster and later journey remain outside this slice.

## Changed-state review and reproduction

| Supplied change or interruption | Required conditional result |
| --- | --- |
|750 set /805 clear /256 set|Ordered caller early return; no inventory lookup/removal/scene; a direct scene call bypasses all three.|
|Either item absent during predicates|Accepted missing-item branch, no consumption; current custody differs from gift flags761/804.|
|Multiple/dead/reserve/unjoined holders or gapped slots|Current joined/count-limited search selects one copy; unjoined or unexamined high slots can be invisible.|
|Removal fails after predicates or inventory changes|Caller still attempts both services and runs scene if they return; no atomic success or rollback.|
|Interruption after Cannon but before Dynamite removal|Retain consumed prefix/compaction; no automatic pair restoration.|
|R already mapped/joined or repeated direct scene|Always new physical allocation/remapping; join announcement repeats; no existing-actor reuse or unlimited capacity.|
|Missing/hidden/aliased Bowie/A/B/P mappings|No safe lookup rejection/repair; script/position/hide/follow effects can hit shared or unintended physical entries.|
|Dead R or unusual class|New sprite can be Blue Flame/class-derived; guarded setPos/facing may skip; unguarded actions and join still execute.|
|Dead Bowie, living Peter|Facing may skip; unguarded work still runs; all three final follow guards skip because they read the high byte.|
|Active count11 versus12+, existing active43|Below12 can set43; at12+ it skips a new active write without clearing an existing one; joined11 still set.|
|Changed A mapping or physical1|Final follows resolve logical A; eas_452BA clone reads fixed physical1; these need not be the same actor.|
|Text skip before/partway through dialogue|Ordinary cursor need not advance; csWait skips; join446/input and internal action waits remain.|
|Interrupted movement/Idle/scroll/DMA/text/input|No universal finite completion or settled-cast claim; reached mutations remain.|
|Interrupted after live copy, join,425,750 or256|Different persistent/current prefixes; a later flag does not prove all requested presentation completed.|
|750 set,84 clear versus84 set,11 clear|Fresh opening/follower/membership are independent; neither copied opening nor join guarantees R follower.|
|CURRENT255 / scroll / fresh72|Different actor and latch lifetimes; fresh flag copy uses reloaded cells, not necessarily previous live cell contents.|

Reproduce from the owning checkout after loading ignored
`local/private-inputs.ps1` in the **same process** as the command. `$upstream`
denotes the verified clean read-only pinned checkout. Reuse the
[retained-pair recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
(map-init equality, _verify_map_script_engine_fixture and _cutscene_facts), with
size/mtime unchanged. No extraction regeneration is needed.

1. Join the Map72 callback's complete operations/extent with
   `tests/fixtures/h2/map-events/zone-programs.json` and the selected transaction
   service entries/callsites. Resolve actual section02/07 includes and jump
   aliases; inspect full search/removal/compaction/stat consumers, not merely
   their names or a successful caller receipt.
2. Select only cs_4FFDA from retained programCorpus. Compare every macro/argument
   to source4–141; account separately for all inline action/custom records and
   encoded sizes. Sum558+42+12=612 and compare the next symbol$5023E. Resolve
   actual opcode dispatch, packed byte/word guards, scripts and action operands.
3. Join every cursor/context to literal text0D0D–0D1C and join446, sprite/name/
   class/input consumers; inspect complete JoinForce/JoinBattleParty and fresh
   follower84. Verify that the scene has no choice/transfer/heal or84 write.
4. Follow every reached map/flag/list/mapping/buffer operation through actual
   consumers. Compare the live copy with Map72's selected3-flag-events.asm and
   loader; read the single next-route warp row without expanding into its target.
   Review the changed-state table, including interrupted and repeated prefixes.
5. Check full prose/diff, relative links, UTF-8, sole-path/private boundary and
   `git diff --check`. On a clean committed head run
   `uv run sf2 verify plan --base origin/main --head HEAD`; interpret it under
   this documentation-only scope. Record exact identities, actual hosted CI and
   completed diagnostics on the Issue/PR handoff. No blanket local normal/full/
   engine/Godot/H3/H4 suite is required solely for this note.

**Unknown:** natural reach/supplied event/route and exact cell artwork;
flags, current inventory/packing/membership/HP/class, mode/mappings/actor order
and allocator capacity; completion of music/sound/text/input/sprite/DMA/scroll/
action/follower services, visible blast/withdrawal/settling, usable control and
save/load persistence. The note closes its static conditional outcome only.

Retain [#981 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/981#issuecomment-6180545109)
and [PR984 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/984#issuecomment-6180565663):
final e53e678a/tree81d1f6ef/base1a4d0092, original a21d8fe9 with identical note
blob, CI38652506094(scope15s/research33s), author0.081s and independent0.082s
unchanged-pair receipts. Preserve literal dc.w versus macro, zone-field,
jump opcode/timed-wait/obstruction corrections and linked predecessor failures/
Unknowns. The earlier guessed block-copy filename was resolved to the actual
Map72 `3-flag-events.asm`; no missing source is claimed. This slice's initial
read-only discovery guesses and a command-record display using the wrong keys
were replaced by exact paths/fields; they did not regenerate or alter evidence.
Direct VInt review corrected the draft distinction between retained action
pointers and execution: hidden X=$7000 suspends ordinary entity updates.

Leave the clean pushed Draft PR frozen, completed local processes and Project
Review / Verify for independent main-gate acceptance. Preserve environment,
source/pair and all receipts; no merge, cleanup, archive or next Issue.
