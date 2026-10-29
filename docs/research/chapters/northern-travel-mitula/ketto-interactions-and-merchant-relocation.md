# Ketto interactions and merchant relocation

Evidence date: 2026-10-14. **Confirmed:** eight resident callbacks separate
conversation, merchant relocation, the woman's real choice, and shop/church
invocation. Shared consumers handle descriptions/items; separate departure
requests Map72, where battle admission can intervene. **Inferred:** candidate
local activities after accepted Ketto entry. **Unknown:** natural reach,
supplied state, completed services, presentation, usable control and saves.

## Agreement and provenance

Standard rigor for [Issue #997](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/997)
under [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
dialogue, choices, persistent 800, temporary 256/257, motion, service dispatch
and completion are separate. Accepted starting base is
`7e95f6d2699e554e8d4d2c601ec9c96327d0ff73`. Sole owned path is this note;
no shared indexes, registries, contracts, fixtures, schemas, tooling, design,
remake, glossary or Chinese writes. Independent #998/UI692 are not evidence.

Consume accepted [Ketto entry](./battle25-outcomes-through-ketto-entry.md),
[interaction state](../../world/map-event-interaction-state.md),
[dialogue state](../../world/map-event-dialogue-state.md),
[request writers](../../world/map-event-request-state.md),
[request consumers](../../world/map-event-request-consumption.md),
[services](../../../design/contracts/party/service-interactions.md),
[dialogue](../../../design/contracts/interface/dialogue-system.md) and
[area descriptions](../../../design/contracts/world/map-area-description-routing.md).
Stop at local returns/conditional control and Map72 next-route admission.
Battle26 roster/combat/aftermath, later towns, runtime, acquisition,
regeneration, new machinery and exhaustive generic-service audits are excluded.

Source paths are relative to `disasm/` in `ShiningForceCentral/SF2DISASM`
commit `c834c652b6862bc5679fd7f69a38a7093206efc6`; retained read-only source
was clean at that pin. Fixtures/payloads identify USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Constants/RAM: `sf2enums.asm`/`sf2const.asm`. Fnnn means flag nnn; rows are
zero-based, extents half-open. Short scripting paths are under
`code/common/scripting/`, exploration paths under `code/gameflow/exploration/`;
local tables under `data/maps/entries/map25/` and its `mapsetups/` directory.

Minimum evidence is selected source/contracts/fixtures and the retained
18,285,979-byte init/script pair. Linear comparison budget: under one minute/
512 MiB, plus small prose/receipts; unchanged-pair comparison took 0.081s.
No runtime, regeneration or installation. Replan a material contradiction or
tool expansion. The dispatch's nonwaiting-Bowie premise is corrected below
from its actual consumer; this requires no expanded product scope.

## Activation and all callbacks

**Confirmed:** MapSetups has only default ms_map25; F800 changes init, not
setup/table selection. The entry owner proves eight fixed NPC declarations.
For n preceding followers, logical 128..135 normally map to physical n+1..n+8;
main is physical 0. Merchant133/woman135 use lookup offsets 37/39 and physical
n+6/n+8. Follower flags, duplicate aliases, HP/class and supplied mappings
remain relevant. Logical IDs are not physical slots or a guaranteed cast.

`explorationvints.asm:ProcessPlayerAction` idles physical 0 and waits for
destination/view scroll. A opens the field menu; C reaches activation after
debug and overlapping physical 1-Caravan alternatives.
`explorationfunctions_0.asm:GetActivatedEntity`, with ordinary view-target0,
uses current position/facing, advances through desk/counter layout kind $2800
under mask$3C00, then selects the first eligible physical actor at Manhattan
distance below 384. It skips the viewed actor and current followers. Negative
view target returns; other camera states are not proven ordinary activation.
The layer read is not a layer-equality test. Return d1 is saved actor facing,
d2 player facing. In `code/gameflow/battle/battlefunctions/battlefunctions_0.asm`,
GetEntityEventIndex searches ENTITY_INDEX_LIST from its beginning for that
physical ID, switches its logical counter to 128 after 32 ally entries, and
falls back to 0 on no match. An earlier alias can change event selection.

`map/mapsetupsfunctions_1.asm:RunMapSetupEntityEvent` stores d2 in
EVENT_RELATIVE_POSITION, scans four-byte rows for first logical match or
$FD default, obtains portrait/speech from the actual mapped sprite, prefills
CURRENT_PORTRAIT/CURRENT_SPEECH_SFX, optionally opens the portrait, and
resolves the actor again. Row operand UP1 means turn after a VInt to
`(playerFacing+2)&3`, without saved-facing restoration; DOWN3 additionally
restores saved facing after callback return. They are action bits, not
required approach directions. Default0 does neither. The wrapper clears
the VInt_UpdateEntities flag before the callback, then restores facing where
selected, closes portrait/text and activates that VInt function on return.

The complete `s2_entityevents.asm` ($5D224..$5D33A) selects:

| Row / logical actor | Action / callback | Complete local work |
| --- | --- | --- |
|0 /128 young man|UP1 / Map25_EntityEvent0|1956, return.|
|1 /129 old man|UP1 / Event1|1957 →1958 →1959, return.|
|2 /130 man|UP1 / Event2|1960 →1961 →1962, return.|
|3 /131 young man|UP1 / Event3|1963 →1964, return.|
|4 /132 man|UP1 / Event4|1965, return.|
|5 /133 merchant|UP1 / Event5|Clear800: saved-speed 1966–1969, restore speed, set 800, cs_5D3B8, return. Set800: shop 9/ShopMenu, return.|
|6 /134 priest|UP1 / Event6|Clear256:1971, set 256. Both paths: ChurchMenu, return.|
|7 /135 woman|DOWN3 / Event7|Set800: shop 27/ShopMenu. Else set 257:1978. Else1972, YesNoPrompt, close text; zero→1973–1975, nonzero→1976–1977; both set 257. Return.|
|8 /default|0 / Map25_DefaultEntityEvent|RTS only within the shared wrapper.|

`nullsub_5D2E8` is a separate RTS between priest and woman, not a ninth
resident row. No local callback/relocation program sets 426, recruits,
activates/deactivates members, charges gold or grants items. Invoked services
can change inventory/gold/HP/status/class/save state on their selected paths;
that is not an unconditional conversation effect or a Battle26 unlock.

## Complete dialogue and actual choice

**Confirmed:** these literals match actual `data/scripting/text/gamescript.txt`
keys 07A4..07BA, independently of ASM comments. `{N}` is newline; W1/W2 are
distinct text consumers. Spaces/punctuation are retained, including the space
before 1965's W1. None of these 23 strings has a name/item substitution.

| ID | Complete literal |
| --- | --- |
|1956|`Ketto is on the southern{N}edge of North Parmecia.{W1}`|
|1957|`Where did you come from,{N}boy?{W2}`|
|1958|`From South Parmecia?{N}Really?{W2}`|
|1959|`The tunnel to the south is{N}blocked.{W1}`|
|1960|`From South Parmecia?{N}Why did you come here?{W2}`|
|1961|`Pacalon has become isolated{N}since we opposed the devils.{W2}`|
|1962|`If I were you, I'd leave.{W1}`|
|1963|`It's unusually hot!{W2}`|
|1964|`Is Mitula as sick as I've{N}heard?{W1}`|
|1965|`The devils forbade us to go{N}out. {W1}`|
|1966|`Blast!  I hate devils....{W1}`|
|1967|`Ah, are you a customer?{N}Wow!{W2}`|
|1968|`It's a secret, OK?{N}I have very good weapons.{N}Hee, hee.{W2}`|
|1969|`Do you want to buy one?{N}Oh, you do?!  OK, come to my{N}shop!{W1}`|
|1970|`I'm a priest.{W2}`|
|1971|`Sorry, I can't open my{N}church for you.{N}Please understand.{W1}`|
|1972|`Hey, listen to me.{W1}`|
|1973|`Pacalon was always a good{N}customer for us!{N}They bought a lot!{W2}`|
|1974|`But, now we can't sell our{N}weapons because of the{N}devils!{W2}`|
|1975|`Annoying devils!{W1}`|
|1976|`Oh, sorry.{W2}`|
|1977|`I don't want to bother you.{W1}`|
|1978|`You again?{N}Bye now.{W1}`|

1970 exists but none of these callbacks requests it. Dialogue about blocked
travel, a closed church or forbidden sales is not an executable gate.
Merchant1967–1969 have no YesNoPrompt call or choice-result branch. W1 on 1969
is not consent, payment or a purchase.

`txt` reaches DisplayText; successive local txt calls have no implicit
single-textbox close/sleep. `text/textfunctions_1.asm:DisplayText` creates/
reuses the window, resets its substitution pointer to DIALOGUE_NAME_INDEX_1
per call, decodes through text-bank/Huffman owners and returns at terminator;
it does not increment the cutscene cursor. The accepted dialogue owner
preserves W1's RNG draw/copy →VInt wait →masked input test and W2's distinct
cursor, RNG/wait/input, validation-sound and cleanup work. Both restore saved
typewriting state; neither is a decision menu. Text continuation, explicit
clsTxt and callback cleanup remain different operations.

The woman's `j_YesNoPrompt` resolves to `code/common/menus/yesnoprompt.asm`:
choice byte 0 starts at label Yes; held P1 input at entry requires raw release.
The loop tests Left, Right, B, C, A in order. Left stores 0, Right$FF/No;
selection loops rather than confirming. B commits $FF; C/A confirm the current
byte. After window cleanup, the helper reads it and sign-extends: Yes=0,
No/B=-1. The callback's clsTxt then tst.w/bne chooses1973–1975 for zero,
1976–1977 otherwise. Both set 257 only after final text returns. Interruption
before that leaves257 clear; held 257 instead requests 1978 without prompt.
F800 is tested first, overriding every 257/prompt path with the item shop.

## Speed and complete merchant program

**Confirmed:** clear 800 Event5 saves MESSAGE_SPEED's byte through d0/stack
longword, writes 0/1/2/3 before 1966/1967/1968/1969, restores the saved byte,
then sets 800 **before** cs_5D3B8. Its shop branch replays neither dialogue
nor relocation. `text/textfunctions_2.asm:HandleDialogueTypewriting` derives
ordinary glyph delay from 3-MESSAGE_SPEED: these values yield4/2/1/0 operands
before remaining speech/input work. W1/W2 remain independent; this is no
wall-clock guarantee. Interrupted text may leave temporary speed; interrupted
motion may leave800 set and actors partly changed. There is no transaction.

`mapsetups/scripts.asm:cs_5D3B8`, $5D3B8..$5D40E, is 86 bytes. The retained
programCorpus has 12 top-level commands; inline data are additional bytes.
`sf2cutscenemacros.asm` and actual consumers resolve the complete program:

| Index / source lines | Operation and physical width |
| --- | --- |
|0 /4–7|customActscriptWait 133: opcode $14,133,$FF (4 bytes); ac_setSpeed 64,64 (4), ac_jump eas_Idle (6), ac_end/$8080 (2).|
|1 /8|setDest ALLY_BOWIE,12,2: opcode $29, actor word 0,X12,Y2 (8 bytes).|
|2 /9|setFacing ALLY_BOWIE,DOWN: opcode $23,0,3 (4 bytes).|
|3 /10–12|entityActionsWait 133: opcode $2D,133,$FF (4); moveRight 5 (00,05), endActions/$8080 (4 more bytes).|
|4 /13–15|Same header; moveDown 1 (03,01), endActions.|
|5 /16–18|Same header; moveRight 2 (00,02), endActions.|
|6 /19–21|Same header; moveUp 1 (01,01), endActions.|
|7 /22–24|Same header; moveRight 2 (00,02), endActions.|
|8 /25|headshake ALLY_BOWIE: opcode $27, actor word 0 (4 bytes).|
|9 /26|setPos 133,25,18,RIGHT: opcode $19 plus four bytes (6 bytes).|
|10 /27|setPos 135,25,17,RIGHT (6 bytes).|
|11 /28|csc_end/$FFFF (2 bytes).|

ExecuteMapScript resets generated-action cursor to FF9004_LOADING_SPACE,
saves registers and dispatches. `csc14_setEntityActscriptManual` resolves 133,
seeds its action-read timer with that physical ID and installs the inline
pointer. Nonzero wait$FF polls until the pointer equals eas_Idle, then scans
the $8080 marker. `entity/entityscriptengine_2.asm:esc10_setSpeed` stores
paired $4040 into X/Y speed bytes; esc34 jumps to eas_Idle. Installation alone
does not prove that those consumers executed or that an actor moved.

**Confirmed correction to the dispatch premise:** command 1 **waits**.
ALLY_BOWIE=0, and setDest emits that word unchanged.
`map/mapscriptengine_1.asm:csc29_setEntityDest` retains it in d6, scales 12,2
by 384 and writes destinations/travel/±32 velocity. Its
`btst #$F,d6; bne return_46DEC` skips waiting only for bit 15 set. Actual 0
falls through to WaitForEntityToStopMoving, which waits for both positions
to equal destinations, with VInt waits while unequal. Changed operand $8000
would still mask to logical 0 for lookup but skip that wait. Command 2 then
requests Down; the ally-HP guard skips facing for dead Bowie. setDest and
headshake have no such HP guard. The actual branch, not the macro name or
dispatch assumption, controls the waiting claim.

Each `csc2D_entityActionSequence` resolves 133, seeds its timer, ANDs FLAGS_A
with $9F (clearing bits5/6), and generates 14 bytes: words `[4,dx,dy,1,$34]`
then the eas_Idle longword. Ordered signed deltas are (5,0),(0,1),(2,0),
(0,-1),(2,0), with up dy encoded $FFFF. Each stream has 8 move/wait bytes and
6 jump bytes: 70 generated bytes total. It advances the generated cursor,
installs that stream, then polls the actor's action pointer for eas_Idle.

Action opcode 4 scales signed tile deltas by 384 and adds actual current
position. Its destination-obstruction helper is gated by FLAGS_A bit 5,
cleared by sequence installation; the shared motion tail derives destination,
travel and signed velocities from stored speed bytes. Opcode 1 waits for
X/Y=destination, then opcode $34 jumps to idle. Five segments are sequentially
waited, cumulative(+9,0). From untouched(11,3), their conditional endpoint
is (20,3), **not** the shop; altered starting positions change endpoints.
This is neither pathfinding nor accessibility evidence.

`csc27_entityShakeHead` resolves 0, sets ANIMCOUNTER=-1, executes seven DBF
iterations (d7=6) of two transforms, DMA requests/sprite updates and six
explicit VInt waits per iteration, then clears the counter. Forty-two named
waits do not prove total duration, completed DMA or usable control.

The final two `csc19_setEntityPosAndFacing` calls independently write
X/XDEST=25*384, Y/YDEST=18*384 or 17*384 and Right0, then request sprite update.
They do not walk, replace action scripts or clear velocity/travel. NPC133/135
bypass ally-HP guards. Lookup masks the actor byte, subtracts 96 for NPCs and
scales the mapped physical ID by 32. Absent zero entries alias physical 0;
hidden $FF entries are not safe no-ops. Fresh declarations supply intended
actors, but retained/corrupt mappings are not repaired. Script termination
can wait for view scroll if text is open, then clears VIEW_SCROLLING_SPEED.
These action/destination/facing/headshake/setPos commands are not text-skip
operations; installed scripts still depend on actual service progress.

## Shops, priest and reload lifetime

**Confirmed:** Event5 set 800 writes CURRENT_SHOP_INDEX9; Event7 set 800
writes 27; both call j_ShopMenu → `code/common/menus/shop/shopactions.asm`.
GetShopInventoryAddress skips exactly that many length-prefixed rows from
list_ShopInventories. PopulateShopInventoryList copies count/items into the
generic list. Index 9 is the tenth physical row (comment “Weapon shop 10”),
not comment “Weapon shop 9”;27 is row 27 (comment “Item shop 13”), also aliased
SHOP_ITEM_MOUN. No separate local type byte chooses a different menu.

| Index/category | Ordered stock symbols (ITEM_ prefix) |
| --- | --- |
|9 /weapon|BROAD_SWORD, BUSTER_SWORD, HEAVY_LANCE, JAVELIN, LARGE_AXE, ROBIN_ARROW, ASSAULT_SHELL, GUARDIAN_STAFF, INDRA_STAFF, POWER_GLOVE|
|27 /item|MEDICAL_HERB, HEALING_SEED, HEALING_DROP, ANTIDOTE, FAIRY_POWDER, ANGEL_WING|

These are actual `data/stats/items/shopinventories.asm` rows; shopInventory
emits count followed by item bytes. ShopMenu conditionally opens the current
portrait, requests 158, closes text/portrait and enters the common selector.
Definitions/prices, gold, member inventory/capacity/equipment, deals quantities
and later prompts feed shared actions. The service owner preserves buy/deals
payment-before-add (then deals removal), sell payment/drop/rare-deals, repair
payment/repair, cancellation and looping. Stock/invocation is not a successful
purchase, free item, affordable service or all-service completion claim.

Priest134 clear 256 requests 1971, sets 256, then calls j_ChurchMenu regardless
of the warning's words; set 256 skips only warning/set 256. Text1970 is uncalled.
`code/common/menus/church/churchactions_1.asm:ChurchMenu` conditionally opens
CURRENT_PORTRAIT, requests 110, closes text/portrait and enters raise/cure/
promote/save. Entry alone does not change EGRESS_MAP, heal everyone or save.
Accepted shared service/H3 seams are not new Ketto runtime observations.

@StartSave zero prompt →@DoSaveGame copies CURRENT_MAP to EGRESS_MAP, sets 399,
calls SaveGame with CURRENT_SAVE_SLOT, then music/input and continue/suspend
prompt. At Ketto it copies 25. Nonzero first prompt does not commit this path;
cancel before Save leaves the former EGRESS_MAP. The accepted savepoint
lookup's `data/maps/global/savepointmapcoords.asm` row is 25,(12,11),Up;
GetSavepointForMap requires 399 for this table path (clear 399 uses the initial
Granseal fallback); its later raft-reset handling remains with the entry owner.
The row is a return datum, not an entry teleport or persistence proof. Post-save
zero exits; nonzero reaches fade/WitchSuspend instead of ordinary callback
return. Other service effects remain with the accepted service owner.

Fresh explicit25 rebuilds NPC origins/actions and clears temporary 256–383
before init. Persistent800 survives: clear 800 leaves origins; set 800 runs
only cs_5D3AA's two setPos calls, with no speed 64, segments, text or headshake.
Fresh relocated merchant therefore has eas_Init's speed 32 policy, not a
persisted 64 guarantee. F256/F257 reset, but800 still overrides the woman's
prompt. CURRENT255 on 25 retains population/temp flags yet runs init: set 800
reapplies positions to existing mappings; clear 800 does not undo relocation.
Scroll skips reset/init. Manually clearing800 can replay relative motion
from changed positions. No reload transaction repairs partial earlier work.

## All descriptions and both items

**Confirmed:** `s4_descriptions.asm` is a 16-byte wrapper at $5D340 followed
by 11 six-byte rows at $5D350 and a two-byte $FD00 terminator. Base$1024=4132.
CheckArea uses the tile ahead in ordinary view-target0, clears name index 1
to 0 and speech SFX to 0, and prioritizes chest/vase/barrel/bookshelf/area item
layout kinds over RunMapSetupAreaDescription. Normal no-entity C supplies
d6=1; front tile/layout remain premises, not facts derived from row coordinates.

DisplayAreaDescription scans packed X/Y in order; every local row has
condition 0/kind 0, so none requires d6=0. It displays 423+investigation offset,
then 4132+description offset, closes portrait/text, and returns d7=-1; no match
returns 0, retaining the caller's fallback distinctions. NAME consumes a
dialogue-name word via GetCurrentDialogueNameIndex/GetCombatantName: here
member0's actual name, not a hardcoded English name. Investigation425 is
`{NAME} investigated{N}the sign.{W2}{CLEAR}`;427 is
`{NAME} investigated{N}the book shelves.{W2}{CLEAR}`. All selected description
literals below were checked in gamescript.txt; this is the bounded local
table, not the complete global assignment graph.

| Row / X,Y | Investigation offset →ID | Description offset →ID | Complete description |
| --- | --- | --- | --- |
|0 /2,10|4 →427|0 →4132|`A book...{N}TRAVELS{W1}`|
|1 /3,10|4 →427|1 →4133|`A book...{N}VILLAGES AND TOWNS{W1}`|
|2 /2,16|2 →425|2 →4134|`It reads...{N}Don't go out!  Or you'll{N}be punished!{N}- Devils{W1}`|
|3 /6,26|2 →425|3 →4135|`It reads...{N}You doubt the punishment?{N}- Devils{W1}`|
|4 /15,26|2 →425|4 →4136|`It reads...{N}Church is closed!{W1}`|
|5 /15,15|2 →425|5 →4137|`It reads...{N}No praying today!{N}- Devils{W1}`|
|6 /14,18|2 →425|6 →4138|`It reads...{N}Pacalon will never come to{N}save you!{W1}`|
|7 /16,19|2 →425|7 →4139|`It reads...{N}Don't work!{W1}`|
|8 /22,29|2 →425|8 →4140|`It reads...{N}Mitula ignores you!{W1}`|
|9 /27,24|2 →425|9 →4141|`It reads...{N}No weapon or item sales!{N}- Devils{W1}`|
|10 /16,28|2 →425|11 →4143|`It reads...{N}Village of Ketto{W1}`|

No local row selects offset 10/text 4142. W2/CLEAR between investigation and
description differs from final W1; prohibitory signs do not gate services.
No map was extracted, so table coordinates alone prove no accessibility,
approach or actual inspected layout kind.

`7-chest-items.asm` is only endWord. `8-other-items.asm` has exactly
(4,22,170,BRIGHT_HONEY), (29,18,171,HEALING_WATER), endWord. Flags 170/171
are collection flags, not temporary 256/257. CheckArea reaches CheckNonChestItem
through masked layout kinds $2C00/$3000/$3400/$1C00, not coordinates alone.
GetNonChestItem selects MAPDATA_OFFSET_ITEM_OTHER; GetItem scans four-byte
rows for coordinate low bytes, applying battle crop only when CURRENT_BATTLE
is not255. No match returns-1. Empty chest data does not disable shared search.

CheckNonChestItem reads the flag; held flag returns ITEM_NOTHING, then sets
the flag again. Clear flag returns the item and sets the flag **before**
inventory success. `explorationfunctions_1.asm:loc_239C8` requests 413 with
NAME index 1=0/ITEM index 2 from selected item. These are ordinary items, not
gold-chest indices. If Bowie holds fewer than four items, AddItem runs;
otherwise UpdateForce and ordered OTHER_FORCE_MEMBERS_LIST select the first
member with room. The actual joined-force list, not only active battle party,
governs that scan, with no HP condition. OTHER_FORCE_MEMBERS_LIST starts one
byte after TARGETS_LIST: it assumes ordinary joined Bowie is the first entry.
If he is not joined, the separate initial slot 0 attempt still occurs, while
the fallback omits the actual first joined member; it does not repair that
supplied state. Success retains the flag, requests
415 or 416 (third name index is recipient), item music and
FadeOut_WaitForP1Input. That helper is a separate music/plain-input boundary,
not an invented W1/W2 in 415/416. Acquisition does not use the consumable.

All-full requests 417 before CloseChest and RefillNonChestItem at saved
candidate coordinates. Refill re-finds the nonchest row and clears its flag,
allowing retry. Already-collected ITEM_NOTHING takes empty-object text, not
a duplicate grant. Interruption between flag set and AddItem/refill can leave
flag without item; after AddItem can leave item without completed presentation.
Normal full-force recovery is not a transaction or completion guarantee.

`s3_zoneevents.asm` has only Map25_DefaultZoneEvent/RTS; `s5_itemevents.asm`
only Map25_DefaultItemEvent0/RTS. Zone wrapper still closes text and waits
a VInt/physical 0 destination. Item-event wrapper clears speech, masks used
item, stores supplied player-Y in EVENT_RELATIVE_POSITION, sets d6=0 and
performs cleanup/wait. This item-use callback is not the other-item collection
table. Default no-ops do not abolish generic handling or unlock 426.

## Conditional return and departure

**Confirmed:** after successful local service/callback cleanup, control follows
the existing caller. Later WaitForEvent with pending 0 selects camera physical 0
and controlled action script: mode 0 character, mode 1 raft, other nonzero
Caravan. Pending nonzero returns that event instead. These are conditional
requests, not observed control or completed text/menu/action/VInt work.

`6-warp-events.asm` contains one row at $A8308 then endWord: wildcard X255/Y31
→no-scroll Map72(31,27),Down3. No 800/256/257/426, item or party predicate.
Wildcard matching requires a qualifying producer; it does not make every
Y31 position instantly leave. Ordinary esc02_controlCharacter requires
masked layout kind $1000 before exploration.asm:WarpIfSetAtPoint. That helper
divides internal coordinates by 384, scans eight-byte rows, accepts negative
source X as wildcard, writes type1/PARAM1–5 and WARP_SFX.

ProcessMapEvent clears type before dispatch. No-scroll consumption submits/
clears SFX, idles physical 0, unwinds exploration, samples coordinates,
replaces nonnegative destination low bytes, applies facing and returns d4=0.
Ordinary sampled high bytes are a premise of nominal(31,27). Mutable pending
replacement/interruption can alter the result. MainLoop then takes SwitchMap
and CheckBattle, not guaranteed exploration.

Accepted Map72 candidates are 8/24/26 in order, unlock 408/424/426, wildcard
trigger(255,255). First unlocked wins; held matching completion 508/524/526
clears that unlock **after selecting the same battle**. Crops are output,
not containment tests; any of these can intercept(31,27). Neither 800 nor
shopping selects 26. Without admission, Map72 population/init still applies,
including F65 Caravan eligibility and independent 716-set/Luke10-clear scene.
Later zone X32/33/34 can invoke CheckRandomBattle26: it tests 526, requests
immediately if clear; if set, nonzero STEP_COUNTER declines, zero counter
draws range 8. Zero requests; nonzero sets counter 2–5 from range 4+2. Request
sets 426 and a CURRENT255 no-scroll request before later services. The next
scan can still select8/24 first. Other cave/default zones retain their accepted
behavior. This proves neither a necessary/unopposed/navigable route nor
Battle26 combat or campaign completion.

## Changed-state acceptance and reproduction

| Changed state / interruption | Required distinction |
| --- | --- |
|Camera, actor order, followers/aliases|Actual activation/reverse lookup determines target.|
|Different approach side|UP1/DOWN3 turn/restore, not approach gates.|
|800/257 combinations|Relocation versus shop; woman's first/repeat versus overriding shop.|
|Yes/No/B; interrupted final text|0 versus-1;257 commits only after branch text returns.|
|Priest256 clear/set|Only warning changes; both invoke church.|
|Merchant text/movement interruption|Temporary speed, committed800 or partial actor writes can remain.|
|Bowie0 versus$8000; dead Bowie|0 waits, high bit skips; facing alone has HP guard.|
|Changed positions/mappings/service progress|Relative endpoints change; no safe absent-actor no-op or bounded-duration promise.|
|Fresh25/CURRENT/scroll;800 manually cleared|Different population/temp/action/init lifetimes; no automatic undo.|
|Shop cancel/gold/inventory/equipment/deals varied|Invocation is not completed transaction.|
|Church cancel/Save/decline/suspend|Savepoint write belongs to selected Save branch.|
|Description coordinate/layout varied|Actual priority and first admitted row govern, not narrative.|
|170/171 held; Bowie/all-force full|No duplicate; first available recipient; refill only after failure text.|
|Collection/presentation interruption|Flag-before-item and item-before-service boundaries remain.|
|Warp request/mode/layout/flags altered|Actual request and ordered72 scan; no unconditional26.|

Load ignored `local/private-inputs.ps1` in each command process. Verify retained
read-only source's clean pinned HEAD. Use the
[retained-pair recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
for map-init equality and _verify_map_script_engine_fixture; preserve pair
size/mtime, without regeneration.

1. Compare all nine entity records and complete target operations with
   `tests/fixtures/h2/map-events/entity-programs.json`, selected routing and
   default zone/item siblings. Follow actual activation/lookup/facing/cleanup.
   Join Event5 speed accesses to `map-event-dialogue-state-static-v1.json`;
   check each gamescript hex key and YesNoPrompt's actual return/consumer.
2. Select retained programCorpus.programs cs_5D3B8/cs_5D3AA. Compare every
   sourceLine/macro/arguments/encodedBytes and all inline data against source;
   follow csc14/29/23/2D/27/19 and action/HP/lookup/wait consumers. Resolve
   actor0 versus$8000 and generated widths/deltas independently.
3. Compare actual stock rows9/27, shop/church entry/Save and Ketto savepoint;
   consume `common-menus-static-v1.json` menuFacts.yesNoPrompt and
   serviceStateMachines. Aggregate counts do not prove private rows/completion.
4. Compare all 11 description rows/macros/literals/base additions; use
   `map-descriptions-static-v1.json` for consumer rules, not local-row proof.
   Follow CheckArea/GetItem/collection/AddItem/full-force/refill for both
   items, and sole warp/request/complete72 candidates/accepted setup-zone
   boundary. Review changed/interrupted states against actual branches.
5. Check complete prose/diff, UTF-8/LF, relative links/anchors, sole-path and
   private boundary, and `git diff --check`. On clean committed head run
   `uv run sf2 verify plan --base origin/main --head HEAD`; record actual
   exact-head CI in the Issue handoff. Documentation-only scope skips blanket
   local normal/full/engine/Godot/H3/H4 suites.

**Unknown:** natural interaction/accessibility; supplied flags/party/HP/class,
lookup/camera/action/mode and pending state; full services, actor settling,
presentation/timing, usable control and save/load persistence. These limits
are not an automatic runtime/acquisition queue.

Preserve [#993 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/993#issuecomment-6181265165)
and [PR996 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/996#issuecomment-6181273945):
original/final identities, pair/CI receipts, Windows separator assertion
failure/correction, follower C/D158/159 correction and discovery diagnostics,
plus linked #989/predecessor failures/Unknowns. This slice corrects the
nonwaiting premise and distinguishes zero-based shop indices from comments;
it rewrites no original evidence. Retain source/configuration/pair/results,
freeze clean pushed Draft PR/Project Review, and leave independent acceptance
to main-gate. No merge, cleanup, archive or next Issue.
