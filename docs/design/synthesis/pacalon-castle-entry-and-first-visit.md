# Pacalon castle: entry and the first drought-state visit

The castle's first-visit conversations describe hunger, drought and the cost
of shutting Moun's gate. The king asks the visitors to save Mitula in Tristan;
one centaur offers to fight, then concludes that he is too hungry. A priest,
books and statues, three possible pickups and four internal stair requests
share this visit. **Confirmed:** these are separate interactions. The king's
request creates no local quest flag or choice branch, and none of these
interactions is a prerequisite in the castle's exit records.

This guide continues [Pacalon conversations, supplies and the castle handoff](pacalon-interactions-and-castle-handoff.md)
from its three north requests into Map36. Its main premise is **F530 clear**.
It explains conditional exploration construction and the default cast, the
complete optional F810 scene, examination and services, then internal stairs
and the south request. It stops south at MainLoop **before** town battle
selection. **Unknown:** natural reach, supplied state, completed presentation,
usable control and persistence. No observed or remake frontier advances.

## Evidence and scope

**Confirmed evidence owner:** [Pacalon castle entry and first visit](../../research/pacalon-castle-entry-and-first-visit.md),
accepted through [#1025 / PR1028](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1028)
at `1d62ba1cd233577ddda8275d70ff1b8f9a41572a`, supplies the complete selected
tables, literals, command payloads and actual consumer joins. The accepted
[town guide](pacalon-interactions-and-castle-handoff.md) and its
[research owner](../../research/pacalon-interactions-and-castle-handoff.md)
retain incoming-request and shared interaction/pickup/service limits.
Source paths below are relative to `disasm/` at SF2DISASM `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; research retains the USA retail
ROM identity and source/address provenance. Flags use Fnnn, indices are
zero-based, and address ranges are half-open.

Standard rigor remains appropriate for [#1030](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1030):
membership, following, physical identity, current setup, script return and
reload lifetimes have separate predicates. Its five Markdown paths are the
only writes. The F530-set population/pointer boundary is context; its later
Frayja callback, restored-rain conversations and `cs_5DA28` are deferred.
Departure research, UI692, runtime, acquisition, new comparison machinery,
general service/battle audits and Chinese synchronization are outside scope.
Source/pair are unavailable in this design environment; accepted provenance
is reused without copying inputs or claiming a fresh private comparison.

## From the town request to conditional castle control

**Confirmed:** town's ordered north rows supply these nominal seeds:

| Town source X,Y | Explicit map / destination X,Y / facing |
| --- | --- |
|19,0|36 /20,30 /Up1|
|23,0|36 /22,30 /Up1|
|Negative X wildcard,0|36 /preserve sampled X,30 /Up1|

The third row is considered only after the exact rows fail. Ordinary walking
requires its applicable layout-kind/control consumer; the row is no proof of
a walkable route. The event slot is mutable, not a saved journey: producers
write type before parameters, no match preserves existing pending state, and
ProcessMapEvent clears the type before dispatch. The no-scroll handler
submits/clears WARP_SFX, idles physical0 and unwinds to MainLoop.

The handler clears d0–d4 words, reads the destination map and samples
physical0's **current signed coordinate words divided by384**. Nonnegative
destination bytes replace only low bytes; negative bytes preserve whole
sampled words. Thus 20/22/30 do not discard a supplied high byte, and wildcard
X preserves consumption-time X, not necessarily trigger-time X. Facing is1
and d4 is0. Parameter replacement or intervening motion changes this input.

MainLoop's ordinary `@Start` calls SwitchMap then CheckBattle. There is no36
flag-switched replacement and no36 in the 45 seven-byte battle-coordinate
records. An intact explicit36 request therefore returns d7=-1 without a
matching unlock test or battle crop. This differs from battle-return
`alt_MainLoopEntry`, which bypasses CheckBattle. A changed request/map remains
actual input, rather than being repaired into castle exploration.

ExplorationLoop clears pending type, performs signed-word STEP_COUNTER
subtraction20000/conditional clear, heals eligible allies, requests fade-out
and temporarily selects view-1. Explicit36 writes CURRENT_MAP36 and
CURRENT_BATTLE255, loads tilesets, waits for fade, clears VInts, selects and
rebuilds setup actors/mapsprites, clears temporary flags256–383 and sets80.
It initializes sprite counters, selects view0, calls LoadMap with the saved
map word and view-coordinate-1, installs base VInts and invokes the currently
selected init. Palette comparison conditions music/fade-in; movement SFX
and WaitForEvent follow.

The heal scans all30 allies: Peter7/Lemon28 always, others only at nonzero
current HP. Eligible members receive maximum HP/MP, retain only already-held
STUN/POISON/CURSE and recompute stats. Dead Frayja22 stays dead; joined22 is
not the heal predicate. Rebuilding clears previous actors, mappings and
walking scratch, but leaves F22/F82/F530/F810 and item flags174–176.
Physical0 uses whole returned coordinates scaled by384, facing1 and Bowie's
actual class/HP sprite rules, rather than the first active battle member.

After init, WaitForEvent checks pending type before A/C. Zero installs main
control and resets view0: numeric PLAYER_TYPE0 chooses character,1 raft,
other nonzero Caravan. Nonzero pending bypasses that installation and is
dispatched first. Actors/VInts may already progress. This is a conditional
event/control boundary, not an observation of usable castle control.

## The loaded castle and its changing setup

**Confirmed:** Map36's header selects Map36s0_Blocks, Map36s1_Layout and
its area, flag, step, roof, warp, chest and other-item tables. Palette10 and
tilesets90/52/50/92/58 are data selections. Its two areas are:

| Row | Main rectangle | Secondary foreground / background origin | Layer type / music |
| --- | --- | --- | --- |
|0|0,0 through47,31|0,32 /0,0|0 /MUSIC_CASTLE|
|1|50,0 through61,15|0,0 /0,0|255 /MUSIC_CASTLE|

Both layers have256,256 parallax and zero autoscroll. LoadMap with supplied
view-coordinate-1 selects by the viewed actor's actual internal position;
normal20/22,Y30 seeds select row0. Preserved X can select differently; no
matching rectangle falls back to the first area. The empty flag-event table,
five step block-copy records and nine roof records are separate resources.
Area metadata does not decode collision, door access or a traversable route.

`data/maps/mapsetups.asm` selects `ms_map36` or F530's `ms_map36_flag530`.
Their six-pointer tables at `$5D756..$5D76E` / `$5D76E..$5D786` differ only
in entity list and entity events. Zone, description, item-event and init
pointers are shared. GetCurrentMapSetup rereads map/flag on each call:
construction, interaction and init do not consume one frozen530 snapshot.

The alternate set530 population has11 rows: ally22 at27,8 Left, then
NPC128..137 at25,24 Left;12,25 Down walking;21,16 Down walking;8,3 Down;
5,22 Left;40,17 Down; king29,3 Down;31,5 Left;31,11 Left;priest40,4 Down.
Other NPCs are male centaurs. Its event pointer changes to `$5D924`.
Changing530 after construction selects new callbacks against retained actors,
including a changed priest index; it does not rebuild or relabel them.
The later visit's callbacks and progression remain outside this guide.

## Who is present, and what Frayja's flags actually do

**Confirmed:** the default list `s1_entities.asm`, `$5D786..$5D7F0`, has
13 eight-byte rows and a sentinel: **ally22 plus twelve NPCs**. It does not
begin at NPC128. The complete list and default conversation themes are:

| Row / logical | Declared X,Y / facing / sprite | Action | Map36_ callback / complete theme |
| --- | --- | --- | --- |
|0 /22|28,6 /Down /Frayja ally sprite|eas_Init|Event0,3301 →3302: confesses abandoning Moun's people to protect Parmecia.|
|1 /128|18,24 /Down /male centaur233|walk range1|Event1,3281: welcomes visitors from South Parmecia.|
|2 /129|33,26 /Left /male centaur233|eas_Init|Event2,3282 →3283: links dryness to the devils and suspects harm to Mitula.|
|3 /130|7,5 /Right /male centaur233|eas_Init|Event3,3284 →3285: denies that any soldiers can fight the devils.|
|4 /131|4,22 /Right /male centaur233|eas_Init|Event4,3286/3287: bravado, optional scene, then hunger.|
|5 /132|21,16 /Down /male centaur233|walk range1|Event5,3288 →3289 →3290: Nazca west of Moun, locked gate and Vicar Frayja's key.|
|6 /133|31,5 /Left /male centaur233|eas_Init|Event6,3291 →3292 →3293: invaders through Moun, blocked gate and abandoned residents.|
|7 /134|31,10 /Down /male centaur233|walk range1|Event7,3294 →3295: food shortage from drought and hope for one rainfall.|
|8 /135|29,3 /Down /king231|eas_Init|Event8,3296 →3297 →3298: thanks Higins's visitors, asks them to save Mitula in Tristan and fears drought before invasion.|
|9 /136|38,5 /Down /princess232|eas_Init|Event9,3299 →3300: describes a northward mountain route to Tristan.|
|10 /137|16,10 /Down /female centaur234|walk range1|Event10,3303 →3304: asks whether the soldiers in Mitula's Shrine were killed.|
|11 /138|22,9 /Down /female centaur234|eas_Init|Event11,3305 →3306 →3307: reports seeing the princess eating in the kitchen and calls it uncouth.|
|12 /139|40,4 /Down /priest192|eas_Init|Event12: j_ChurchMenu, then return.|

Map36 is not overworld or follower-excluded, so F65 adds no Caravan prefix.
Ordinary enabled flag-followers allocate first in the accepted
[follower order](battle26-return-through-pacalon-entry.md#town-population-is-conditional-content).
Their admission does not test force membership. Each enabled row allocates
independently; duplicate aliases can allocate actors and overwrite lookups,
and finite storage is not protected against mass enabling.

F82's follower row names22. InitializeMapEntities sees the fixed row's sprite
below30 as an ally; if lookup22 is already nonzero it consumes the action
longword but skips that declaration and physical increment. With only82,
Frayja remains follower1, with its position/script, instead of being placed
again at28,6. Without that lookup he is declared regardless of joined22.
For n actual follower declarations and a=1 if this ally row allocates,0 if
deduplicated, NPC j=0..11 maps logical128+j / lookup32+j to physical
n+1+a+j. The ally does not advance the NPC lookup cursor. No followers gives
Frayja1/NPCs2–13; only82 also gives NPCs2–13 but a different Frayja origin.
These formulas assume successful bounded allocation and valid mappings.

Init at `$5DA16..$5DA28` independently tests **joined22**. Clear returns;
set resolves logical22 and calls MoveEntityOutOfMap. SetEntityPosition
writes actual **and destination** X/Y to `$7000,$7000`. This neither deletes
the actor nor clears mapping/follower membership, stops its script, changes
HP/class, joins22 or enables82. VInt_UpdateEntities skips X at least$7000,
so the retained script does not immediately bring him back; later relocation
can make it eligible again. On retained CURRENT reload this same init runs
again. Missing lookup22 is not safe absence: zero resolves physical0 without
a guard, so a corrupt/missing mapping can displace main or another actor.

Frayja/Bowie sprites depend on actual ally class/HP; outside battle zero HP
uses blue flame. NPC fixed sprites have no ally HP/class selection. Four
walkers each copy50 bytes into walking scratch and patch origin/range1;
random-step requests do not prove a route. eas_Init establishes speed32/32,
motion/collision flags, Up orientation, size24 and sprite update before
eas_Idle. Orientation and declared facing are different fields.

## Talking, spoken claims and the priest

**Confirmed:** `s2_entityevents.asm`, `$5D84A..$5D924`, has the13 named
four-byte rows and a $FD default RTS. Every named row uses UP1: turn toward
the supplied player facing, without restoration. Relative callback offsets
resolve from the table base, not from a stable actor identity.

Ordinary C follows movement/view-scroll waits and the debug/Caravan
alternatives. GetActivatedEntity uses position/facing, can step through
counter/desk layout and scans physical actors near the tile ahead, skipping
the viewed actor and followers. Reverse lookup chooses the first alias;
failure returns0. The wrapper stores player-facing in EVENT_RELATIVE_POSITION,
selects the current setup and first logical match/default, then takes portrait
and speech from the **actual mapped sprite**. It clears VInt_UpdateEntities
before callback; ordinary return closes portrait/text and reactivates it.
Default RTS still has wrapper work. Camera/mode/mapping changes can change
activation independently of the nominal population table.

King/male-centaur sprite properties select no portrait and DIALOG_BLEEP_6;
princess/female-centaur select no portrait and DIALOG_BLEEP_2. Priest selects
PRIEST/DIALOG_BLEEP_5. FRAYJA_PROMO selects FRAYJA/DIALOG_BLEEP_3; another
class/HP sprite follows its own row or fallback. A portrait or text label is
not proof that the intended physical actor was activated.

The research owner joins all27 default text literals3281–3307 to gamescript
keys0CD1..0CEB. `txt` calls DisplayText; each call resets the substitution
pointer to DIALOGUE_NAME_INDEX_1. Explicit `{NAME;22}` in3290 names Frayja,
and `{NAME;19}` in3296 names Higins through GetCombatantName, independently
of speaker, membership or first active member. Multi-text sequences retain
W2 between entries and W1 at their end. Consecutive txt calls do not imply
a close. The [dialogue contract](../contracts/dialogue-system.md#poll-order-and-distinct-consumers)
keeps W1/W2 input/RNG/VInt/cursor/sound/cleanup rules distinct; neither is
YesNoPrompt, and a text wait is not measured presentation here.

The king's request is dialogue only:3296–3298 writes no quest flag, tests no
F19, makes no YesNo branch, grants/joins nothing and posts no map or battle
request. The other plain text bodies also return. Only Event4 has the local
F810 write, while the priest delegates conditional shared effects.
**Inferred:** remorse, drought and frightened bravado connect these themes.
Moun's gate/key, route access, casualties, divine harm and the princess's
behavior remain spoken statements, not independently proven world state or
mandatory objectives. A soldier's question does not confirm any deaths.

Event12 calls ChurchMenu without a local first-visit flag. Text110 welcomes
the visitor with two W2 controls before raise/cure/promote/save selection.
The [service contract](../contracts/service-interactions.md#church-source-boundary)
owns payment, member/status/data gates and bounded Raise/Cure/Save observations;
contact alone neither heals nor saves. Selected Save first-prompt result0
copies then-current CURRENT_MAP (nominally36) to EGRESS_MAP, sets399 and calls
SaveGame with CURRENT_SAVE_SLOT. A nonzero first answer avoids save writes,
then text112's another-service prompt uses zero to loop to @StartMenu and
nonzero to exit. After actual saving, the separate continue-adventure prompt
uses zero for menu exit, nonzero for text117/FadeOutToBlack/WitchSuspend.
That suspend path is not ordinary callback return. Map36's savepoint40,5/Up
is return data, not an immediate teleport. Invocation, reached mutation,
completed service and durable persistence remain different boundaries.

## The centaur's optional movement and gesture scene

**Confirmed:** Event4 with held810 requests only3287, "Oh, I can't. I'm too
hungry!", then returns. With clear810 it requests3286's offer to beat the
devils, closes text, calls `cs_5DA7A`, **then sets810**, requests3287 and
returns. No hunger, membership or route prerequisite is tested. F810 is
outside temporary256–383; ordinary reload does not clear it.

The script trap activates VInt_UpdateEntities before ExecuteMapScript,
allowing action progress despite the wrapper's earlier clear. The interpreter
initializes action scratch at FF9004_LOADING_SPACE and resets
SKIP_CUTSCENE_TEXT. The full program at `$5DA7A..$5DAE4` is:

| Index | Command and all inline/packed work |
| --- | --- |
|0|setCameraEntity131|
|1|setDest ALLY_BOWIE,4,23|
|2|setFacing ALLY_BOWIE,UP|
|3|entityActionsWait131: moveRight2; endActions|
|4|entityActionsWait131: moveDown1; endActions|
|5|setFacing ALLY_BOWIE,RIGHT|
|6|csWait60|
|7|customActscriptWait131: ac_setSpeed16,16; ac_jump eas_Idle; ac_end|
|8|entityActionsWait131: moveRight2; endActions|
|9|headshake131|
|10|shiver131|
|11|csWait120|
|12|shiver131|
|13|customActscriptWait131: ac_setSpeed8,8; ac_jump eas_Idle; ac_end|
|14|entityActionsWait131: moveRight1; endActions|
|15|shiver131|
|16|csc_end|

The camera resolves131 through lookup35 and changes VIEW_TARGET_ENTITY,
without waiting for arrival. Nothing here resets that target before final
text. Bowie's setDest scales4,23 by384, writes destinations, absolute travel
and signed velocities32 per axis, then waits for actual X/Y to equal their
destinations through WaitForEntityToStopMoving/VInts. It does not teleport
or search a path. The two facing commands separately check ally HP: zero HP
skips their operands/facing writes; living mapped Bowie receives Up1 then
Right0 and a sprite update. setDest has no equivalent HP gate.

Each entityActionsWait replaces131's action pointer with compiled scratch,
seeds its action timer with the physical ID and clears flagsA bits5/6 via$9F.
The packed direction/distance emits relative-destination opcode4, signed
word deltas and opcode1 (`esc01_waitUntilDestination`, not a one-frame wait).
The $8080 payload end appends opcode$34/jump eas_Idle; the nonzero Wait byte
polls for that exact idle pointer. Old actions and flags are not restored.
esc04 scales deltas by384 relative to **then-current** actual position.
Its overlap helper is gated by flagsA bit5, which this compiler clears;
loc_55C8 supplies destinations/travel/velocity from the current speed bytes,
consumed by VInt movement. No collision-checked walking guarantee follows.

If all four moves complete uninterrupted from nominal4,22, their successive
destinations are6,22;6,23;8,23;9,23. This is conditional arithmetic, not a
forced endpoint for changed mapping, position or scripts. The two inline
payloads install their action address, seed the physical-ID timer, wait for
eas_Idle and scan words through$8080. Each has10 action bytes plus the
two-byte terminator: opcode$10 writes the packed adjacent speed bytes;
opcode$34 consumes the absolute eas_Idle pointer. Reached completion leaves
speed16/16 for the second rightward move and8/8 for the last, rather than
restoring initial32/32.

Headshake sets animation counter-1, performs seven DBF iterations of two
mapsprite transformation/DMA/update sequences with2+1 VInt waits, and ends
with counter0 rather than restoring the saved value. Its sub_45D1C/sub_45D46
helpers use sub_45DA4's nibble-copy work on both animation frames. Each shiver
saves size/counter, sets size21/counter-1, toggles flagsB bit3 on/off three
times with Sleep5 after each sprite update, then restores size/counter.
Bit3 ends clear even if initially set. It is RESIZE: ordinary
ChangeEntityMapsprite decompresses facing graphics, conditionally resizes
with size21 and queues VRAM DMA; special sprites/entnum32 may skip that path.
Shiver changes graphics state, not X/Y. These requests prove no rendered
headshake or shiver at the castle.

csWait60/120 encode$803C/$8078: the interpreter passes the low byte to Sleep
for that many WaitForVInt calls. Debug/P2-Start may set SKIP_CUTSCENE_TEXT and
skip those negative-word sleeps. Headshake and shiver's explicit waits remain
separate. Requested counts are not measured seconds or completed animation.
At csc_end, nonzero DIALOGUE_WINDOW_INDEX conditions a view-scroll wait;
the interpreter clears VIEW_SCROLLING_SPEED and returns without restoring
camera target, old scripts, speed, positions or flagsA.

An interruption before script return may leave scene effects with810 clear;
one during final3287 may leave810 set without completed dialogue. Callback
return still requires wrapper cleanup, and the next WaitForEvent resets view0
and installs control only with pending zero. **Script return, callback return
and usable control are three boundaries.** None is a natural castle runtime
observation in this evidence.

## Books, statues, the locked door and three possible pickups

**Confirmed:** both setups share the description wrapper at
`$5D9C6..$5DA0E`:16 bytes, nine six-byte records at$5D9D6, then$FD00.
Base$1038=4152, condition0 and text payload-kind0 apply to every row:

| Row / X,Y | Investigation offset →ID | Description offset →ID / content |
| --- | --- | --- |
|0 /35,3|4 →427, bookshelves|0 →4152, book: PACALON KINGS|
|1 /36,3|4 →427, bookshelves|1 →4153, book: PALADIN HONOR|
|2 /56,8|10 →433, door|2 →4154, "It's locked."|
|3 /19,15|9 →432, statue|3 →4155, fine soldier|
|4 /23,15|9 →432, statue|4 →4156, fine soldier|
|5 /26,8|9 →432, statue|5 →4157, fine soldier|
|6 /32,8|9 →432, statue|6 →4158, fine soldier|
|7 /26,11|9 →432, statue|7 →4159, fine soldier|
|8 /32,11|9 →432, statue|8 →4160, fine soldier|

CheckArea clears name slot/speech to0, so investigation NAME means Bowie's
name. It requests423+investigation with W2/CLEAR, then4152+description with
W1, closes portrait/text and returns d7=-1 on the first admitted match.
These condition0 rows admit ordinary d6=1. No match returns0, allowing
FieldMenu fallback for d6=1; supplied d6=0 instead uses generic area/empty
text. The [description contract](../contracts/map-area-description-routing.md#six-byte-record-and-first-match-scan)
owns the packed scan, condition and text/function distinction.

Entity activation happens first. Within CheckArea, actual layout kinds
$1800 chest,$2C00 vase,$3000 barrel,$3400 bookshelf and$1C00 area-item take
priority over descriptions. Thus a book-labelled coordinate does not prove
that its wrapper executes. The locked-door literal checks no key, writes no
unlock flag and opens no passage. No local description has a function,
choice or story mutation. Table coordinates are not decoded access evidence.

The shared four-byte collection records are:

| Table / row | Candidate X,Y | Flag | Item symbol / index |
| --- | --- | --- | --- |
|Chest0|56,4|176|MITHRIL123 ($7B)|
|Other0|22,4|174|FAIRY_POWDER5|
|Other1|30,19|175|PEGASUS_WING118 ($76)|

The chest table is `$AE07A..$AE080`; other items `$AE080..$AE08A`, each
ending endWord. Flags174–176 are independent of530/810 and outside temp.
GetChestItem/GetNonChestItem select current-map pointers; GetItem compares
candidate low coordinate bytes in row order. Changed CURRENT_BATTLE≠255
retains the original erroneous stride5 crop alternative, not a repaired
seven-byte stride. Layout/caller still determines whether a record is used.

Actual chest-kind examination calls OpenChest: held176 substitutes
NOTHING127; it sets176, writes open block55298/$D802 and requests the plane
update (type0 bit1, other layer type bit0) **before** pickup. Text403 announces
opening; empty results request408. CheckChestItem only tests flag/layout,
without those opening writes. Repeats do not establish duplicate Mithril;
the actual retained/loaded block kind still determines examination routing.
Eligible non-chest branches set174/175 before returning and substitute
NOTHING when held. No match-1 masked to127 selects empty-object text.

Items5/118/123 take the ordinary inventory branch below the gold threshold.
The common tail requests413 (found item, W2/CLEAR), checks Bowie's four slots
and calls AddItem on room; it then requests item music,415 and the separate
FadeOut_WaitForP1Input boundary. With Bowie full, UpdateForce builds joined
members and ordered OTHER_FORCE_MEMBERS_LIST searches for room; AddItem,
music,416 with recipient-name substitution and input/fade follow. Fallback
is not active-party-only or HP-gated. Its list starts one byte after TARGETS_LIST
and assumes joined Bowie first: altered unjoined-Bowie state still tries0
but omits the actual first joined fallback member. Text415/416 has no W1/W2;
plain input is separate. AddItem can fail if capacity changes after its read,
and this caller does not test that return. Success-sounding text is not proof
of delivery.

All-full requests417 before **both** CloseChest and RefillNonChestItem at
saved candidate coordinates. Each re-finds its own table: Mithril recovery
clears176 and writes closed block55297/$D801 with plane update; non-chest
recovery clears174/175. Retry is enabled only once recovery is reached.
Interruption can leave a flag without an item, or an inserted item without
completed presentation. Collection is not atomic and establishes no Pegasus
Wing use, promotion or Mithril forging. The accepted
[pickup owner](../../research/ketto-interactions-and-merchant-relocation.md)
and corrected town guide retain shared text/order/force semantics.

Shared zone/item-event tables contain only their default RTS. Their wrappers
still clean up/wait; item use supplies d6=0 and differs from collection.
Neither default grants items, joins Frayja or posts an exit request.

## Internal stairs retain state; south requests the town

**Confirmed:** `6-warp-events.asm`, `$AE050..$AE07A`, holds five ordered
eight-byte no-scroll/type0 records and endWord. Facing is a byte followed
by zero padding:

| Row | Source X,Y | Map operand | Destination X,Y | Facing |
| --- | --- | --- | --- | --- |
|0|255,31|PACALON2|255,1|Down3|
|1|2,4|CURRENT255|52,3|Right0|
|2|52,3|CURRENT255|2,4|Left2|
|3|55,11|CURRENT255|5,12|Right0|
|4|5,12|CURRENT255|55,11|Right0|

WarpIfSetAtPoint checks$FFFF terminator before first-match scanning; negative
source bytes are wildcards. Character walking requires layout kind$1000;
raft/Caravan have other callers. No row tests530/810/joined22/items/services.
Each writes event1, PARAM1..4 longword, PARAM5/padding word and WARP_SFX.
No match preserves pending. The handler consumes mutable parameters, idles
main, samples current words and independently replaces nonnegative low bytes
or preserves negative whole words, returning to MainLoop with d4=0.

CURRENT255 is neither scrolling nor resolved by SwitchMap. CheckBattle
temporarily resolves CURRENT_MAP; actual36 has no battle row, so it restores
operand255 before exploration. Changed CURRENT_MAP can change that result.
For the intact stair requests, ExplorationLoop still clears pending,
adjusts STEP_COUNTER, heals, fades/waits, updates main facing/sprite and
nonnegative-low-byte coordinates, calls DeclareRaftEntity, resets sprite
counters, LoadMap, base VInts and current init. Whole coordinate words are
scaled only when their low byte is nonnegative; changed negative low bytes
skip that coordinate update.

Unlike explicit entry, CURRENT does **not** rebuild actors/mappings/walking
scratch, load a new fixed list, replace CURRENT_BATTLE or clear temp flags.
DeclareRaftEntity is significant: numeric mode2 returns; mode1 skips follower
relocation; other modes move listed followers' actual/destination coordinates
to main's supplied tile. It then uses F64/RAFT_MAP to declare or move off-map
physical raft31. Follower mappings/scripts survive. Castle init runs **after**
relocation and again moves mapped22 to$7000 when joined22 is held. This is
displacement of a retained actor, not removal from the party or follower list.

LoadMap sign-extends CURRENT255 and branches directly to area's pointer:
no new palette/tilesets or LoadMapBlocksAndLayout. **Loaded blocks/layout
remain**, while area/view/display and roofs refresh. Ordinary rows1/4 select
the50..61 area, rows2/3 the0..47 area. Explicit-map loading instead rebuilds
blocks/layout and reapplies held chest flags as open blocks. Stairs do not
reset174–176 or unlock the described door. They normally retain810 and the
centaur's reached action/speed/position, subject to subsequent actor execution.
A fresh explicit36 rebuild restores declared position/script even with810
held; repeat hunger dialogue does not require a persistent scene endpoint.
Current530 may change init/event selection without reconstructing the cast.
Palette/music and pending/control conditions still apply.

South row0 returns explicit Map02, Down3, destination Y low byte1 and
preserved consumption-time X. **The endpoint is this MainLoop return before
town SwitchMap/CheckBattle/setup.** The earlier town-entry guide retains
possible Battle31 admission; this request establishes no town rebuild or
usable town control. Explicit-map requests rebuild population/temp/layout
only if subsequent admission reaches exploration. That conditional lifetime
is separate from the four CURRENT stairs.

[Pacalon departure: the north scroll and conditional Battle27 admission](pacalon-departure-to-battle27-admission.md)
continues that south request through conditional town admission, fresh
overworld construction, retained north scrolling and the separate zone/
reload/battle scan. It stops before BattleLoop initialization and retains
natural-route, supplied-state and control/persistence Unknowns.

## Changed states and retained review

| Changed premise or interruption | Required distinction |
| --- | --- |
|530 clear/set or changes between setup reads|Default cast and current callbacks can disagree; alternate progression stays deferred.|
|22/82 independently held|Fixed ally allocation, follower dedup and init displacement are different predicates; no recruitment here.|
|Missing/aliased mapping, many followers, altered HP/class|Actual actor, first reverse alias, sprite and finite storage govern; zero is not safe absence.|
|810 held or scene/final text interrupted|Scene effects precede810;810 precedes final text completion; repeat skips the script.|
|Actor position/action/speed/flags, camera or debug changes|Relative motion, retained speed, overwritten scripts and wait/graphics consumers use current state.|
|Layout, mode, pending or description d6 differs|Activation, first match, control and examination priority remain conditional.|
|174–176 held, joined list/capacity changes or pickup interrupts|Flags, AddItem, music/text and recovery are distinct commits; no guaranteed delivery.|
|Church decline, Save, continue/suspend or interruption|Service loop differs from save and suspend; invocation proves no persistence.|
|CURRENT/explicit map, signed sentinel, high bytes or pending replacement|Retained versus rebuilt actor/layout/temp state follows actual consumed operands.|

The pre-synthesis review consumed complete accepted #1025 research and the
prior interaction guide, plus the necessary
[entry](../../research/battle26-return-through-pacalon-entry.md),
[interaction](../../research/map-event-interaction-state.md),
[dialogue](../../research/map-event-dialogue-state.md),
[request](../../research/map-event-request-state.md),
[consumption](../../research/map-event-request-consumption.md),
[maps](../../research/common-maps.md), [script/action](../../research/common-scripting.md),
description, pickup and service owners. Selected real public payloads, schemas
and verifier consumers were inspected, rather than substituting aggregate counts:

| Public fixture / actual consumer | Checked boundary and limit |
| --- | --- |
|`sf2-map-init-static-v1`; [fixture](../../../tests/fixtures/h2/map-init-static-v1.json), [map_init.py](../../../src/sf2tool/h2/map_init.py)|Actual Map36 default/530 route81/82 and pointer82/83 joins and five init operations; not a runtime mapping or natural init observation.|
|`sf2-map-events-static-v1:routing-setup` and `:entity-programs`; [routing](../../../tests/fixtures/h2/map-events/routing-setup.json), [programs](../../../tests/fixtures/h2/map-events/entity-programs.json), [map_events.py](../../../src/sf2tool/h2/map_events.py)|Selected default targets and all14 default bodies, ordered text,810/script/write/return and Church alias. Neither the table nor bodies prove activation or display.|
|`sf2-map-script-engine-static-v1`; [fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json), [map_script_engine.py](../../../src/sf2tool/h2/map_script_engine.py)|Actual selected camera, placement, action bridge, gesture/presentation and interpreter handler guards/cursor/wait consumers. Public compact projection does not expose cs_5DA7A's full program or six payloads; their exact local join is accepted #1025 evidence, not newly compared here.|
|`sf2-entity-action-scripts-static-v1`; [fixture](../../../tests/fixtures/h2/entity-action-scripts-static-v1.json), [entity_action_scripts.py](../../../src/sf2tool/h2/entity_action_scripts.py)|Compact handler/terminator and update/graphics facts plus their actual parser/verifier consumer. Exact relative-destination/speed/jump source joins remain accepted research; this projection is not observed castle motion or rendered animation.|
|`sf2-map-event-interaction-state-static-v1`, `sf2-map-event-dialogue-state-static-v1`, request-state/consumption fixtures; [interaction](../../../src/sf2tool/h2/map_event_interaction_state.py), [dialogue](../../../src/sf2tool/h2/map_event_dialogue_state.py), [requests](../../../src/sf2tool/h2/map_event_request_consumption.py)|Acquisition/reverse mapping/wrapper and selected pending read/reset/dispatch contexts. Bounded contexts do not enter every warp/map callee or prove a natural producer-to-consumer route.|
|`sf2-map-descriptions-static-v1`; [fixture](../../../tests/fixtures/h2/map-descriptions-static-v1.json), [map_descriptions.py](../../../src/sf2tool/h2/map_descriptions.py)|Six-byte/terminator/d6/first-match/text-base consumers. Nine local rows, literal joins and three collection records rely on accepted research, not a decoded public layout.|
|`sf2-common-menus-static-v1` and `sf2-church-save-lifecycle-runtime-v1`; [menus](../../../src/sf2tool/h2/menus.py), [Save](../../../src/sf2tool/h3/church_save_lifecycle.py)|Actual Save route operations, choice polarity and five controlled observation chronologies; not a natural Map36 service or persistence observation.|

Owning commands are `uv run sf2 h2 map-init`, `map-events`, `map-script-engine`,
`entity-action-scripts`, `map-event-interaction-state`, `map-event-dialogue-state`,
`map-event-request-state`, `map-event-request-consumption`, `map-descriptions`,
`common-menus` and `uv run sf2 h3 church-save-lifecycle`. Their public surfaces
were reviewed without rerunning private-source extraction or runtime.
No blocking owner disagreement was found; a future contradiction returns to
research before extending this explanation.

[#1025's final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1025#issuecomment-6183213238)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1028#issuecomment-6183221581)
retain its complete source/table/literal/payload/consumer review: final
head2ad7209b/tree048f728d/base936cee7d, direct0.146s/independent0.144s and
CI38672496689. The original5fa5cb21/tree015ecf08/basee9da6214, direct0.145s,
CI38672213037 and failed expanded-review fixture-key diagnostic remain
distinct. The rebased owned document was byte-identical; no completed result
is reset. The 18,285,979-byte pair's accepted0.083s provenance remains reused,
without regeneration or a new comparison in this checkout.

[#1026's corrected handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1026#issuecomment-6183164493)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1027#issuecomment-6183175026)
retain headb0b886fe/tree8ec730a0/basee9da6214, corrected direct2.369s /
independent2.357s and CI38672157287. Its original882ec169, direct2.400s and
CI38671842716, correction history and inspection/checker diagnostics remain
separate, including pickup music/text chronology and Save menu-loop versus
post-save suspend. Their linked #1021/#1022/#1017/#1018 handoffs preserve
earlier failures, original/rebased receipts and Unknowns.

This Issue's handoff records whole-prose/diff/changed-state review, exact
five paths, selected public-schema/consumer checks, navigation/links/anchors,
UTF-8/private checks, git diff --check, configured clean-head planner and
actual exact-head scoped CI. Documentation scope skips blanket normal/full/
Python/.NET/Godot/H3/H4/translation suites. **Unknown:** supplied flags,
HP/class, mapping/followers/capacity, inventory/modes/pending, natural reach,
completed scene/dialogue/services/collection/travel, usable control,
rendering/timing and persistence. Optional compressed-layout absence remains
a no-decoded-route limit, not acquisition authorization or a ROM blocker.
These Unknowns create no runtime queue; later Frayja progression stays deferred.
