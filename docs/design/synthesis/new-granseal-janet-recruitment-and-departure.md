# New Granseal Janet Recruitment and Departure

Janet finds the party as it prepares to leave New Granseal. She asks about
Elric, explains that she wants to accompany him, and insists on coming despite
the warning about the journey. The party accepts her; a recruitment
announcement follows, then the scene requests a new local following chain.
**Inferred:** this is a willing reunion and acceptance, expressed by scripted
dialogue and gestures. There is no yes/no menu or player refusal branch.

**Confirmed static:** the town encounter needs F805 set and F806 clear at one
of two selected zones. Janet's force membership, active-party membership,
allocated map entity, current following, caller completion and later usable
control are separate changes. The preceding
[inventor conversation](new-granseal-inventor-and-dynamite-gift.md) is a reading
connection: obtaining Dynamite is not a Janet prerequisite. **Unknown:** natural
reach, supplied state, completed presentation/services, usable control and
save/load persistence. This explanation ends at local return and necessary
fresh-town consequences; the south exit identifies the next route only.

## Evidence and Reading Boundary

The accepted Layer A owner is
[New Granseal Janet recruitment and departure](../../research/new-granseal-janet-recruitment-and-departure.md),
accepted through [#973 / PR975](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/975).
It pins USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
and [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6).
Source paths below are relative to its `disasm/`; Fnnn denotes flag nnn.
The roadmap owns this slice's [Standard scope agreement](../documentation-roadmap.md#current-baseline-and-near-term-synthesis).

Read the [town/Peter guide](new-granseal-town-through-peter-introduction.md)
and [Astral return guide](new-granseal-return-and-astral-following.md) for
the existing population, follower declaration order and audience. The
[membership contract](../contracts/party-membership-state.md) owns joined/active
state and rebuild chronology; the [roster contract](../contracts/party-roster-state.md)
owns map-script command boundaries. Janet's accepted research adds the selected
numeric values and actual callee review without broadening those contracts'
older symbolic or handler-local scopes. Research describes the original;
this guide chooses no remake behavior.

## Finding the Party in the Current Town

**Confirmed static:** Map6 zone rows `(19,29)` and `(20,29)` select
`Map6_ZoneEvent3`. In `data/maps/entries/map06/mapsetups/s3_zoneevents.asm`,
the complete callback at `$54D24..$54D3C` reads F805, returns if clear, reads F806,
returns if set, calls `cs_55288`, sets F806 and returns. Both rejected paths use
`Map6_DefaultZoneEvent`, the shared final `rts` at `$54D3A`, without recruitment.
F806 is written after the scene returns, not when its first text appears.

The predicate tests no F804, Cannon, Dynamite, F70, Elric membership, HP or
party capacity. Neither the plot's statement about Elric nor the earlier
Cannon/Dynamite transaction adds such a test. Supplied805/806 can admit or
suppress the encounter independently of those histories.

A coordinate match needs a real event producer. Walking must admit movement
and reach the layout-attribute `$1400` path at
`entityscriptengine_2.asm:loc_5294`, which submits event6. The pending request
is a single mutable slot; its survival and the movement/layout conditions are
premises. `ProcessMapEvent` clears the request type before the zone handler
installs `eas_Init` on physical0, loads the coordinate words and invokes
`RunMapSetupZoneEvent`. That wrapper selects the current setup/table but does
not rebuild actors. Directly invoking the scene bypasses the zone predicate
and does not itself write caller806. See the accepted
[request-state](../../research/map-event-request-state.md) and
[request-consumption](../../research/map-event-request-consumption.md) owners.

Map6's ordered default/F701/F702 setup selection and independent F802-priority,
then F706 init still apply. All three setups share this zone table and init.
With F802 set, init moves132 to `(11,18)`, Down; otherwise F706 moves138 to
`(63,63)` and139 to `(15,23)`, Down. These are current lookup writes. Neither
805 nor806 chooses a population or builds Janet. The default has fixed Peter7
at `(18,25)`, Up, unless an existing follower7 mapping suppresses it; the701/702
resident lists have no fixed Peter. Their139 is a **town guard**, declared
at `(11,12)` Left, `(13,24)` Up or `(18,28)` Up respectively. It is distinct
from castle Astral139 and from B31, even when the narrative adviser is Astral.

Fresh town population builds enabled ordinary followers before residents and
physical0. F67 supplies Peter; F74/B/Astral, F68/B/Oddler and later F70/B/Astral
can each allocate a follower. Later B declarations overwrite the character
lookup without removing earlier physical entries. Thus F70 clear alone does
not prove B absent; F805 alone does not establish an Astral mapping. Other
enabled followers affect allocation/list order. Current actors can retain an
earlier setup's mappings until a separate rebuild. The linked population
owners supply the full order without turning this into a town catalog.

## Janet's Request and the Party's Acceptance

**Confirmed static:** `data/maps/entries/map06/mapsetups/scripts.asm:cs_55288`
has 45 records and three inline action sequences, spanning
`$55288..$5537E` (246 bytes). It has
no scene branch. The story below accounts for its complete staging order.
B means follower slot B (31), P means Peter7, J means Janet18 and Bowie
means ally0; every selector operates on its current physical mapping.

The scene sets text cursor1126 and allocates J at `(16,22)`, facing Down.
It installs and waits for `eas_Init` on B, then P. Janet's first text says
she has found the leader and asks who they are. Bowie and B receive jump
actions without a wait; P receives a jump action whose completion is awaited.
Guard139 is asked to face Up and the camera is sent to `(14,17)`. Waiting for P alone
does not establish that every jump completed.

Bowie is sent to `(19,28)`, B to `(20,28)` and P to `(20,27)`, then all three
are asked to face Up. Janet walks **Down2, Right3, Down1** and the camera moves
to `(14,22)`. B turns Left, asks her identity, then turns Up. Janet names Elric
as her love and asks whether he is with them. Peter says he joined willingly.
Janet shakes her head and explains that she wants to accompany him, not stop
him; she then pleads to be taken along.

Peter turns Down, the scene requests 50 ticks, and he walks **Right1, Down1**
before facing Up. B warns Janet about the journey. Janet advances **Down1**
and insists despite the warning. Bowie turns Right, B turns Left and accepts
her as tough enough. Bowie nods, Bowie and B face Up, and Janet thanks the
party. The scene then executes `join J`, followed by the three follow calls
and end. The warning and acceptance belong to B's dialogue selector, while
the nod belongs to Bowie; actual mappings still govern their physical actors.

These are source requests with explicit limits. `eas_Init` sets speed32 and
size24, resets acceleration, enables obstruction and auto-facing, disables
map/entity uncollidability, faces Up, updates the sprite and reaches Idle.
The jump disables auto-facing, requests relative `(0,-1)`, wait5, `(0,1)`,
wait5, `(0,0)`, then restores auto-facing/Idle. Inline actions generate
tile-sized relative moves (384 internal units) and wait for the addressed
action pointer to become Idle. Absolute `setDest` writes scaled destinations
and signed velocity32/travel and waits for position equality; it has no HP
guard here. Camera commands wait for their scrolling request and select
VIEW_TARGET_ENTITY=-1; the scene has no explicit final camera-track command.

For distinct valid mappings and completing actions, Janet reaches `(19,25)`
after her first walk and `(19,26)` after her last; Peter reaches `(21,28)`.
Those and the earlier absolute destinations are conditional instruction-derived
positions before following, not observed or universal final coordinates.
Existing movement, collision, aliases and replaced actions can change completion.
Nod requests sprite/DMA transformations with10/20/10 sleeps; headshake requests
seven paired transformations/restores with six VInt waits per iteration.
Neither gesture validates identity, heals an actor or joins all movement.

## Dialogue, Allocation and Changed Actors

The ten consecutive scene texts are 1126–1135 (`0466..046F`); the paragraphs
above paraphrase their accepted exact literals. `LEADER` in1126 names the
first active member after rebuilding force lists, not necessarily Bowie or
physical0; an empty active list has no guarded fallback here. `NAME;13` in1128
reads Elric's explicit ally-name record without testing whether he is joined.
Renaming affects substitutions. Peter's statement about Elric remains dialogue.

All are `nextSingleText` except1130's `nextText`. The former closes portrait
and dialogue after display and sleeps10; the latter keeps them for1131's
continuation. Newline controls remain line breaks. W1 requests a VInt then
direction/A/B/C input; W2 uses the continuation/validation-sound path.
Janet's modifiers are `$00`; B/P use `$C0` for right-side/mirrored portraits.
Speech/portrait comes from the **actual mapped sprite**, with no replacement
of an already-open portrait: Janet45/46/47 selects portrait18/bleep4;
Peter18/19 selects portrait7/48, bleep4/5; Astral209 selects portrait31/bleep6.
Blue Flame has no row and uses portrait-1/bleep6. An aliased B speaks with its
actual mapped context; its selector is not proof that Astral is present.

Allocation itself is unconditional. `newEntity` calls `InitializeNewEntity`
with `eas_Init`, without checking existing Janet, joined state, HP or capacity.
For ally18, the supplied sprite byte18 is replaced by Janet's class-dependent
mapsprite45/46/47, or Blue Flame190 for HP0 outside battle. It does not promise
Peter's sprite18 or initialize/heal Janet's combatant record. The allocator
scans63 lookup bytes for the maximum positive signed physical index, adds1,
overwrites Janet's lookup and declares/updates that new entity. It does not
find a free physical slot or remove an earlier Janet. Reentry can leave an
old physical actor or follower-list entry behind.

The shared lookup masks the selector with `$FF`, maps negative references
through NPC/enemy offset `$60`, and reads/scales the physical index by32.
Cleared ordinary lookup0 aliases physical0; hidden-1 has no safe-absence check.
Missing or aliased B/P is not rejected. Several names may address one physical
actor, allowing later destinations/actions to replace earlier ones. Those
paths are neither safe no-ops nor guaranteed scene endpoints.

HP guards also differ. `setFacing` reads the actual actor **byte**, skipping
zero-HP named allies below30; B31 and NPC139 bypass that check. Allocation,
action installation, destinations, inline actions, nod and headshake lack that
guard. Dead Janet can therefore be allocated, moved, addressed and joined
without revival. In contrast, each `followEntity` reads a byte **before** its
follower word. Words `$0007`, `$001F`, `$0012` all begin with0, so all three
calls test **Bowie's HP**. Bowie HP0 skips each six-byte operand group; living
Bowie admits them regardless of Peter's or Janet's own HP.

The interpreter clears its skip latch on entry; debug P2 Start can set it.
Skipped ordinary scene text consumes operands without display or cursor
increment, and the50-tick wait skips. Allocation, actions, gestures, join,
following and caller806 are not suppressed. Changed text attribution after
skipped cursor advances must not be described as the normal visible dialogue.
The direct join announcement has its own path below.

## Joining, Announcement and Active Capacity

**Confirmed static:** `join` resets speech0 and waits scrolling before reading
word `$0012`. It clears/tests bit15 and selects normal MUSIC_JOIN19 because
that bit is clear. This is not the
special128 Sarah/Chester form. It calls `JoinForce` for Janet18, reads her
current class, writes the name/class substitution indexes and directly displays
text446 (`01BE`): NAME, CLASS and the joined-force announcement. NAME then
CLASS consume those two indexes after `DisplayText` resets the substitution
pointer. Text446 has a newline but no W1/W2. No stats, class, items, AI or HP
are initialized by this announcement, and it promises no future battle slot.

The actual membership order is joined flag18 → `UpdateForce` → unsigned
active-count comparison with12 → conditional `JoinBattleParty` setting active
flag50. `UpdateForce` scans joined allies0–29 into targets and active/reserve
counted lists, irrespective of HP. The later active-bit write does not rebuild
those lists again; flags can be newer than cached active/reserve prefixes.
The next rebuild is a separate event.

With12 active members and inactive Janet, she still joins in reserve. Already
joined Janet is not rejected and receives the announcement again; if inactive
with room, she becomes active. Already active Janet keeps that bit even at or
above capacity. An unjoined Janet with active50 pre-set enters the rebuilt
active count immediately. No existing active bit is cleared and inconsistent
supplied state is not repaired. Active membership does not govern later following.

After446, `j_FadeOut_WaitForP1Input` requests previous music `$FB` through
`PlayMusicAfterCurrentOne`, submits WAIT_MUSIC_END and polls with Sleep3, then
calls `WaitForPlayerInput` once. It preserves/restores d0.w. A held direction,
A/B/C/Start satisfies input; no fresh edge or release is required. Despite the
helper's name there is no palette fade. Join then closes dialogue, sleeps10
and returns. This is **one** held-input service, unlike the inventor receiving
helper's two. Force membership precedes announcement, music and input completion;
none establishes visible acknowledgement or durable save completion.

## Current Following and the Departure Handoff

**Confirmed static:** with the Bowie HP guard admitted, each call resolves
both names to physical actors at that moment. In order they request
**P→Bowie, B→P, J→B**, using direction2's signed pixel offset `(-24,0)`.
They name dependencies, not the last list entry, adjacency or fixed coordinates.

`AddFollower` installs the current walking-parameter buffer as the action
pointer, copies42 bytes of `eas_Follower1`, advances that buffer and patches
the physical leader and offsets at `$1E/$20/$22`. Only then does it scan the
exploration list to its-1 terminator. An existing physical follower is not
appended/reordered; a new one is appended with a new terminator. Installation
precedes deduplication. Extra followers and older Janet entries remain; there
is no list-capacity repair, self-follow rejection or cycle guard. B aliasing
physical0 can make Bowie follow P after P follows Bowie; B/P aliasing can
replace the first action with self-following. Later control reset can replace
a physical0 action without repairing the list.

The action sets speed32, zero acceleration, disabled obstruction and enabled
map/entity uncollidability, then loops follow/wait-destination. The actual
`esc03_follow` consumer keeps its stored physical leader. At maximum axis
distance≤30 pixels it advances without changing destination; above that it
uses the leader's destination/facing, rotates the offset and tests layout
`<$C000`. It retries without relative Y, then falls back to the leader's
destination; here Y is already0, so the first two candidates coincide. It
writes destination/velocity/travel and waits there before looping. It does
not snap actors into a line or later re-resolve their names. Completion and
final coordinates remain conditional.

At scene end, scrolling is conditionally awaited if dialogue remains open,
VIEW_SCROLLING_SPEED is cleared and registers restored. Caller806 then sets.
The zone wrapper closes portrait/dialogue, waits one VInt, then waits for
**physical0** position to equal destination. It does not await all followers
or toggle VInt_UpdateEntities off/on as the inventor's entity-event wrapper
does. Existing update services must continue to complete actions.

Exploration subsequently calls `WaitForEvent`. Only if there is no pending
request does it restore camera target0 and install the mode-selected controlled
action on physical0. A pending request bypasses that reset; the scene does
not normalize PLAYER_TYPE. Scene return,806, wrapper return and usable control
are separate boundaries, without an atomic save transaction.

Fresh ordinary Map6 entry rebuilds residents/followers and applies the same
setup/init and accepted exploration-healing limits. No resident list or ordinary
follower row declares Janet. Joined18, active50 and806 therefore do not recreate
her temporary town actor or chain. F67/F70 and other follower flags reconstruct
their own order;805/806 do not repair missing67/70. This is a fresh-town fact,
not a claim that Janet disappears from the force or every other map.

| Changed state or interrupted prefix | Local consequence |
| --- | --- |
|805 clear, or805 set/806 set | No scene/new806; no repair of missing Janet membership, entity or following. |
|805 set/806 clear with804 clear, no relevant items/Elric | Scene still admitted; no inventory search or consumption. |
|Janet already allocated/joined/dead | New allocation and join announcement still attempted; old entries, replaced mappings and HP remain separate. |
|Bowie dead; or Bowie live with dead P/J | First state skips all follow calls; second admits them. Facing still uses its distinct actor-byte guard. |
|Interrupt after allocation before join | Current Janet may exist without joined18/806; retry can allocate again. |
|Interrupt after joined18 before active50/announcement return | Membership, caches and acknowledgement can disagree;806 remains clear. |
|Interrupt between follow calls or after them before806 | Following may be absent/partial/installed without suppressing a later eligible replay. |
|After806 before wrapper/control return | Replay suppressed even if windows, movement or usable control remain incomplete. |
|Fresh town with806 set | No recruitment replay and no Janet reconstruction from force flags alone. |

The scene deliberately writes joined18 and conditional active50, followed by
caller806; it has no item/HP/revival, warp, battle, Egress or follower-unlock
flag command. Ordinary service state still changes. The separate south warp
`(any,31)` → Map73 `(29,53)`, Down, no scroll identifies the next route; this
scene does not issue it. Battle24 entry/roster, North Cave blast/Rohde and later
travel/combat belong to separate outcomes.

**Unknown:** natural zone reach; supplied flags/mode/force/HP/class; allocation,
mapping/capacity and follower-list validity/order; completed movement/collision,
DMA/text/portrait/music/input; visible recruitment, usable control and save/load
persistence. These limits do not create a runtime queue or a remake requirement.

## Evidence Review and Retained Results

This synthesis adversarially checks the complete accepted #973 prose and
predecessor boundaries against selected public evidence: `sf2-map-events-static-v1`
([zone shard](../../../tests/fixtures/h2/map-events/zone-programs.json)),
`sf2-map-init-static-v1` ([fixture](../../../tests/fixtures/h2/map-init-static-v1.json)),
`sf2-map-script-engine-static-v1`
([fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json)), and
the common-stats/roster owners. Their schemas and actual consumers retain
zone branch polarity, shared return, setup/init ordering, encoded widths,
join music/name/class/call order and follower byte-before-word guard. Generic
roster observations are handler-local seams, not a Janet scene observation.
Older symbolic-contract Unknowns do not negate #973's selected callee review
or authorize global contract edits. No evidence contradiction was found within
these selected boundaries.

The owning reproduction commands are `uv run sf2 h2 map-events`,
`uv run sf2 h2 map-init`, `uv run sf2 h2 map-script-engine` and the research
owner's retained-pair/consumer recipe. The pinned upstream/pair are absent in
this synthesis checkout; no acquisition, regeneration or fresh comparison is
claimed. Public schema/fixture inspection does not reproduce the retained
45-record/246-byte source/corpus comparison or natural service completion.

[#973 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/973#issuecomment-6180052225)
and [PR975 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/975#issuecomment-6180065642)
retain head3e907b8f/tree5bc75a39/base36b0026b, CI38648632852 (scope19s,
research40s), author0.093s and independent0.096s unchanged-pair receipts,
all45 records/246 bytes/ten scene texts plus446, corrected `(pc)` alias
inspection and HTTPS/private-regex diagnostics, and linked predecessor results.
[#974 updated handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/974#issuecomment-6180123789)
and [PR976 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/976#issuecomment-6180132098)
retain original92af6c79/CI38648796686 (scope15s), final00791e35/treebbf88d2a/
basecdbcccba/CI38649272542 (scope11s),521 links and independent review.
Its base/separate-scope wording update was not a research correction; its
corrected KeyError inspection remains retained. The predecessor's
[retained review](new-granseal-inventor-and-dynamite-gift.md#evidence-review-and-retained-results)
keeps #969/#970/#965 results and failures, Battle19/32-byte-stride, #954
callback and #962 Fairy corrections and linked Unknowns. No completed failure
is reset or described as interrupted.

The Issue handoff records direct complete prose/diff, navigation/links/anchors,
UTF-8/private/exact-ownership checks and `git diff --check`, the same-process
configured clean committed planner and actual exact-head CI. Documentation-only
policy uses direct checks without blanket suites or translation. Evidence
contradictions return to the research owner; independent acceptance remains
with main-gate.
