# New Granseal inventor and Dynamite gift

Evidence date: 2026-10-14. **Confirmed:** the inventor callback gives F802
priority, then F804 before F805. With802/805 set and804 clear, it searches the
joined force's inventories for Cannon; a match leads to follower B's nod and
an attempted Dynamite gift. The receiver selects the first joined member with
capacity, acknowledges the gift, returns0, and only then does the caller set804.
All-full returns1 and leaves804 clear. Neither F761/F70 nor an actual Astral
entity is an admission test, and Cannon is not consumed here.
**Inferred:** dialogue presents Astral authorizing the explosive for the Cannon.
**Unknown:** natural reach, supplied state, completed services/presentation,
usable input and save persistence.

## Agreement and provenance

Standard rigor under [Issue #969](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/969)
and [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
one castle-to-town inventor/gift outcome, with distinct story predicates,
actual actor mapping, inventory search/capacity, result and return boundaries.
This document alone is owned. Dependencies are accepted
[castle return/Astral following](./new-granseal-return-and-astral-following.md),
[earlier town admission/setup](../eastern-parmecia/new-granseal-town-through-peter-introduction.md),
[Cannon gift](../southern-travel-creed/creed-departure-dwarf-recovery-and-cannon-gift.md#cannon-helper-and-complete-digging-scene),
[item-transaction caller seam](../../world/map-event-item-transactions.md),
[common stats/inventory](../../party/common-stats.md),
[request state](../../world/map-event-request-state.md) and
[request consumption](../../world/map-event-request-consumption.md).
No unmerged work is evidence. Stop at local entity-interaction return and
relevant fresh-entry consequences. No full town NPC catalog, Janet recruitment,
North Cave use/blast, subsequent travel/battle, acquisition/runtime, tooling,
schema/fixture/index, engine/UI or Chinese changes are included.

Source paths are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Reused fixtures bind USA retail
ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Unqualified setup files mean `data/maps/entries/map06/mapsetups/`;
map/entity engines mean `code/common/scripting/map/` and
`code/common/scripting/entity/`; constants come from `sf2enums.asm` and RAM
symbols from `sf2const.asm`. Fnnn denotes flag nnn; intervals are half-open.

Minimum evidence is selected pinned source, existing public fixtures and the
retained18,285,979-byte map-init/map-script-engine pair. One linear comparison
is budgeted below one minute/512 MiB, with small local receipts and prose.
No regeneration, installation or cleanup is needed. Replan material source
contradictions or broader machinery. Counts locate records; complete ordered
joins and consumer inspection support the static conclusions below.

## Castle exit, current town and fresh population

**Confirmed:** Map7's `6-warp-events.asm` ($9AFF6..$9B008), zero-based row1,
is `mWarp255,23; warpNoScroll; Map6; destination11,1; facingDown3`.
The X byte is a wildcard, not evidence that every X on Y23 is passable.
The movement/layout `$1000` seam must reach WarpIfSetAtPoint, and its event1
request must survive in the single pending slot until consumption clears it.
The no-scroll consumer idles physical0 and returns Map6 `(11,1)`, Down through
MainLoop. Coordinate low-byte replacement retains the accepted sampled-high-
byte premise. Map6 has no flag-switched-map or battle-coordinate row.

Ordinary exploration admission clears pending, performs the accepted living/
immortal heal with its HP/status limits, selects/rebuilds entities and sprites,
clears flags256–383, sets80 and runs init. Story flags and player mode remain
supplied state; the warp does not normalize mode or force membership. A current
Map6 callback does not itself repeat that admission or rebuild followers.
Natural travel from the castle endpoint to this interaction is not proved.

Map6 setup selection is default, then701, then702, with the last set selector
winning. F802/804/805/70 do not select the town table. All three pointers share
zone and init. The relevant resident is dwarf reference141 in every list:

| Setup | Relevant declaration / callback table |
| --- | --- |
| Default | Dwarf `(14,27)`, Right, eas_InitSlow; Peter plus19 NPCs. Entity-event row13 addresses141, flags byteRIGHT=0. |
| F701 | Dwarf `(14,27)`, Right, eas_InitSlow;14 NPCs. Row13 addresses141, flags byteUP=1. |
| F702 | Dwarf `(24,7)`, Down, eas_InitSlow;14 NPCs. Row13 addresses141, flags byteUP=1. |

These row bytes are consumed as facing-policy bits, not a final desired
compass direction. All three rows target Map6_EntityEvent13, physically defined
in `s2_entityevents_701.asm`, including the separate702 event table. The common
default target is Map6_DefaultEntityEvent. The earlier population owner retains
the other residents; no full town callback catalog is required here.

Shared `ms_map6_InitFunction` ($54E54) has eight operations: check802; if set,
script cs_54E70 and return; otherwise check706, optionally script cs_54E78,
then return. cs_54E70 places132 `(11,18)` Down. cs_54E78 places138 `(63,63)`
Down then139 `(15,23)` Down. These are current lookup writes, no hiding,
flag writes, inventor relocation, grant or adviser admission.

The accepted castle scene's current Astral is resident139, appended as a
follower; setting70 does not rename that live actor to B31. Ordinary town
rebuild instead consumes `table_Followers`' row `70,FOLLOWER_B,MAPSPRITE_ASTRAL,1`
before residents. Earlier74/B/Astral or68/B/Oddler allocations may also exist;
later70 overwrites the B lookup, without deduplicating those physical followers.
Other enabled rows affect physical allocation/order. F70 clear does not prove
B absent because those other declarations can supply it;805 set does not repair70.
Town139 is a guard, distinct from castle139 and from B31. No physical slot
number, actual Astral, living Bowie or active-party composition is guaranteed
by the inventor's flag tests.

## Complete callback and nod scene

**Confirmed:** `s2_entityevents_701.asm:461–532`, Map6_EntityEvent13
$54C10..$54CC6, has49 ordered operations. The nested default context starts
at$54C30 with `rts`; its40-record contextual extent includes the later branch
blocks but default entry immediately returns. It is not another gift call.
The following is the complete control flow; source branch labels preserve
shared-tail and branch-polarity identity.

| Block | Exact ordered work and successor |
| --- | --- |
| Entry$54C10 | chkFlg802; bne.w byte_54C32; chkFlg702; bne.s byte_54C24; txt1055; bra.s Map6_DefaultEntityEvent. |
| byte_54C24 / default | txt1063; txt1064; txt1065; Map6_DefaultEntityEvent: rts. |
| byte_54C32 | chkFlg804; bne.w byte_54CC0; chkFlg805; bne.s loc_54C4C; txt1114; txt1115; bra.w loc_54CBE. |
| loc_54C4C | moveq ITEM_CANNON,d1; jsr j_GetItemInventoryLocation; cmpi.w -1,d0; bne.s byte_54C8E. |
| Cannon absent fallthrough | txt1116; clsTxt; copy CURRENT_SPEECH_SFX word to SPEECH_SFX_COPY; move.w FOLLOWER_B,d0; jsr GetEntityPortaitAndSpeechSfx; copy d1 word to CURRENT_PORTRAIT; copy d2 word to CURRENT_SPEECH_SFX; jsr DisplayCurrentPortrait; txt1117; txt1118; clsTxt; bra.s loc_54CBE. |
| byte_54C8E | txt1119; clsTxt; script cs_54CC6; txt1120; clsTxt; move.w ITEM_DYNAMITE,d0; moveq0,d1; bsr.w ReceiveMandatoryItem; btst0,d0; bne.s byte_54CBA; setFlg804; bra.s loc_54CBE. |
| byte_54CBA | txt1729; fall through to loc_54CBE. |
| loc_54CBE | bra.s return_54CC4. |
| byte_54CC0 | txt1122; fall through to return_54CC4. |
| return_54CC4 | rts. |

Thus802 clear retains the older702 dialogue even if804/805 are set. With802
set,804 bypasses both805 and inventory search. With804 clear/805 clear,
permission text appears without inspecting Cannon or B. With805 set the
actual inventory result, not761 or a historical gift announcement, selects
the branch. F70, actual adviser identity and HP are never checked here.
There is no Cannon removal, Dynamite uniqueness test, warp, battle request,
recruitment,805/806 mutation or yes/no decision.

cs_54CC6 ($54CC6..$54CD0), source534–536, is exactly three commands:
`setFacing FOLLOWER_B,UP` ($23, byte31, byte1;4 bytes),
`nod FOLLOWER_B` ($26, word31;4 bytes), `csc_end` ($FFFF;2 bytes).
It contains no text, flag, permission prompt or inventory operation.

csc23's byte actor guard skips dead named allies below30;31 bypasses that
HP guard, even if its actual lookup aliases dead Bowie. csc26 reads a full
word and has no alive-status guard. GetEntityAddressFromCharacter masks the
selector to$FF, resolves ENTITY_INDEX_LIST[31], and scales the physical index
by32 (`ENTITYDEF_SIZE_BITS=5`). Missing ordinary lookup0 can therefore act on
physical0; a hidden -1 lookup has no safe-absence guard. Duplicate/changed
mappings affect the actual actor instead of preventing the grant.
Nod writes animation counter-1, Sleep10, mapsprite transform/DMA, Sleep20,
sprite update, Sleep10, then counter0. It does not validate an Astral sprite,
wait for all movement or restore a previous animation counter. These are
requested operations, not a measured visible nod.
ExecuteMapScript clears its skip latch at entry; this scene has no skippable
text/csWait. End conditionally waits view scrolling if dialogue is open,
clears VIEW_SCROLLING_SPEED, restores registers and returns.

## Inventory predicate and admitted recipient

**Confirmed:** the existing `map-event-item-transactions-static-v1.json`
`eventItemTransactions.serviceEntries` pins j_GetItemInventoryLocation at$81D0
→ GetItemInventoryLocation$9146 in `code/common/stats/itemstats.asm`, and
ReceiveMandatoryItem$4F48A in `code/common/stats/iteminventory.asm`.
Section02 includes the first file; section07 includes the second. The similarly
named implementation under `stats/items/itemfunctions_s7_0.asm` is excluded.
The accepted caller fixture does not itself assert callee algorithms; the
selected bodies and consumers below supply this document's bounded analysis.

`battleparty.asm:UpdateForce` scans ally IDs0–29 in increasing order, testing
joined flags0–29. Every joined member enters TARGETS_LIST, whether active,
in reserve, alive or dead. Active flags32–61 determine separate battle-party/
reserve lists, not inclusion in TARGETS_LIST. It writes actual list lengths
and preserves its saved registers. Neither selected item service filters HP,
class, map visibility, current follower status or the active party's size.
B31 is a follower alias, outside this joined-member scan.

GetItemInventoryLocation saves d2–d3/d6–a0, copies requested d1.w to d3,
rebuilds the force and loops TARGETS_LIST in ascending joined-ID order.
For each byte ID it reads slot0/count; if count is zero it advances. Otherwise
it examines slots `count-1` down through0, masking each entry with$7F before
comparing it with **unmasked requested word d3**. The caller supplies exactly
ITEM_CANNON=$72, so equipped/broken entry bits do not obstruct this match.
First matching member wins; within that member the highest examined slot wins.
On success d0.b is the member and d1.w the slot; d0's other bits are not
normalized. Exhaustion writes full-word-1 to both. The callback tests d0.w
against-1, not a byte or a Boolean, and ignores the returned member/slot.

`GetItemBySlotAndHeldItemsNumber` uses the combatant record (56-byte stride),
items offset32 and four word slots. It returns the requested slot's unmasked
entry in d1 and counts all four entries whose `$7F` index is not NOTHING=$7F
in d2. Counting all four is **not** the same as searching all four: with packed
inventory, count n means occupied slots0..n-1; with a gap, a Cannon in slot3
can be missed when n<4. For example `[other,NOTHING,NOTHING,Cannon]` has n=2,
so the search examines1,0 and misses3. A Cannon belonging only to an unjoined
member is also excluded. The search neither compacts slots nor consumes items.
Both service loops assume at least one joined member: length0 becomes-1 before
a body-first DBF loop, with no empty-list guard. Do not extrapolate a safe
"not found"/"all full" result to that malformed force premise.

ReceiveMandatoryItem saves d1–d5/a0, keeps item$74 in d4 and caller option0 in
d5, calls j_UpdateForce again, then scans the rebuilt TARGETS_LIST from its
first member. It clears d0.w before each member byte and d1.w for slot0,
calls the same count helper and uses unsigned `d2.w < COMBATANT_ITEMSLOTS(4)`
to enter @ReceiveItem. Thus the earliest joined member with capacity wins,
including reserve/dead members; it need not be Bowie, the Cannon holder,
the active leader or the first follower. A gapped inventory still has capacity.

At @ReceiveItem it puts d4 in d1 and calls j_AddItem. AddItem scans all four
slots in ascending order, masking with$7F to find the first NOTHING. It masks
the inserted word with$807F (index plus broken bit; equipped/other bits clear),
stores it and returns d2.w=0; no slot returns1. This call's$74 stores plain
Dynamite. It preserves d0, while d1 carries the item after masking. Neither
routine rejects already-owned Dynamite or duplicate Cannon. With valid,
unchanged inventory, the preceding count guarantees an empty slot. However,
ReceiveMandatoryItem **does not test AddItem's d2 result** before announcement
or its own later0 return; those are distinct result contracts. At helper return
d1 is restored to the caller's0, not retained as an item or recipient result.

The accepted Cannon helper instead counts only Bowie's inventory, uses a
word equality-to4 capacity rejection, and calls AddItem for fixed combatant0.
Its acknowledgement is1728/Sleep100 and its flag is761. This helper uses
joined-force capacity,213 and the two input calls below; caller804 follows
its return. Neither helper checks AddItem's result or supplies an atomic save.

## Text, borrowed context and receiving acknowledgement

**Confirmed:** all13 callback txt literals and the admitted helper213 literal
join `data/scripting/text/gamescript.txt` by exact four-hex-digit keys.
The table paraphrases content and preserves ordered control tokens; every
site is explicit-ID `txt`, not a cursor increment.

| ID / context | Content | Ordered controls |
| --- | --- | --- |
| 1055 / inventor | Ponders | `{W1}` |
| 1063 / inventor | Invented an explosive | `{N}{N}{W2}` |
| 1064 / inventor | Could clear North Cave rocks | `{N}{N}{W2}` |
| 1065 / inventor | Retracts because user would also be blown up | `{N}{W1}` |
| 1114 / inventor | Astral forbids Dynamite use | `{N}{W2}` |
| 1115 / inventor | Needs permission to give it | `{N}{W1}` |
| 1116 / inventor | Asks Astral for permission | `{N}{W1}` |
| 1117 / B mapping | Dangerous as it is | `{N}{W2}` |
| 1118 / B mapping | Needs safe detonation | `{N}{N}{W1}` |
| 1119 / inventor | Cannon would be suitable | `{N}{W1}` |
| 1120 / inventor | Says permission was given and offers explosive | `{N}{W1}` |
| 1729 / inherited helper-return context | Full hands; deferred gift | `{N}{W1}` |
| 1122 / inventor | Requests removal of cave-blocking rocks | `{N}{W1}` |
| 213 / inherited receiving context | NAME receives ITEM | `{NAME}{N}{ITEM}`; no W1/W2 |

The entity wrapper resolves the actual interacted actor's mapsprite. Dwarf226
has no portrait and speech bleep6. The absent-Cannon branch closes dialogue,
copies the current speech word, then resolves B31 and writes both portrait and
speech. Actual Astral209 gives portrait31/bleep6; changed/missing/aliased B uses
its actual mapped sprite. DisplayCurrentPortrait opens only a nonnegative
portrait. This branch has no restore from SPEECH_SFX_COPY before return;
closing dialogue alone is not speech restoration or portrait closure.

The present-Cannon nod scene never resolves speech/portrait, so1120 and the
receiving helper inherit the inventor wrapper's context. The helper writes
DIALOGUE_NAME_INDEX_1=recipient d0.w and INDEX_2=item d1.w after AddItem;
DisplayText resets the substitution pointer, NAME consumes the first word
through GetCombatantName and ITEM consumes the next through GetItemName.
Item names at indices$72/$74 are Cannon/Dynamite. Recipient naming is not
LEADER substitution and does not identify B31 as receiver. No explicit speech0
or narrator portrait selection is made for213.

The admitted success sequence is exactly: AddItem; write the two name indices;
submit MUSIC_ITEM=27; txt213; j_FadeOut_WaitForP1Input; direct WaitForPlayerInput;
clsTxt; clear d0.w; restore saved d1–d5/a0; rts. The alias resolves through
`s05_jumpinterface.asm` to `battlefunctions_0.asm`: preserve d0.w, request
previous music$FB via PlayMusicAfterCurrentOne, call WaitForPlayerInput, restore
d0.w, return. The music service submits WAIT_MUSIC_END and the requested command,
then polls WAIT_FOR_MUSIC_END with Sleep3. Its name does not imply a palette
fade, and command completion does not establish audible jingle completion.
There are **two sequential input-service calls**, but no required release or
new press between them: each accepts a currently held direction/A/B/C/Start,
otherwise waits VInt and retries. A held input can satisfy both.

On all joined inventories full, the helper sets d0=1 and tests d5.w. Here d5
is0, so it returns immediately through saved-register restoration: no AddItem,
item jingle,213 or forced discard. The alternative nonzero-option path's214,
DiscardItem,215 and result2 are outside this call's admission. The caller's
bit0 predicate would accept2, but this call cannot select that path normally.
No yes/no choice belongs to the inventor's permission or this receiving flow.

`txt` invokes Trap5_TextBox, which preserves registers and displays its explicit
ID; `clsTxt` passes$FFFF and closes dialogue. They do not use the cutscene skip
latch. N is newline; W1 accepts directions/A/B/C (not Start) after its VInt wait,
and W2 supplies the continuation/validation-sound path. Text asking permission
is scripted conversation, not a choice service. All actual input, speech,
portrait, text and sound completion remains unobserved here.

## Ordered return, retries and changed state

**Confirmed:** the item write precedes213, the music/input waits, helper0,
caller bit0 test and F804. The caller's odd result branch displays1729 without
setting804. On the normal0 path it sets804, branches through loc_54CBE and
returns. Dialogue1120 alone establishes neither insertion nor804 nor persistence.

RunMapSetupEntityEvent reselects the current setup/event table, resolves the
interaction speaker, and saves the mapped physical actor/entry facing/options.
Its row flag bit0 optionally waits a VInt and turns that physical actor opposite
the supplied relative direction. For later tables byte1 enables this; default
byte0 does not. It clears VInt_UpdateEntities before invoking the callback.
Bit1 would restore the saved facing after return; these141 rows have bit1 clear,
so no such restoration is requested. It then closes portrait, closes dialogue,
reactivates VInt_UpdateEntities, restores registers and returns. This does not
restore borrowed speech/name RAM, wait all actors or verify the next usable
control read. Unlike the accepted zone wrapper, this is an entity wrapper.

| Changed state or interruption | Consequence at this boundary |
| --- | --- |
| F802 clear, any804/805 | Earlier702/1055-or1063–1065 branch; no inventory service or new804. |
| F802 set and804 set, even805 clear/Cannon absent |1122 only; no repair or reacquisition if Dynamite was lost. |
| F802 set,804 clear,805 clear, any inventory |1114/1115 only; neither actual Astral nor Cannon supplies the missing predicate. |
| Eligible flags,761 clear, a searchable Cannon | Present-Cannon branch is still admitted.761 is not consulted. |
| Eligible flags,761 set, no searchable Cannon | Absent-Cannon branch; historical Cannon grant is insufficient. |
| Cannon only beyond the count-limited range or on an unjoined member | It can be missed; packed/force premises matter. |
| Bowie full, later joined member has space | Gift goes to the earliest eligible later member; reserve/HP0 is allowed. All-full gives1729 and permits a later retry with804 clear. |
| Existing Dynamite,804 clear | No uniqueness check; another item can be added if capacity and Cannon predicate permit. |
|70 clear/805 set, or missing/aliased B | Permission gate still passes. Sprite/facing/nod/speech context follows actual lookup; there is no safe missing-adviser rejection. |
| Interrupt after AddItem but before helper0 or804 | Dynamite may exist with804 clear, and music/dialogue/input may be incomplete. A later eligible retry can add another. |
| Interrupt after helper0 but before804 / after804 before wrapper return | Respectively no flag yet / flag can suppress another gift while cleanup/control remains incomplete. No rollback or atomic save boundary. |
| Fresh town entry after804 | Unchanged701/702 select residents and802 selects init.804 changes this callback branch, not population, admission or follower construction. |
| Current town with changed setup/follower flags | Callback table is reselected but entities are not rebuilt by this callback. New selector policy may act on old mappings until a separate reload. |

Janet admission is only the next boundary: Map6 zone rows `(19,29)`/`(20,29)`
reach Map6_ZoneEvent3, requiring805 set/806 clear independently of804. This
document does not execute or analyze that recruitment scene or North Cave
Dynamite consumption/blast. The local gift does not open the cave.

**Unknown:** natural route/interaction reach; supplied flags/mode/force membership,
HP/inventory packing/capacity, actor aliases/sprites and follower order; completion
of entity, DMA, text, portrait, music and input services; visible authorization/
receipt, usable control and save/load persistence. None is an automatic runtime
or acquisition queue, and no new shared inventory contract is claimed.

## Reproduction and review boundary

Load ignored `local/private-inputs.ps1` in the same process as controlled commands.
Reuse the [retained pair recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init equality, `_verify_map_script_engine_fixture`, and `_cutscene_facts`.
Preserve both payloads and all completed receipts; no extraction rerun is needed.

1. Join selected Map7 south warp, Map6 setup pointers and row13 in all three
   entity tables, relevant141 populations, follower B declarations, full shared
   init body and both init scripts to retained route/program facts.
2. Join every operation/line/operand in entity-programs.json's49-record
   Map6_EntityEvent13 and40-record shared default context; join the selected
   item-transaction physical program, two service entries/calls and result
   predicates. Do not count contextual shared tails as separate execution.
3. Join all three cs_54CC6 records, opcode/operand widths and10-byte extent;
   all13 callback literals plus213, exact IDs/controls and item names. Review
   byte/word actor guards, lookup aliases, portrait/speech and direct-text
   consumers, wrapper bits and return ordering.
4. Read the included service bodies and aliases through UpdateForce, count,
   search, AddItem, substitution, music/input and full helper return. Check
   joined/reserve/dead members, packed/gapped inventories, all-full, repeat,
   missing-B and interrupted prefixes against the actual instructions.
5. Check complete prose/diff, local links/anchors, UTF-8, sole-path scope and
   private/generated boundary; run `git diff --check`. On a clean committed
   head run `uv run sf2 verify plan --base origin/main --head HEAD` and record
   actual exact-head CI in the Issue handoff. Documentation-only policy uses
   direct local checks, not blanket normal/full/engine/Godot/H3/H4 suites.

Preserve [#965 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/965#issuecomment-6179605996)
and [PR968 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/968#issuecomment-6179629067):
all distinct0.087/0.081/0.080s pair receipts, independent unchanged-pair0.080s,
Battle19/32-byte-stride corrections, initial/final CI, source-location/shape/
truncation diagnostics and linked #961/#957/#953/#949/#945/#941/#937 failures,
results and Unknowns. Retain #954's callback and #962's Fairy corrections via
those linked handoffs. The preflight's nonexistent inventory-and-items owner
was replaced by the accepted map-event-item-transactions source seam, not a
new contract. No completed failure is erased or reclassified as interrupted.
Freeze a clean pushed Draft PR for independent review; no merge or cleanup.
