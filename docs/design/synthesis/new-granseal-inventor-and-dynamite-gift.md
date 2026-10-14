# New Granseal Inventor and Dynamite Gift

In New Granseal, the inventor has an explosive that could clear the North
Cave rocks, but warns that using it would also blow up its user. Later he
requires Astral's permission. Once the return-audience flag is set, an actual
Cannon in the joined force's searchable inventory changes the conversation:
the inventor recognizes a safe delivery method, follower B nods, and the
inventor offers Dynamite. The earliest joined member with room can receive it.
Full inventories defer the gift and leave it available to retry.

**Inferred:** the story presents Astral authorizing an explosive for use with
the Cannon. **Confirmed:** permission flags, actual inventory, the mapped
actor's gesture, item insertion, receiving announcement and caller completion
are distinct. This interaction neither consumes the Cannon nor opens the cave.
**Unknown:** natural reach, supplied state, completed presentation/services,
usable control and save persistence.

This continues [New Granseal return and Astral following](new-granseal-return-and-astral-following.md),
using [accepted #969 research](../../research/new-granseal-inventor-and-dynamite-gift.md)
and predecessors on main `36b0026bca3f7aaa5dee2b1d366312534000c522`.
The guide ends at the local inventor interaction return and necessary
fresh-entry consequences. Janet recruitment and the North Cave blast have
separate boundaries. This Layer B explanation selects no remake rule or new
observation work.

## From Castle to Inventor

**Confirmed:** the [selected return and population owner](../../research/new-granseal-inventor-and-dynamite-gift.md#castle-exit-current-town-and-fresh-population)
identifies Map7's south row at any X/Y23: no-scroll town Map6 `(11,1)` Down.
The wildcard does not prove passability at every X. Movement must produce
the layout warp request, and that request must survive the pending slot until
dispatch. Coordinate low-byte replacement retains the accepted high-byte
premise. Town6 has no flag-switched-map or battle-coordinate row. This is a
conditional connection, not proof of a naturally walked route to the inventor.

Ordinary fresh exploration admission clears pending state, applies the
accepted living/immortal healing with its HP/status limits, rebuilds actors
and sprites, clears temporary flags256–383, sets F80 and runs init. Story
flags, force membership and player mode remain supplied state. A callback
in the current town does not repeat that admission or rebuild its followers.

Town setup selects default, then F701, then F702, with the last applicable
selector winning. F802/F804/F805/F70 do not select its resident table. The
inventor is dwarf reference141 in all three lists: default/F701 place him
at `(14,27)` Right; F702 places him at `(24,7)` Down, all with slower Init.
The earlier [town population guide](new-granseal-town-through-peter-introduction.md)
owns the remaining cast. All three entity-event row13 entries call
Map6_EntityEvent13; default row byte0 and later row byte1 govern the wrapper's
facing policy, rather than prescribe Right or Up as a final facing.

Shared town init gives F802 priority, placing priest132 at `(11,18)` Down
and returning. With F802 clear/F706 set, it instead places worker138 at
`(63,63)` Down and guard139 at `(15,23)` Down. Both clear returns. None
relocates the inventor, grants an item or admits an adviser.

The castle audience's current Astral remains resident139 after becoming a
follower; F70 does not rename him B31. A fresh town rebuild consumes the
F70/B/Astral declaration before residents. Earlier F74/B/Astral or F68/B/Oddler
allocations can coexist, and the later F70 declaration overwrites the B lookup
without deduplicating physical followers. Other enabled declarations affect
allocation and order. F70 clear therefore does not prove B absent; F805 does
not repair F70. Town139 is a guard, distinct from castle Astral139 and B31.

## Earlier Warning and Permission

**Confirmed:** the [complete callback](../../research/new-granseal-inventor-and-dynamite-gift.md#complete-callback-and-nod-scene)
first checks F802. With it clear, F702 selects the earlier explosive warning
(1063–1065); F702 clear gives the inventor's pondering line1055. The warning
names the cave rocks, then retracts the offer because of danger to the user.
Even supplied F804/F805 cannot override this earlier branch.

With F802 set, F804 is checked before F805. F804 set gives1122, asking for
the cave rocks' removal, without searching inventory or offering a replacement.
With F804 clear/F805 clear,1114–1115 say Astral forbids use and permission is
needed. No Cannon search or B lookup occurs in that branch.

| Selected state | Local conversation or attempt |
| --- | --- |
| F802 clear, F702 clear/set | Pondering / earlier warning; no gift. |
| F802 set, F804 set | Cave-removal request, regardless of F805 or current items. |
| F802 set, F804 clear, F805 clear | Permission required; no inventory test. |
| F802/F805 set, F804 clear, Cannon search fails | Inventor asks Astral; B's context says it is dangerous and needs safe detonation. |
| F802/F805 set, F804 clear, Cannon search matches | Cannon suitability, B's nod, offer and capacity-dependent receiving attempt. |

Permission is scripted dialogue, not a yes/no prompt. The local admission
has no F761, F70, actual-Astral, living-character or active-party prerequisite.
F805 is a predicate, not verification that the intended adviser exists now.
The caller never recruits anyone, changes F805/F806, requests travel/battle,
consumes the Cannon or checks Dynamite uniqueness.

## What Counts as Having the Cannon

**Confirmed:** [inventory search and recipient analysis](../../research/new-granseal-inventor-and-dynamite-gift.md#inventory-predicate-and-admitted-recipient)
separates joined-force membership from active battle membership. UpdateForce
scans member IDs0–29 in ascending order and puts every joined member into
TARGETS_LIST, including reserve and dead members. Active flags make separate
party/reserve lists. Neither selected item service filters HP, class, current
map visibility or following; B31 is outside that joined-member scan.

GetItemInventoryLocation rebuilds that list, then examines members in ascending
order. It counts occupied entries across all four word slots but searches only
slots `count-1` down through0. Each entry's index is masked with `$7F` before
comparison with the unmasked request word, here Cannon `$72`. Equipped/broken
entry bits therefore do not obstruct this exact request. The first matching
member wins, and that member's highest examined matching slot wins. Exhaustion
returns full-word `-1`; the caller compares `d0.w` with that sentinel and
ignores the returned holder/slot. A byte test or Boolean is not this contract.

Packed inventory makes the counted prefix cover its occupied slots. Gapped
inventory need not: `[other, NOTHING, NOTHING, Cannon]` has count2, so the
search examines1,0 and misses3. A Cannon held only by an unjoined member is
also excluded. Search does not compact or consume inventory. F761's earlier
Cannon-gift history cannot replace this current search, and F761 clear cannot
block a searchable Cannon. Both selected loops assume a nonempty joined list;
their body-first DBF form has no empty-list guard. Safe not-found/all-full
behavior is not guaranteed for that malformed premise.

## Suitability, Nod and Offer

**Confirmed:** with a matching Cannon,1119 says it would be suitable. The
callback closes dialogue, runs the three-command cs_54CC6, then1120 says
permission was given and offers the explosive. That short scene sets B31
facing Up, nods B31 and ends. It has no dialogue, permission menu, flag or
inventory operation of its own.

B is a lookup selector, not a physical slot or guaranteed Astral. Resolution
masks to a byte and scales the mapped physical index by32. A missing ordinary
lookup can alias physical0; a hidden `-1` mapping has no safe absence guard.
Changed or duplicate mappings change the actor actually used. The byte-facing
command's dead-ally guard applies below30, so31 bypasses it even if mapped to
dead Bowie. The word-operand nod has no alive-status guard. It requests
animation-counter changes, Sleep10/20/10, transform/DMA and sprite update,
then counter0; it neither verifies Astral's sprite nor restores the old counter
or waits all motion. Visible nod completion remains Unknown.

On a failed Cannon search,1116 asks Astral, then the callback borrows B's
portrait/speech context for1117–1118 about danger and safe detonation. The
[context owner](../../research/new-granseal-inventor-and-dynamite-gift.md#text-borrowed-context-and-receiving-acknowledgement)
distinguishes intended Astral209 (portrait31/bleep6) from the actual mapped
sprite. Missing/changed B affects speech and portrait as well as staging.
The callback copies current speech before borrowing but does not restore that
copy before return. Closing dialogue alone is not speech restoration or
portrait closure.

The success nod scene resolves no speech/portrait. Thus1120 and the receiving
helper inherit the inventor wrapper's context: ordinary dwarf226 has no
portrait and bleep6. Neither the nod nor the receiving announcement switches
to a narrator or makes B the item recipient.

## Receiving Room and Acknowledgement

**Confirmed:** the caller requests Dynamite `$74` through ReceiveMandatoryItem
with option `d1=0`. The helper rebuilds the joined force and chooses the
earliest member whose count across all four slots is below4. That member can
be dead or in reserve, and need not be Bowie, the Cannon holder, active leader
or first follower. A gap still provides receiving capacity.

AddItem searches all four slots from0 upward for the first entry whose masked
index is NOTHING. It stores the item masked with `$807F`; this request inserts
plain Dynamite. Its own result is0 for insertion or1 for no slot.
ReceiveMandatoryItem does **not** inspect that result before announcing the
gift or later returning0. Under valid unchanged inventory, the prior count
provides an empty slot; that premise must not collapse the two result contracts.
Neither routine rejects already-owned Dynamite or duplicate Cannon.

The helper writes the recipient and item name indices after AddItem, requests
item music27 and displays213, announcing NAME receiving ITEM. NAME uses the
selected combatant's name, not LEADER; ITEM uses Dynamite. This is inherited
inventor speech/portrait context, not an explicit narrator selection. The
announcement has NAME/newline/ITEM substitution and no W1/W2 of its own.

The [accepted Cannon gift](creed-departure-dwarf-recovery-and-cannon-gift.md#accepting-or-deferring-the-cannon)
instead checks only Bowie's four slots, inserts for fixed combatant0 and uses
1728/Sleep100 before F761. This gift searches the joined force for receiving
room and uses213 plus two input calls, with caller F804 after helper return.
Both leave AddItem's result unchecked and supply no atomic save boundary.

After213, the helper calls j_FadeOut_WaitForP1Input and then a direct
WaitForPlayerInput. The first preserves `d0.w`, requests previous music through
PlayMusicAfterCurrentOne and waits input before restoring it. Despite its
name, it performs no palette fade. The sound service submits its commands
and polls with Sleep3; a completed command boundary does not establish audible
jingle completion. These are two sequential held-input services: each accepts
a currently held direction/A/B/C/Start, with no release or new press required
between them. One held input can satisfy both. Only afterward does the helper
close dialogue, clear `d0.w`, restore saved registers and return; `d1` is restored
to the caller's0, not retained as an item or recipient result.

If every joined member's inventory is full, the helper returns1 immediately
for option0. It performs no AddItem, item jingle,213 or forced discard. The
alternative nonzero-option discard path is not admitted by this caller.
The caller tests bit0: odd result displays1729 and leaves F804 clear; normal0
sets F804. Its predicate would accept2, but this option0 call cannot normally
select that other path. Full hands therefore permit a later eligible retry.

## Dialogue and Local Return

**Confirmed:** callback lines use explicit-ID `txt`, not cutscene text-cursor
increments. The accepted text joins retain all13 callback literals plus213,
including warning1063–1065, permission1114–1118, suitability1119, offer1120,
full-hands1729 and completed-branch1122. Newlines and W1/W2 remain text controls;
W1 accepts directions/A/B/C after its VInt wait, whereas the two direct
receiving services also accept Start. W2 has continuation/validation-sound
behavior. None creates a permission choice.

Direct `txt`/`clsTxt` do not use the cutscene skip latch. The nod script clears
that latch at entry but contains no skippable text or csWait. Its end
conditionally waits view scrolling when dialogue is open, clears scrolling
speed and returns. Skipping a scene is not a basis for omitting direct gift
text or input services.

The [entity wrapper and ordered return](../../research/new-granseal-inventor-and-dynamite-gift.md#ordered-return-retries-and-changed-state)
reselects the current setup/event table, resolves the interacted actor and
saves its mapped physical identity, facing and options. Row bit0 optionally
waits a VInt and faces that actor opposite the supplied relative direction;
default0 disables it, later1 enables it. It clears VInt_UpdateEntities for the
callback. These inventor rows have bit1 clear, so no saved-facing restoration
is requested. On callback return it closes portrait, closes dialogue,
reactivates entity updates, restores registers and returns. It does not restore
borrowed speech/name RAM, wait all actors or prove the next usable control read.
This entity cleanup is distinct from the predecessor's zone-wrapper waits.

## Changed State, Repeats and Interrupted Prefixes

**Confirmed:** the item write precedes announcement, music/input services,
helper0, caller bit0/F804 and wrapper return. The offer text alone establishes
none of insertion, completion flag or persistence.

| Changed state or stopping point | Consequence |
| --- | --- |
| F804 set, even F805 clear or Dynamite lost | Completed branch wins; no missing-item repair or reacquisition. F802 must still be set to reach it. |
| F761 set but Cannon absent/missed, or F761 clear with searchable Cannon | Actual search selects refusal / gift attempt independently of history. |
| F70 clear/F805 set or missing/aliased B | Gate still passes; actual lookup controls nod/speech. No automatic safe-adviser rejection. |
| Bowie full, later joined reserve/dead member has room | Earliest joined member with room receives; all-full defers without F804. |
| Existing Dynamite with F804 clear | Another can be inserted if Cannon and capacity admit the attempt. A forced repeat with F804 cleared has no uniqueness protection. |
| Interrupt after AddItem but before helper0/F804 | Item can exist while F804 remains clear and acknowledgement is incomplete; later retry can insert another. |
| Interrupt after helper0 before F804 | Helper returned, but no caller flag yet. |
| Interrupt after F804 before wrapper return | Gift branch can be suppressed while cleanup and usable control remain incomplete. No rollback repairs the prefix. |
| Fresh town entry after F804 | F701/F702 still select population; F802 selects init and F70 independently contributes B. F804 only changes this callback branch. |
| Current town with changed setup/follower flags | Table selection can change while actual actors retain earlier mappings until a separate rebuild. |

Janet's zone admission is an independent F805-set/F806-clear boundary; F804
is not its prerequisite. This guide does not explain that recruitment scene,
walk the next journey or consume Dynamite/blast North Cave. Natural reach,
supplied mode/flags/force/packing/actors, completed entity/DMA/text/portrait/
music/input services, visible authorization/receipt, usable control and
save/load persistence remain **Unknown**, without an automatic runtime queue.

## Evidence Review and Retained Results

**Confirmed review boundary:** this synthesis checks the accepted #969 owner
and predecessor population, actor, item, input and music owners against selected
public evidence. `sf2-map-event-item-transactions-static-v1`
([fixture](../../../tests/fixtures/h2/map-event-item-transactions-static-v1.json),
[schema](../../../schemas/h2/map-event-item-transactions-static-fixture.schema.json),
[consumer](../../../src/sf2tool/h2/map_event_item_transactions.py)) retains the
49-operation physical caller, service aliases `$81D0`→`$9146` and `$4F48A`,
Cannon/Dynamite request registers and sentinel/bit0 branch edges. Its40-record
default context shares the physical tail but enters at an immediate return;
contextual duplicate call records are not two executions. The schema asserts
record shape; the consumer checks operand identity and ordered joins. Neither
asserts the callee's full inventory algorithm or natural interaction reach.

Selected `sf2-map-events-static-v1`, `sf2-map-init-static-v1` and
`sf2-map-script-engine-static-v1` fixture/schema/consumer inspection checks
town callback routing, setup/init priority and facing/nod operand/handler
boundaries. Callee algorithms and literal/control-token joins remain #969's
accepted pinned-source findings. No contradiction was found in these selected
public surfaces. Owning fresh-source commands remain `uv run sf2 h2 map-init`,
`uv run sf2 h2 map-events`, `uv run sf2 h2 map-script-engine` and
`uv run sf2 h2 map-event-item-transactions`. The pinned upstream/retained pair
are absent in this synthesis checkout: no acquisition, regeneration or fresh
comparison is claimed.

[#969 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/969#issuecomment-6179831287)
and [PR971 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/971#issuecomment-6179847971)
retain head c4c138e4/tree02f3543e/baseaf7a1e65, CI38647099720 (scope16s,
research32s), author0.081s and independent0.086s comparisons of the unchanged
18,285,979-byte pair, complete49/40 operation joins and14 text joins.
[#970 updated handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/970#issuecomment-6179905336)
and [PR972 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/972#issuecomment-6179913615)
retain original5625ee54/CI38647220538 (scope19s), final5849263a/treec56b4c2c/
base9c65746c/CI38647686363 (scope11s),529 links and independent review. Its
base update/separate-scope clarification was not a research correction.

The predecessor's [retained review](new-granseal-return-and-astral-following.md#evidence-review-and-retained-results)
keeps #965's Battle19/32-byte-stride corrections, distinct pair receipts,
original/final CI, linked failures/diagnostics/Unknowns, #954 callback correction
and #962 Fairy correction. All completed results remain retained; no failure
is reset or described as interrupted. Any evidence contradiction returns to
its research owner without expanding Layer B ownership.

This slice's Issue handoff records direct full-prose/diff, links/anchors,
navigation, UTF-8, exact ownership/private boundary and `git diff --check`, the
configured clean committed planner and actual exact-head CI. Documentation-only
policy calls for those direct checks, with no blanket suite or translation run.
