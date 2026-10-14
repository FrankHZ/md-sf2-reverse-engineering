# North Cliff Opening and Rohde Recruitment

When North Cliff's cave guards admit the party, carrying both the Cannon and
Dynamite selects a different scene from the missing-item warning. The caller attempts to remove one of each,
then stages the cannon blast, changes the entrance layout and joins Rohde.
Rohde asks to use the weapon, but the scene offers the player no yes/no choice. Joining does not transfer or equip the inventory Cannon.

**Confirmed static:** consumption attempts, live map mutation, membership,
withdrawal, following and caller flags are ordered effects with different
conditions. **Inferred:** with the usual distinct cast, Rohde opens the North
Cliff passage and volunteers to fight with the cannon. **Unknown:** natural
arrival, supplied state, completed presentation, usable control and save/load
persistence. This guide follows the complete scene, local return and fresh-map
consequences, then identifies the next entrance warp. It stops before the cave
interior or Battle25 admission.

## Evidence and Reading Boundary

The accepted Layer A owner is
[North Cliff opening and Rohde recruitment](../../research/north-cliff-opening-and-rohde-recruitment.md),
accepted through [#985 / PR987](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/987).
It pins USA retail SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
and [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6).
Source paths are relative to `disasm/`; Fnnn means flag nnn. The roadmap owns
the [Standard scope agreement](../documentation-roadmap.md#current-baseline-and-near-term-synthesis).
This is an English explanation of accepted evidence, adding no original
finding or remake behavior.

[Battle24 outcomes through North Cliff preparation](battle24-outcomes-through-north-cliff-preparation.md)
explains the preceding outcome, exploration return, cave admission and
missing-item retry. The accepted
[inventory search](../../research/new-granseal-inventor-and-dynamite-gift.md),
[item transaction seams](../../research/map-event-item-transactions.md),
[Caravan consumers](../../research/caravan-boarding-and-southwest-parmecia-travel.md),
[request state](../../research/map-event-request-state.md) and
[request consumption](../../research/map-event-request-consumption.md)
owners supply shared mechanisms. Detailed scene and callee facts here consume
#985's complete selected review. Public structural fixtures alone do not prove
the private scene, text or layout artwork.

## Two Items Are Checked, Then Searched Again for Removal

**Confirmed static:** the Map72 cave callback first rejects F750 set, then
F805 clear, then F256 set. Its specific zone rows cover X25–30 at Y37 before
the default random-battle row. A rejected specific callback does not fall
through to the default. Actual walking/layout/request production remains a
condition; standing at a coordinate alone does not prove the event occurs.

On admission it clears 256, searches Cannon `$72`, then Dynamite `$74`, testing
each full result word against `$FFFF`. If either predicate fails, the accepted
warning names Dynamite and consumes neither item. No battle-completion,
Astral-presence, HP, class or party-capacity test supplements these guards.

When both searches succeed, the caller attempts Cannon removal, attempts
Dynamite removal, runs `cs_4FFDA`, sets 750, then 256. It ignores both removal
return values. It passes item IDs, rather than preserving either predicate's
holder or slot, so each removal searches the current inventory again.

The selected service is `code/common/stats/iteminventory.asm:RemoveItemFromInventory`
at `$4F542`, selected by section07. Its jump aliases lead to section02's
`itemstats.asm` removal/stat chain. The similarly named
`stats/items/itemfunctions_s7_0.asm` is not this selected implementation.

The search rebuilds joined force IDs in ascending order, including reserves
and dead members. For each carrier it counts non-NOTHING entries across four
slots, then searches count-1 down to0, matching entry masked by `$7F` against
the unmasked requested word. The first matching member and highest examined
matching slot win. Unjoined carriers are excluded; a gap can hide a higher
slot; an empty joined list lacks a safe initial loop guard. Success returns a
holder byte without normalizing all of d0, plus the slot word. The caller's
full-word failure test must not be replaced by a simplified signed-byte test.

For a found item, `RemoveItemBySlot` reads the full slot word. NOTHING returns
status3; otherwise `RemoveAndArrangeItems` shifts every following whole word
left and fills the final slot with NOTHING, returning0. Slot3 simply becomes
NOTHING. There is no cursed-item or equippability veto. Shifted equipped and
broken bits travel with the item words. Both paths then recompute the holder's
stats from base/status and remaining equipped items, including curse state.
Even the NOTHING path invokes that update. The wrapper maps status3 to -1,
otherwise0, and restores its saved d1/d2 words; it does not return the removed
holder/slot as an equipment handoff.

With unchanged packed inventory and valid force state, this removes one Cannon
and one Dynamite, potentially from different reserve/dead carriers. Duplicate
copies remain. Dynamite is searched after Cannon's compaction. If custody
changes or a later removal fails, the scene still runs when the calls return.
An interruption after the first removal can leave only that item consumed.
There is no pair transaction, rollback, repair or automatic reacquisition.
The later narration naming LEADER as donor does not identify the actual holder.

## The Cast Is Current State, Plus Three New Allocations

**Confirmed static:** use A=FOLLOWER_A30, B=FOLLOWER_B31, P=Peter7,
R=Rohde11, O=object128 and E=effect129 below. These are logical handles.
The scene neither reloads Map72 nor repairs Bowie/A/B/P. With only ordinary
overworld flags65/67/70 and no conflicting rows, physical 0 is main, A maps to
Caravan1, P to2 and B to Astral3. F805 alone does not guarantee those actors.
Other enabled follower rows can add actors or replace A/B mappings.

Lookup masks the handle to a byte, subtracts96 for enemy-bit handles
(128→32,129→33), reads a physical byte and scales it by 32. A missing entry
cleared to0 aliases physical 0; hidden `$FF` is not safely rejected. Aliases
can overwrite a shared actor's destination, facing or script. Logical B's
lookup 31 is separate from the raft's lookup 63/physical 31.

The first scene commands unconditionally allocate R, O and E at `(63,63)`,
with R facing Up and O/E Left. There is no existing-mapping, joined, HP or
capacity guard. The allocator scans
lookup 0–62 for the largest signed physical byte, adds one and replaces the
selected handle's mapping. Lookup63's raft is excluded. Ordinary maximum 3
therefore yields physical 4/5/6; changed mappings change that result. Repeated
calls can replace mappings while leaving older physical actors behind. They
do not reuse or deduplicate an existing Rohde or promise unlimited capacity.

Declaration sets position/destination in 384-unit tiles, zeroes velocity,
travel and acceleration, installs asynchronous Init and requests property
refresh. For R, `GetAllyMapsprite` selects Blue Flame first for out-of-battle
HP0; otherwise unjoined11 selects NPC_Rohde170 and joined 11 uses the actual
class/table, including BRGN22→sprite26. Other supplied classes follow the
same algorithm. O/E use OBJECT4=181 and EFFECT3=178. The later join does not
refresh R's current mapsprite, so portrait/speech can still use the earlier one.

Init requests speed32/32, zero acceleration factors, acceleration/deceleration
off, obstruction/collision and auto-facing on, Up, immersion off, size 24,
refresh and Idle. Its asynchronous execution and subsequent replacement
scripts are distinct. The scene explicitly waits for P, B and A Init; it
stops O/E animation and waits for their auto-facing-off scripts. Stop/start
change animation counter -1/0, not movement enablement. These facts do not
prove every actor began settled or every sprite service completed.

## Staging and the Cannon Blast

The accepted program has 114 top-level records including its terminator,
covering `$4FFDA..$5023E`/612 encoded bytes. Its
[complete command inventory](../../research/north-cliff-opening-and-rohde-recruitment.md#complete-scene-program)
accounts for every command and inline body. The connected phases below retain
the meaningful order without treating encoded bytes as a gameplay duration.

**Confirmed static:** the scene sets ordinary text cursor3341 and prepares
the actors above. The camera requests `(21,33)`. A, B and P receive nonwaiting
destinations `(27,39)`, `(28,37)` and `(25,37)`; Bowie receives `(26,37)` with
a position wait. Destinations write travel and signed velocity magnitude32.
Bowie's arrival and the following fixed60-tick script wait do not join the
other three arrivals. Aliases can overwrite earlier destinations.

O is positioned `(27,36,Left)`. Bowie, P and B face Up; after 50 ticks B faces
Down and calls Rohde in 3341. Rohde answers3342. R is positioned `(27,39,Right)`
and receives waited `eas_4536C`: move one tile along current facing, resize,
warp sound, sizes 12/14/16/18/20/22 with refresh/wait1 each, resize off, global
size 16, refresh, destination wait and Idle. After 3 ticks R faces Up, B asks
3343/3344 and R answers3345.

Given the admitted living R placement and completed movement, that script
requests `(28,39)`. R's waited Up1/Left2 then requests `(26,38)`. Bowie/P face
Down, R faces Up, Bowie nods, and narration3346 names the Dynamite donor.
R's waited Right1/Up1 then requests `(27,37)`. These coordinates are conditional
derivations: dead R can skip guarded positioning/facing while unguarded actions
still install. SetPos writes position/destination/facing and refreshes but does
not clear every velocity/travel field. Facing guards also distinguish living
Bowie/P/R from B; nod itself has no HP guard and performs its own10/20/10 sleeps,
transformed sprite/DMA and refresh.

Bowie/P/B face Up for3347, Rohde's stand-back line. The camera requests `(21,32)`.
O receives waited BumpDown: auto-facing off, relative Down1/wait5, Up1/wait5,
zero displacement, auto-facing on and Idle. These are timed recoil steps from
the then-current position; zero displacement is not restoration of a saved
coordinate. O's settled return is not established by reaching Idle alone.

E is positioned `(27,35,Left)`. A waited custom action sets speed80/80 then
Idle. Its following waited Up2 is a compiled movement with a destination wait,
conditionally reaching `(27,33)`. The scene sets quake amplitude 3 directly,
changes E to FLAME1=171 with a VInt/refresh, starts its animation, installs
nonwaiting AnimSpeedx2 and requests BATTLEFIELD_DEATH sound 116. After 40 ticks it
copies the entrance cells, hides E, sets quake0 and waits 60 ticks. These are
sound/sprite/quake/display requests, not an observed successful blast. There
is no explicit map fade or palette-flash command.

The live copy is one layout cell wide and two high: current RAM cells
`(0,0)/(0,1)` → `(27,33)/(27,34)`, on 64-column word rows. `CopyMapBlocks` copies
layout words and the command sets both view-plane update bits. It does not
write ROM or directly copy VRAM tiles. Exact artwork/collision contents are
not established here; repeated calls read the then-current mutable source
cells. This change precedes recruitment and the later F750 write.

The movement distinction applies throughout: compiled cardinal actions insert
relative displacement and a destination wait before Idle. Their flags mask
`$9F` clears map collision and bypasses the shared other-entity obstruction
check while bit5 remains clear. The nine inline bodies contain12 movements;
they allocate 150 generated bytes, separately from 42 encoded inline bytes.
ExecuteMapScript resets that action scratch cursor on entry. Action installs
are unguarded by HP and write the resolved physical index into the action-wait
timer; waiting forms poll the script pointer for Idle. Nonwaiting installs
can be replaced before completing. Waiting for Idle does not
universally mean waiting for rendered animation or a whole assembled cast.

## Rohde Volunteers, Then Joins

After the copy, B makes a waited Up1 move and says3348. After 40 ticks R receives
a waited Jump and says3349. P/Bowie face Right and B faces Left. P and Bowie
receive nonwaiting JumpLeft while B receives waited JumpRight. Ordinary Jump
uses Up1/Down1 with 5-tick waits; the side jumps use diagonal offsets with 6-tick
waits, then zero displacement, auto-facing on and Idle. These offsets use
current positions and actual flags/obstruction behavior. Waiting for B does
not join P/Bowie's actions or prove exact restoration.

R is explicitly repositioned six times, each followed by 25 ticks:
`(26,36,Right)`, `(27,37,Up)`, `(28,36,Left)`, `(27,37,Up)`,
`(26,36,Right)`, `(27,37,Up)`. R says3350, faces Left, asks3351/3352 and
says3353. Join follows, then 3354. His questions are dialogue, with no
YesNoPrompt or branch on an answer anywhere in the program.

**Confirmed literal context:** the complete ordinary cursor is3341–3356.
B usually represents Astral, but the actual current B/R mapsprite supplies
portrait and speech. Astral209 gives Astral/bleep6; NPC_Rohde170 or sprite26
gives Rohde/bleep5; alternate sprites use their actual row/default. Existing
open portraits can bypass opening while speech lookup still occurs. The
narration has no speaker and speech0. The source wording and substitutions are:

| Text | Context | Literal wording |
| --- | --- | --- |
|3341|B, single|`{NAME;11}, {NAME;11}!{W1}`|
|3342|R, single|`Yes?{W1}`|
|3343|B, next|`You know about devices like{N}this, right?{W2}`|
|3344|B, single|`{LEADER} has a cannon and{N}some dynamite here.  Can you{N}remove those rocks?{W1}`|
|3345|R, single|`Ah...OK, let me try.{W1}`|
|3346|No speaker, single|`{NAME;11} gets the dynamite{N}from {LEADER}.{W1}`|
|3347|R, single|`Stand back.  Ready?{W1}`|
|3348|B, single|`Perfect!  We can now go to{N}North Parmecia.{W1}`|
|3349|R, single|`Wow!  This cannon is great!{W1}`|
|3350|R, single|`I can attend the battles{N}with this weapon.{W1}`|
|3351|R, next|`{LEADER}, do you need{N}this any more?{W2}`|
|3352|R, next|`May I use it?  Huh?{W2}`|
|3353|R, single|`Wow, now I can fight along{N}side you!{W1}`|
|3354|R, single|`Please call me when you{N}need my help.{W1}`|
|3355|B, single|`{NAME;11} is hooked.{N}Ha, ha!{W1}`|
|3356|B, single|`OK, {LEADER}.  Let's go{N}to North Parmecia!{W1}`|

`{NAME;11}` uses Rohde's current name. LEADER rebuilds force and names the
first active member, independently of actual item custody or mapped physical
Bowie. There is no ITEM substitution. N is newline. W1 accepts held directions
or A/B/C after a VInt, not Start; W2 uses continuation/input and validation
sound. Non-skipped commands wait scrolling, display and increment the cursor.
Single text additionally closes portrait/text and sleeps 10; next text leaves
them open. These waits advance text rather than answer Rohde's questions.

At the join, `csc08` waits scrolling, clears speech, requests MUSIC_JOIN and
calls `JoinForce(11)`. It sets joined 11, rebuilds force lists, then sets
active 43 through JoinBattleParty if active count is below 12. At 12 or more it
skips that write without clearing an existing 43. Already-joined reserve Rohde
can become active when room exists; already-active Rohde stays active even
in an overfull supplied state. The helper's UpdateForce precedes the active
write, with no second rebuild; a later UpdateForce refreshes those cached lists.

Join does not test HP/class, revive, heal, initialize class/inventory/equipment,
evict a member, transfer Cannon, set follower84 or change current mappings.
It then reads R's actual class and displays 446:
`{NAME} the {CLASS} {N}has joined the force.` The name/class indices are 11
and that class. This repeats even for an already-joined Rohde.

The selected `FadeOut_WaitForP1Input` requests previous music after the current
one and waits for CURRENT_PLAYER_INPUT: held directions/A/B/C/Start suffice.
It is neither a palette fade nor a fresh-button-edge or yes/no wait. It closes
text, sleeps 10 and returns with the ordinary cursor still 3354. Membership,
announcement completion and later active-party use are separate boundaries.

ExecuteMapScript clears the text-skip latch on entry. Debug-enabled P2 Start
can latch it: ordinary text bypasses display and **does not increment its
cursor**; csWait sleeps also skip. A partial skip can therefore leave later unskipped text behind the nominal3341–3356 sequence. Join has no
ordinary-text skip check: its music, membership,446, input wait and Sleep10
still execute. Internal nod/shrink/jump sleeps, movement/Idle/destination waits,
copy, follows and flags remain. Skip is not cancellation or guaranteed completion.

## Withdrawal Uses Physical Caravan State

**Confirmed static:** after 3354, O receives waited auto-facing-off, then
nonwaiting Down2. R receives waited Down1, then nonwaiting Down1. After 3 ticks
R receives nonwaiting `eas_452BA`, then another 3-tick spacer. O receives
nonwaiting Down1, waits 3, receives the same shrink script, then waits 3.
O's second movement can overwrite its first. The spacers do not prove arrival.

The shrink script waits 6, enables resizing, requests warp sound, steps global
sizes 22/20/18/16/14 with refresh/wait1 each, then 12/refresh, waits destination
and clones position/destination/travel/facing from **physical 1** before Idle.
It does not resolve logical A and does not disable resize. Changed physical 1
can alter the withdrawal independently of the actor currently mapped as A.
There is no final wait for either shrink script before R/O are hidden.

Hide sets physical position/destination to `$7000,$7000`, changes every matching
lookup entry to `$FF` and removes the first matching exploration-list entry
by shifting through its sentinel. It does not clear the action pointer,
travel/velocity, HP, inventory, membership or follower-buffer allocation.
Aliases share the hide; old actors orphaned by a replaced mapping are separate.
Crucially, ordinary VInt updates skip entity data and action dispatch for
signed X >= `$7000`. A retained hidden pointer therefore does not continue
shrinking/cloning under that update until position becomes eligible again.
No exact final global size, completed shrink or settled departure is promised.

After hiding R/O, B makes waited Down1/Left2 and says3355, faces Down and
says3356. Bowie/P face Down, Bowie receives nonwaiting Jump and P waited Jump.
Again, P's wait does not establish completion of Bowie's action.

The final follows install A→Bowie slot 2 offset `(-24,0)`, P→A slot 1 `(0,-24)`
and B→A slot 3 `(0,24)`. This branches from A. The handler's alive guard reads
the high byte of follower words `$001E/$0007/$001F`, so **all three test Bowie0
HP**, not their named follower. Dead Bowie skips all three after earlier
unguarded work. Current aliases can overwrite scripts or create self-follow;
unrelated followers are not removed.

Each admitted AddFollower copies 42 bytes and writes its physical target and
offsets before deduplicating only the exploration-list append. Three commands
consume 126 bytes with no capacity guard or settling wait. ClearEntities resets
the walking buffer; ExecuteMapScript's separate action scratch reset and hide
do not reclaim it. Repeated direct scenes can consume more buffer even for
already listed actors; membership/flags alone do not make them safe to repeat.

## Local Return, Interrupted Prefixes and Fresh Entry

**Confirmed static:** the reached order is live layout copy → joined 11 and
conditional 43 → withdrawals/follows → unlock 425 → scene return → caller 750 →
256 → zone physical 0 position wait → conditional control. `setStoryFlag25`
adds 400 to set 425. It neither starts Battle25 nor sets completion 525.

The scene terminator conditionally waits scrolling if dialogue is open, clears
VIEW_SCROLLING_SPEED and restores registers. The zone wrapper closes
portrait/text, waits a VInt and joins physical 0's position. It does not join
all actors/actions. ProcessMapEvent cleared pending before dispatch;
WaitForEvent restores camera 0 and the mode-selected controller only when
pending is zero. Pending overwrites, mode and unfinished services can change
continuation. Neither scene nor caller saves or proves usable control.

An interrupted prefix keeps its reached effects. Items can be removed before
actors exist; quake/flame can precede the copy; the entrance can change while
750 remains clear; membership can be written before withdrawals or caller
latches;425 precedes750, and 750 precedes256. Set750 can suppress later cave
callbacks even if 256 was never written. There is no rollback or repair, and
a later flag does not certify successful presentation of earlier requests.

Fresh Map72 loading reads blocks/layout anew, then its single F750 flag-copy
record reconstructs `(27,33)/(27,34)` from reloaded `(0,0)/(0,1)`. The live
scene instead copies current mutable cells and explicitly sets view update
bits. The fresh loader uses normal map display/plane initialization. Equal
coordinates do not guarantee equal content after arbitrary live edits.

Fresh explicit entry clears temporary 256–383 and clears entities, lookup and
buffers before repopulating followers/main. Map72 has one default setup and
no fixed residents; it does not freshly create scene O/E. Rohde's fresh
follower row tests **84**, independently of joined 11,750 or 425. With84 clear,
joined Rohde is absent from that population; with 84 set and 11 clear, he can
still be declared using current HP/class/joined sprite selection. Scene,
join and caller never write 84.

F65 selects overworld Caravan/A and falls through the ordinary follower table;
67 Peter,70 Astral/B,84 Rohde and conflicting A/B rows are tested in order.
The enabled rows govern physical 1 onward and current lookup overrides; main0
is separate. Membership is not a substitute for these story predicates.
Raft/mode exceptions retain the preparation guide's boundary. Arbitrary actor
counts or buffer capacity are not validated here.

Map72 init still tests 716 then joined-Luke10 and runs the Luke scene only for
716 set/10 clear. It does not replay the blast or use 750 to repair actors.
No-battle CURRENT255 reload loads layout/init while retaining actors and 256;
border scrolling retains actors/latch and skips init/reset. Walking or acquiring
items alone does not clear 256. Fresh entry clears 256, but750 still suppresses
the cave callback. With750 clear after interruption, fresh entry can permit a
retry against remaining inventory; it restores neither consumed item.

The next static route is warp `(27,33)` → Map48 `(55,29)`, facing Up, with
no-scroll semantics. The scene does not directly warp there or prove natural
arrival/event production across the copied cells. Cave interior, Battle25
admission/roster and later travel remain separate scope; concurrent #989 is
not evidence for this guide.

| Changed state | Consequence to preserve |
| --- | --- |
|750 set /805 clear /256 set|Ordered early return; direct scene calls bypass these caller guards.|
|Duplicate, gapped, reserve/dead or unjoined custody|Actual joined/count-limited search selects one copy; later removal searches again.|
|Removal failure or interruption between removals|Later work can proceed; retain consumed/compacted prefix without pair restoration.|
|Already joined/mapped R or repeated direct scene|New allocations/remapping and join announcement still occur; capacity is conditional.|
|Missing, hidden or aliased current handles|No safe rejection/repair; destinations, scripts, hides and follows can affect shared/unintended entries.|
|Dead R or changed class|Sprite/guarded position/facing differ; unguarded actions and join remain.|
|Dead Bowie with living P|All final follow guards skip; earlier unguarded scene effects remain.|
|Active count11 versus12+, or existing43|Conditional active write, no eviction/clear, and cached-list ordering remain distinct from joined11.|
|Logical A differs from physical1|Final following and shrink clone use different targets.|
|Partial skip or unfinished action/input/scroll service|Ordinary cursor can lag; join/input/internal waits and reached state work remain.|
|Interruption after copy/join/425/750/256|Different current/persistent prefixes, with no presentation/save guarantee.|
|750 set,84 clear; or84 set,11 clear|Opening reconstruction, fresh following and membership remain independent.|
|CURRENT / scroll / fresh72|Different layout, actor, latch and allocator lifetimes.|

**Unknown:** natural reach/event/route and exact cell artwork; supplied flags,
inventory/packing, HP/class/membership, mode/mappings/actor order and allocator
capacity; completion of text/input/music/sound/sprite/DMA/scroll/actions and
following, visible blast/withdrawal/settling, usable control and save/load
persistence. These are retained limits, not an automatic runtime queue.

## Evidence Review and Retained Results

The complete accepted #985 owner was reviewed with #986's preparation guide.
The selected public
[map-events fixture](../../../tests/fixtures/h2/map-events-static-v1.json)
and composed loader retain the 29-operation cave callback, six cave-row
references and ordered guards/searches/removal calls/scene/flags. The
[item-transactions fixture](../../../tests/fixtures/h2/map-event-item-transactions-static-v1.json)
anchors the selected service aliases and the `$4FECC/$4FED4` callsites. Its
caller records do not prove the callee's compaction/stat semantics. The
[map-init fixture](../../../tests/fixtures/h2/map-init-static-v1.json)
retains the six-operation716/Luke path, not an automatic replay of the blast.

The [map-script fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json),
its schema and actual verifier/loader consumers retain command-family/handler
facts, including actual guards and follow's packed-byte access, plus corpus
summaries. They do not publish this detailed private scene. Full 114-command/
612-byte accounting,150 generated action bytes, literals, selected removal/
join bodies, hidden-X updates, fresh copy and follower84 consume #985's
accepted selected source/pair review. Structural counts/schema success are
not fresh independent proof of that detail. No in-scope contradiction was
found; any material contradiction returns to research.

Owning commands remain `uv run sf2 h2 map-events`, `map-event-item-transactions`,
`map-init` and `map-script-engine` under that prefix, and Layer A's linked
retained-pair recipe. Source/pair are absent in this synthesis environment
under the accepted boundary. Selected public schema/consumer checks and prose
review are not a fresh comparison. No acquisition, regeneration, runtime,
installation or new maintained tooling is part of this slice.

[#985 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/985#issuecomment-6180719770)
and [PR987 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/987#issuecomment-6180735932)
retain head `02991c0d`, tree `8c4eca33`, base `e96f6faa`, successful
CI38653869752 (scope15s/research33s), author 0.082s unchanged 18,285,979-byte
pair comparison,114 commands/612 encoded bytes/150 generated action bytes.
The initial LF-only assertion failed after the semantic checks; only the
note's CRLF was normalized before the bounded check passed. Guessed source
paths, wrong command-record display keys and an item-fixture `expected`
KeyError were corrected to the actual paths/fields without changing evidence.
The direct VInt review corrected the draft hidden-pointer execution implication;
X=$7000 suspends ordinary updates. These completed diagnostics remain retained.

[#986 original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/986#issuecomment-6180738037),
[rebased handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/986#issuecomment-6180773526)
and [PR988 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/988#issuecomment-6180781344)
retain original `f7c38893`/tree `a4e11c72`/CI38653916276 (scope18s), final
`a8c678d5`/tree `3e30b70f`/base `1238d4d5`/CI38654343691 (scope32s),
byte-identical five documents and 530 links. The ignored scratch check's
arbitrary Markdown length cap was removed without shortening the prose.
The preparation guide's
[retained review](battle24-outcomes-through-north-cliff-preparation.md#evidence-review-and-retained-results)
keeps #981/#982 identities, pair/CI results, literal/field/opcode/obstruction
corrections and #977's Map68 P2, plus linked predecessor failures and Unknowns.
Completed suites/failures are neither reset nor repeated for replacement receipts.

The #990 Issue/PR handoff records full prose/diff/navigation/link/anchor,
UTF-8/private/exact-five-path checks and `git diff --check`, same-process
configured clean committed planning and actual exact-head CI. Documentation
scope uses direct checks without blanket normal/full/Python/.NET/Godot/H3/H4/
translation suites. Independent acceptance and integration remain with main-gate.
