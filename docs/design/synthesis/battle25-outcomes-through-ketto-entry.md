# Battle25 Outcomes through Ketto Entry

Winning the North Cave battle returns the party to a position derived from
its current members. It does not take either cave exit, unlock Battle26 or
deliver the party to Ketto. Cave departures, later battle admission and the
town entrance are separate transitions. Ketto is Map25; the North Cave
encounter is Battle25. Their shared number does not connect their tables.

**Confirmed static:** the outcome, request, admission and initialization
rules below. **Inferred:** the selected exits and town request describe a
candidate journey onward from North Cave. **Unknown:** natural traversal,
supplied state, completed services, rendered arrival, usable control and
save persistence. This guide ends after Ketto init and the conditional
controller installation request, before later town interactions.

[Ketto conversations, shops and the merchant's move](ketto-interactions-and-merchant-relocation.md)
continues from this entry through independent local interactions, inspections,
collection and conditional departure. It retains separate choice, flag, motion
and service-completion boundaries.

## Evidence and Reading Boundary

Continue from [North Cliff entry through Battle25 admission](north-cliff-entry-through-battle25-admission.md).
The accepted Layer A owner is
[Battle25 outcomes through Ketto entry](../../research/battle25-outcomes-through-ketto-entry.md),
accepted through [#993 / PR996](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/996).
Its provenance pins USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
and [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6).
Fnnn denotes flag nnn; rows and identities are zero-based. The roadmap owns
the [Standard scope agreement](../documentation-roadmap.md#current-baseline-and-near-term-synthesis).
This English synthesis adds no original finding or remake behavior.

The [battle-control lifecycle](../contracts/battle-control-lifecycle.md),
[cutscene routing](../contracts/battle-cutscene-routing.md) and
[map-entry routing](../contracts/map-entry-routing-state.md) contracts own
their shared rules. Their public aggregate boundaries do not independently
prove the selected private rows; #993 supplies the accepted selected review.
Concurrent Ketto-interactions research and UI work supply no evidence here.
No combat simulation, Battle26 roster, merchant conversation, shops, church,
items or subsequent quest is included.

## What Actually Ends the Battle

**Confirmed:** the post-action sequence first calls the defeated hook,
processes the death list and counts remaining combatants. It checks zero
allies before zero enemies. If both sides remain, it runs after-turn effects,
processes deaths again and repeats the same ordered counts. This second
checkpoint does not replay the defeated hook.

Battle25's end and after-battle slots both select `ms_Empty`. The leader
presence byte is 0. With Bowie alive and enemy 128 dead, the end wrapper runs
the empty script only when completion F525 is clear; held 525 skips it. Both
paths reach the leader tail, where byte 0 prevents appending the surviving
enemies to the death list. Dark Soldier128 and the fourteen other initial
enemies 129–142 are distinct. Killing 128 alone does not end the battle.
His ATTACKER1 AI is not a leader-presence rule. The leader-death position
table also has no Battle25 row, so this seam supplies no fixed exit position.

The empty script is still an interpreter operation: termination may wait for
open-dialogue scrolling, then clears scrolling speed. Empty hooks neither
cancel their wrappers nor prove service completion.

Counting scans all 30 ally and 32 enemy slots. A slot counts when its X **byte**
is nonnegative and its HP **word** is nonzero. Membership, active-party flags,
AI role, Y and being inside the crop are not these tests. Bowie HP0 forces
the ally count to zero. Thus simultaneous exhaustion is defeat; with Bowie
alive, no counted ally still means defeat. Victory needs a positive ally
count and zero counted enemies. A hidden X=-1 enemy does not count even
with HP, while a supplied placed nonmember or otherwise unused slot can count.
Fresh Battle25's absent 143–159 slots do not constrain resumed or changed state.

Death processing consumes the length-delimited list, rather than scanning
for HP0 anew. Spin/sound passes precede defeat/kill bookkeeping, X/Y=-1,
status 0, stat recomputation and mapped entity movement to `$7000/$7000`.
This function does not itself zero listed HP or clear list length. Stale or
duplicate entries, inconsistent mappings and interrupted presentation remain
actual premises; the empty hook provides no repair.

## Victory Returns to Current Party State

**Confirmed:** victory has four distinct stages, in this order:

1. Heal all living allies and Peter 7/Lemon 28 even if dead. Eligible allies
   receive maximum HP/MP, preserve only STUN/POISON/CURSE from existing status,
   and recompute stats. Other dead allies stay dead. This scans all 30 slots,
   rather than testing joined or active membership.
2. Rebuild joined/active lists in ascending ally ID and select the first
   active-list byte. There is no living-member test or empty-list guard.
   The seed map is CURRENT_MAP; combatant X/Y plus BATTLE_AREA_X/Y are added
   as bytes for coordinates. The actual combatant-to-entity mapping supplies
   physical facing. PARAM1 becomes 0 without writing MAP_EVENT_TYPE.
3. Run the after wrapper: F525 clear dispatches the empty script, set skips
   it; both call `JoinForce(0)` at the shared end. This joins Bowie with F0
   and activates F32 if fewer than 12 are active, without evicting a full party.
   It happens after seed selection and does not repair that seed.
4. Clear unlock 425, set completion 525, leave intro 475 unchanged and return
   the PARAM2–5 bytes in zeroed register words, with result d4=1. There is
   no unlock 426 write.

For ordinary Map48's Battle25 crop `(28,0)`, live local `(x,y)` gives
`((x+28) mod 256, y mod 256)` with current mapped facing. It is neither the
initial formation nor a guaranteed Bowie coordinate. A dead/off-map first
member, different crop/map/mapping or later parameter overwrite changes it.
An empty list exposes its stale first byte. Later joining Bowie is separate
from both Rohde recruitment and the earlier coordinate selection.

An interrupted script, join or flag tail preserves its reached writes. There
is no rollback or guarantee that the final return happened.

## Defeat and Escape Have Different Costs

**Confirmed:** defeat updates the unlock flag, clears the dialogue name,
requests sad music/text 363 and closes text, restores Bowie's maximum HP,
halves unsigned gold, obtains Egress coordinates and returns d4=-1.
Battle04's exception does not apply here.

Admitted Egress deducts its selected spell cost from MP; Angel Wing removes
its selected inventory slot. Their shared text/sound/flash tail updates the
unlock, unwinds the action frame, obtains Egress coordinates and returns
d4=0. They do not apply defeat's gold penalty or Bowie restoration. Action
admission and completed services remain premises, rather than proof supplied
by the outcome code.

For either defeat or escape, the unlock helper preserves 425 while 525 is
clear and clears 425 while 525 is set. Admission may already have cleared 425.
These paths do not newly set 525, run the victory join or change 475. They
return registers rather than manufacturing a new event-kind/parameter tuple.

Subsequent exploration heals eligible living/immortal allies again. A spell
cost therefore does not promise the eventual MP value; an item already
consumed and gold already halved are different mutations. This is not generic
revival: dead non-immortals stay dead, except Bowie's explicit defeat restore.

`EGRESS_MAP` is a map selector into savepoint data, not a stored XYZ/facing
tuple or an automatic Map48/72 return. BattleLoop sets 399 on fresh/resumed
entry. With F399 set, supplied selector 6 gives New Granseal `(11,22)` Up;
selector 25 gives Ketto `(12,11)` Up; unmatched 48/72 keeps its map and uses
`(1,1)` Up. Other selectors use their own row or fallback. These examples
establish neither a prior Ketto visit nor a naturally selected church savepoint.
A direct F399-clear call instead returns initial Map3 `(56,3)` Down before
raft reset.

With F399/F64 set, raft reset uses Map72 `(43,48)` for Polca/Bedoe, Map69
`(28,55)` for Hassan, and otherwise Map69 `(42,15)`, including selectors
6/25/48/72. Clear 64 preserves raft fields. Resetting those fields is not boarding.

## Exploration Starts Before Another Battle Scan

**Confirmed:** victory, defeat and escape all reach the alternate MainLoop
entry: `SwitchMap` then `ExplorationLoop`, without `CheckBattle` or a sign
test on d4. The switch table has no replacement for 48/72/25. Explicit victory
return 48 therefore does not immediately re-admit Battle25 even if 425 is supplied.

Exploration clears the pending kind, subtracts 20000 from STEP_COUNTER with
a signed negative clamp, heals eligible allies and fades out. Explicit 48
sets map 48/battle255, loads tiles, clears VInts, obtains and rebuilds
entities/sprites, clears temporary F256–383 and sets 80 before LoadMap,
base VInts and init. Persistent story, battle, party and follower flags survive.
BattleLoop cleared PLAYER_TYPE and the empty hooks do not replace it, so
ordinary return uses mode 0.

Map48 has no setup entry: its sentinel selects `ms_Void`. Shared initialization
still clears entities, old mappings and walking scratch, builds flag followers
and declares main physical 0 from the supplied coordinate words/facing. It
skips fixed NPCs and the sentinel init call. The former battle population and
scene follow chains are not retained. An absent setup is not an empty lifecycle.

The ordinary follower declarations are flag-driven. Rohde joined 11, active 43
and follower 84 are independent. HP/class affects an ally's sprite, including
the outside-battle dead blue flame, rather than follower admission. Multiple
alias flags can allocate distinct actors while a later declaration replaces
their lookup. Logical A/B/C/D mean 30/31/158/159, with lookup offsets 30/31/62/63;
physical slots depend on preceding declarations. Neither 48 nor 25 uses the
overworld Caravan prefix. Finite storage and follower action-pointer capacity
remain premises, not guarantees for arbitrary mass enablement.

CURRENT255 partial reload still heals, updates main/raft, loads and runs init,
but retains map/battle, population and temporary flags. Border scroll stays
inside exploration and skips ordinary population/temp reset/init. A script's
direct LoadMap has another lifetime. F88 battle continuation restores supplied
state instead of a fresh formation; its eventual outcome uses that state.

## Separate Cave Departures and Map72 Admission

**Confirmed:** these North Cave warp rows store fixed no-scroll requests:

| Map48 source tile | Destination | Facing |
| --- | --- | --- |
|`(55,30)`|Map72 `(27,34)`|Down|
|`(52,13)`|Map72 `(25,29)`|Down|

Other cave-side rows are `(15,27)` →72 `(58,50)` and `(3,12)` →72 `(56,46)`,
also Down. Their presence proves neither connectivity between cave regions
nor a walk from the victory seed. The selected rows add no 525/425/475/750,
Rohde membership, HP or party-size test; victory invokes neither row.

Ordinary walking must first reach the warp-kind layout attribute. The scanner
divides internal coordinates by 384 and matches source low bytes. It writes
the mutable pending request and sound; consumption clears the kind before
dispatch, submits/clears sound, idles physical 0, unwinds exploration, samples
coordinates and replaces nonnegative destination low bytes plus facing.
Nominal destination words depend on the sampled high bytes. A replaced pending
request or interrupted service can change or prevent the transition.

A later ordinary request reaches `SwitchMap` then `CheckBattle`. On Map48,
only Battle11 at `(15,26)` with 411 or Battle25 at `(55,29)` with 425 qualifies;
`(52,12)` qualifies neither. Held 511/525 clears the selected unlock after
selection but still admits that battle on this call. Victory in Battle25 does not clear 411.

On Map72 the complete candidates are 8,24,26, in that order, with unlocks
408/424/426 and wildcard triggers. The first unlocked row wins even at either
cave-return coordinate. Areas `(1,32,32,30)`, `(1,32,32,30)` and `(28,0,36,32)`
are output crops, not containment tests. Held 508/524/526 clears the matching
unlock after selecting that same battle, without continuing to a later row.
Victory in Battle25 alone promises neither 426 nor an unopposed journey.

Without admission, Map72 has its default empty fixed population and shared
followers; F65 can prepend Caravan on this overworld. F750 independently
opens the entrance on load. Init tests 716 then joined-Luke10: set 716/clear 10
runs the accepted earlier recruitment script, otherwise returns. It neither
tests 525 nor directly requests 26. Supplied history can therefore re-enter
that earlier scene.

Walking zone production requires its own layout kind and writes event 6.
The first matching Map72 zone row wins: X32–34/any Y calls helper 26;
X25–30/Y37 calls the separate cave-opening branch; default selects helper 8
when 523 is clear, helper 24 when set. A declined specific callback does not
fall through. Raft/Caravan control has different producers; a supplied event 6
can still call the zone consumer.

Helper 26 checks **526, not 525**. Clear 526 requests 26 immediately without
counter or RNG checks. Set 526/nonzero STEP_COUNTER declines. Set 526/counter 0
draws range 8: zero requests; nonzero draws range 4+2, sets counter 2–5 and
declines. Requesting sets 426 and type1/map CURRENT255, counter 30000, then
waits for scrolling and requests sound/flash, leaving coordinate/facing
parameters supplied. Writes precede service completion; the next scan can
still select 8/24 first. This explains admission conditions, not Battle26 combat.

## A Separate Entrance Builds Ketto

**Confirmed:** Map72's `(31,26)` row requests Map25 `(16,30)` Up, no scroll,
through the same mutable producer/consumer chain. It has no 525/526/800 gate.
This row does not prove a walk around intervening zone/battle triggers. An
intact explicit 25 request passes SwitchMap unchanged; the entire battle
coordinate table has no Map25 row, so CheckBattle returns no candidate
regardless of held unlocks.

Ketto has one default setup, `ms_map25`, with no population flag variant.
F800 changes init, not setup selection. Its flag-copy table is only a terminator.
Explicit entry reconstructs main/followers and all eight NPCs, resets temporary
state and runs LoadMap/base VInts/init before conditional music/fade and
WaitForEvent. Selecting pointers for conversations, zones or items does not
execute those interactions.

Let **n** be the actual number of preceding admitted ordinary followers.
Main is physical 0; NPC logical 128+j occupies physical n+1+j and lookup 32+j.
Active battle members are not all inserted as town actors.

| Logical ID | Physical | Initial X/Y, facing | Actor sprite | Action or walking origin/range |
| --- | --- | --- | --- | --- |
|128|n+1|`(17,26)`, Down|Young man (196)|origin 17,26; range 1|
|129|n+2|`(24,25)`, Down|Old man (198)|eas_Init|
|130|n+3|`(20,13)`, Down|Man (194)|origin 20,13; range 1|
|131|n+4|`(8,21)`, Down|Young man (196)|origin 8,21; range 1|
|132|n+5|`(4,12)`, Down|Man (194)|origin 4,12; range 1|
|133|n+6|`(11,3)`, Right|Merchant (193)|eas_Init|
|134|n+7|`(12,10)`, Down|Priest (192)|eas_Init|
|135|n+8|`(27,17)`, Right|Woman (195)|eas_Init|

For only follower flags 67/70/84, Peter/Astral/Rohde are physical 1/2/3 and NPCs 4–11;
merchant 133 maps 9 and woman 135 maps 11. Without followers they map 6/8.
Duplicate alias declarations still allocate actors. Initialization has no
capacity check, follower action pointers have a finite declared sequence,
and walking/entity storage is finite. Successful declarations are a premise.
A separate raft can use physical 31 under actual map/flag/mode conditions;
it is not logical FOLLOWER_B31.

The four walkers install copied walking scripts, consuming 50 bytes each.
Their nominal bounds are respectively X16–18/Y25–27, X19–21/Y12–14,
X7–9/Y20–22 and X3–5/Y11–13. These are random-step bounds around origins,
not measured paths. The consumer tries up to four random cardinal directions,
checks the relevant current-coordinate bound, layout and other destinations
before motion; slope handling can change Y. It is not a universal rectangular
clamp for altered state. The script waits 30, configures motion/collision and
loops random walk → destination wait → wait 20.

Fixed `eas_Init` scripts configure speed 32, zero acceleration, collision,
obstruction, auto-facing, Up orientation and size 24, then sprite update/Idle.
Orientation differs from declared facing. Installed scripts and VInts mean
these are initial declarations, not proof that every actor still occupies
that position when control is requested.

## F800 Relocates Two Actors During Init

**Confirmed:** clear 800 returns from init without relocation. Set 800 executes
the entire three-record `cs_5D3AA`: `setPos` for 133 to `(25,18)` Right, then
for 135 to `(25,17)` Right, then end. It sets no flag, recruits nobody and invokes no
merchant conversation. F800 is supplied here; its producing event is separate.

Actual setPos resolves NPC lookups 37/39, bypassing the ally HP guard. It writes
position and destination as tile coordinate×384, facing Right and requests
sprite update. It does not walk from the origin, replace the action script
or clear velocity/travel. Fresh declarations provide the intended merchant
and woman. A missing zero lookup aliases main physical 0; hidden$FF is not a
safe no-op. Neither state is automatically repaired.

There is no text or sleep command, and text-skip does not skip these moves.
Termination can still wait for open-text scrolling and clears scroll speed.
An interruption may leave only the first relocation; no later move or init
return follows merely from that prefix.

Fresh explicit 25 rebuilds the eight origins before applying 800. CURRENT on
current 25 retains actors/temp state and still runs init: set 800 reapplies
positions to current mappings; clearing 800 does not restore earlier positions.
Scroll skips init, and CURRENT on another map is not Ketto. Mode 2's population
tail changes main to raft and hides/stands followers; other supplied modes
retain their own effects.

After init, palette state conditionally requests music/fade. With pending
zero, WaitForEvent selects camera physical 0 and requests control for entity 0:
mode 0 installs ordinary character control, mode 1 raft, other nonzero mode
Caravan. Pending nonzero skips installation and returns the event instead.
This is the endpoint, not confirmed playable town arrival. Input, action,
sound, palette, DMA/VInt completion and save/load remain **Unknown**.

## Evidence Review and Retained Results

The complete accepted #993 research was reviewed against the relevant
contracts, public fixtures/schemas and verifier consumers. The stricter
selected-source boundary governs quantities, identities, byte/word arithmetic,
ordering and changed-state consequences; schema success is not private-row proof.

| Reviewed boundary | Public owner and limit |
| --- | --- |
|Death/count/outcome/healing|`sf2-battle-control-static-v1` and `sf2-battle-loop-static-v1`; shared checks and mutations, not simulated Battle25 combat.|
|Empty hooks and join|`sf2-battle-cutscenes-static-v1` and `sf2-battle-routing-data-static-v1`; wrapper facts and table structure, with selected slot 25 resolved by #993.|
|Map admission/savepoint/request|`sf2-common-maps-static-v1`, map-data and request-consumption fixtures; selector and low-byte consumer rules, not a natural journey.|
|Setup/init/zones|Map-setup/map-init and `map-events/routing-setup.json` / `map-events/zone-programs.json`; shared structure/operations, not all private population rows.|
|Relocation|`sf2-map-script-engine-static-v1`; setPos lookup/HP/position/termination consumers, with exact cs_5D3AA and eight actor rows retained by research.|

The [battle-loop verifier](../../../src/sf2tool/h2/battle_loop.py),
[cutscene verifier](../../../src/sf2tool/h2/battle_cutscenes.py),
[map selector verifier](../../../src/sf2tool/h2/maps.py),
[request consumer](../../../src/sf2tool/h2/map_event_request_consumption.py)
and [map-script verifier](../../../src/sf2tool/h2/map_script_engine.py)
preserve their bounded checks. Selected public schemas and facts were checked
directly. The aggregate count fact named `requiresPositiveHp` summarizes
the selected source's nonzero HP-word test; it does not add a signed-positive
requirement for changed word values. No in-scope contradiction was found;
source contradictions return to research. Older aggregate Unknowns do not reopen this accepted static seam.

Owning commands remain `uv run sf2 h2` with `battle-control`, `battle-loop`,
`battle-cutscenes`, `battle-routing-data`, `common-maps`, `map-data`,
`map-event-request-consumption`, `map-setup`, `map-init`, `map-events` and
`map-script-engine`. Research owns the retained-pair reproduction recipe.
Source/pair absence in this synthesis environment is accepted: public checks
and prose review are **not a fresh comparison**. No acquisition, regeneration,
runtime, installation or new maintained machinery was performed.

[#993 final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/993#issuecomment-6181265165)
and [PR996 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/996#issuecomment-6181273945)
retain final `5a8d9fc8`/tree `8dfc21cd`/base `88cf5a36`, unchanged note blob
`73d19594`, CI38658081977 (scope21s/research29s), original `5596723a`/tree
`016175d3`/CI38657738283 and the 0.088s unchanged-pair comparison. They retain
the separator-check failure/correction, follower C/D identity correction and
discovery diagnostics. The corrected logical 158/159 identities are used here.

[#994 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/994#issuecomment-6181206310)
and [PR995 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/995#issuecomment-6181222523)
retain `c52888c0`/tree `7b21c454`, CI38657615281 (scope15s), eleven schema
checks/529 links, unavailable source/pair and bounded diagnostics. Its
[retained review](north-cliff-entry-through-battle25-admission.md#evidence-review-and-retained-results)
keeps linked predecessor failures and Unknowns. Completed results are not reset.

The #998 Issue/PR handoff records full prose/diff/navigation/link/anchor,
UTF-8/private/exact-five-path checks, `git diff --check`, configured clean
committed planning and actual exact-head CI. Documentation scope launches no
blanket normal/full/Python/.NET/Godot/H3/H4/translation suite. Independent
acceptance and integration remain with main-gate.
