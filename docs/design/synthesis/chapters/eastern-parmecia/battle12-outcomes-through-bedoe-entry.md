# Battle12 Outcomes through Bedoe's Initial Guard Encounter

Defeating Mount Volcano's leader can bring the battle to victory through the shared enemy-removal
and counting sequence. On the first-completion branch, the enemy recognizes Bowie's jewel, and the
return places the group back on the mountain. Peter has a separate response to the enemy's claim.
The selected mountain exit leads to Bedoe, where the default first-entry scene stages two guards
challenging the visitors, recognizing Peter, discussing the devils and asking him to see the king.

**Confirmed static content and control flow:** the accepted
[research owner](../../../../research/chapters/eastern-parmecia/battle12-outcomes-through-bedoe-entry.md) supports the conditions,
requests and caller order below. **Inferred:** these form a candidate continuation of
[the Battle12 admission guide](polca-fairy-through-battle12-admission.md), rather than a demonstrated
natural playthrough. **Unknown:** walking, supplied state, completed services, visible presentation
and usable input. This explanation adds no remake capability or product decision.

Stop after Bedoe's default/alternate init and the first conditional restoration of control.
The king's address `cs_573EC`, audience, Volcanon, later choices, Skreech, recruitment, shops,
floor routes, Battle13 and unmerged successor findings are outside this guide. Fnnn means flag nnn;
map and battle numbers retain their source meanings. Coordinates below are map tiles unless
explicitly identified as internal units.

## Defeating the Leader and Hearing the Enemy's Recognition

**Confirmed outcome admission:** Battle12 is `BATTLE_MOUNT_VOLCANO`; its leader-present row is
nonzero (`-1`). After an individual action, the battle loop calls the defeated-cutscene wrapper,
processes the death list, and only then counts remaining combatants. The wrapper first requires
Bowie's word HP to be nonzero and enemy128's word HP to be zero. A failed HP condition returns
without either the selected scene or the shared leader tail.

With those HP conditions met, F512 clear runs `edcs_battle12`; F512 set skips the scene but still
reaches the leader tail. The scene's three text requests, IDs 2490–2492 (`09BA..09BC`), all use
speaker 128. They express surprise at defeat, recognition of the jewel, and a claim that the devils
were waiting for Bowie. Camera tracking starts on 128 and changes to Bowie between the first and
second requests; that change does not change the speaker. These are attributed dialogue claims,
not independent evidence of the devils' history or knowledge.

After the text services return, the script sets F714 and ends. It does not set F512 or declare
victory. Target selection does not wait for a target to appear on screen, and the terminator's
conditional scrolling wait is not proof of completed presentation.

The leader-present tail scans enemy slots 128–159 and appends every nonzero-word-HP enemy to the
existing death list, then writes its sentinel. This scan does not require a placed X coordinate
and does not directly zero the enemies' HP. Death processing performs its services and death
crediting, sets listed combatants' X/Y to `-1`, clears status, updates stats and moves their mapped
entities to internal `$7000,$7000`. The subsequent count requires both nonnegative X and nonzero
word HP. Thus queued enemies cease to qualify through removal from the map; already-dead enemies
fail the HP predicate.

The count checks allies first, with Bowie's zero HP forcing their count to zero. Simultaneous
exhaustion loses. With the wrapper's HP admission, completed death processing and counted allies
remaining, the leader's death therefore leads to zero counted enemies and victory. The later
after-turn-effects path repeats death processing/counting without another defeated-wrapper call
at that same point. Neither the dialogue nor its F714 write alone is a victory boundary.

## First Victory, Repeat Completion and Other Returns

**Confirmed return order:** victory heals living/immortal allies and seeds its return from the
current map, the first battle-party member after force update, that member's position plus the
battle-area origin, and the mapped entity's facing. The seed need not be Bowie's position.
The after-battle wrapper then distinguishes completion state before victory writes F512:

| Supplied F512 on victory | After-scene and resulting return |
| --- | --- |
| Clear | `abcs_battle12` requests Map52 `(23,8)`, Left. It replaces the seeded return tuple, displays no text, and returns. |
| Set | Skip the after script and retain the seeded Map52 position/facing. No fixed `(23,8)` repeat return is established. |

The script writes the mutable map-event parameters; it does not immediately load Map52. Both
branches reach the shared after-wrapper tail and `JoinForce(0)`: Bowie is marked joined, force is
updated, and active admission through F32 depends on capacity below twelve. The return member was
selected before this tail. This joins neither Peter nor Luke and changes neither one's force AI.
Victory then clears F412, sets F512, reads the return parameters and returns D4=1.
The F512-set branch skips both selected scenes while retaining their shared leader/join tails;
the defeated wrapper's earlier HP conditions still apply to its leader tail.

The after script does not test or repair F714. A supplied state can reach first victory without
the defeated scene's F714 write. Conversely, completion does not establish that Peter's response
has run. These flags describe separate consumers rather than an enforced history.

**Confirmed other outcomes:** ordinary defeat requests exhaustion text 363, restores Bowie's HP,
halves gold by unsigned right shift and returns D4=-1 with the carried egress tuple. The Battle4
special-loss branch does not apply. EGRESS/Angel Wing consumes its MP/item, runs its text/flash
services and returns D4=0 through the same egress-position owner, without the defeat gold penalty
or new victory completion. In both routes, the unlock update clears F412 only when F512 was already
set; a first-battle loss/escape retains it. Battle12 has no special egress override: the carried
`EGRESS_MAP` selects the savepoint, with `(1,1)`, Up for an unmatched map. No fixed Bedoe destination
or assumed last savepoint follows. F399 set on battle entry also excludes the pre-first-battle
bedroom fallback.

## Back on the Mountain: Peter's Separate Response

**Confirmed map return:** the battle-return main-loop path switches map and enters exploration
without an immediate `CheckBattle`. Exploration clears pending, heals eligible allies, sets map/
outside-battle state, rebuilds entities and sprites, clears temporary flags 256–383, sets F80 and
loads the map before init. The after-scene tuple is used as the return, rather than replayed as
another pending exploration warp. A later ordinary map request can return to battle admission.

Map52's default has four fixed exploration NPCs 128–131; F512 selects an empty fixed population.
Both setups share init. Victory writes F512 before that selection, but an empty fixed list does
not suppress flag-selected followers. Peter7 has no fixed row: F67 supplies his follower mapping
independently of joined/active flags F7/F39, word HP and force AI. Map52 allows followers and is not
overworld, so the F65 caravan prefix does not apply.

Healing also has a different boundary. Peter and Lemon are always processed, including when
previously dead; other dead allies remain dead. Eligible healing restores HP/MP to maxima while
retaining STUN/POISON/CURSE. Processing Peter does not prove F67, a mapping, membership, AI state
or a particular supplied maximum HP.

`ms_map52_InitFunction` tests F714. Clear returns; set falls through to `sub_5C4DC`, which tests
F712. F712 set returns; clear runs `cs_5C622` and writes F712 only after it returns. The primary
map-init fixture body 65 contains only the first two instructions; accepted research owns the
helper composition. F512 population, F714 admission and F712 suppression remain independent.

The response requests Peter at `(22,8)`, Right, a fade-in, and two texts 2493–2494 (`09BD..09BE`).
Peter recalls the enemy's claim and asks why the devils want Bowie. It does not move Bowie, join
Peter, change AI, install following or request a warp. Peter's position command skips only if his
own word HP is zero; HP 256 is nonzero. The fade and texts have no matching presence/HP admission.
Missing Peter mapping can resolve physical main 0, so an admitted position write can affect main
instead. The scene repairs neither F67 nor an absent mapping.

F714-clear/F712-clear does not write F712; F714-set/F712-set skips the response. F512-set/F714-clear
is not repaired, and F512-clear/F714-set/F712-clear can run it with the default NPC population.
The first pending-zero control handoff remains conditional on services returning. It proves
neither a walk from the first-win return point to the exit nor completed follower movement.

## Travelling to Bedoe and Selecting Its Cast

**Confirmed selected connection:** Map52 warp row 0 matches `(28,6)`, without scrolling, and
requests `MAP_BEDOE=10`, `(12,30)`, Up. Row 1 instead matches X0/wildcardY and requests Polca; the
rows do not overlap. The Bedoe row tests none of F512/F714/F712, Peter membership or Bedoe's flags.
Ordinary movement still needs the layout's `$1000` attribute before ordered warp scanning.
The no-scroll consumer idles main, runs sound services, unwinds exploration and returns the tuple
to `MainLoop` for `SwitchMap` and `CheckBattle`. Intervening services/writers can affect completion
or the mutable request. No extracted path establishes natural reachability.

Map10 has no battle row and neither map has a flag-switched replacement. This request establishes
no Battle13 admission or F413/F513 condition. A separately requested ordinary Map52 entry still
has wildcard Battle12 admission through F412; F512 can clear the unlock without declining that
same call. That differs from the immediate battle return's bypass of `CheckBattle`.

Bedoe's setup selector chooses its single F722 override when that flag is set. It replaces entities,
entity events, zones and init while sharing descriptions/section5. F719 clear therefore does not
guarantee the default first guard scene:

| Selected setup | Population and bounded init |
| --- | --- |
| Default, F722 clear | Luke10 at `(38,51)`, Up, plus 29 regular NPCs. F719 clear runs the guard script then sets F719; either F719 branch subsequently handles F720. |
| F722 set | Luke10 at `(38,51)`, Down, plus 26 regular NPCs. F799 set moves Luke out of map; independently F724 set moves lookup 152 out. No default guard script, F719 write or F720 handling. |

Luke's ally row uses lookup 10 and does not increment regular NPC numbering from 128. An existing
Luke mapping, for example through F75, skips the fixed declaration. Physical slots depend on
followers and are not the same as character/NPC lookups. Default guards 150/151 are birdmen at
`(34,51)` / `(35,51)`, Up, `eas_Init`; lookups 152–156 are the five following birdmen. Under F722,
150/151 instead start at `(35,53)` / `(37,53)`, and 152 is a birdwoman at `(6,10)`, Right.
The alternate caller's 152 is not the default guard 152.

Peter again comes from F67, with no fixed row. Luke's declaration is not recruitment, and the
guard script has no Luke command. Population, party, HP and AI remain separate. Default lists
supply the guards, but altered/supplied states with missing mappings are not guarded by the scene.

## The Guards' Challenge, Recognition and Requests

**Confirmed ordered content:** `cs_570B0` requests 25 texts 1616–1640 (`0650..0668`), from guard 150
eight times, guard 151 seven times and Peter ten times. The following paraphrases cover those
requests and their staging; the full 122-record ledger remains with research.

1. **Challenge and recognition (1616–1621).** The scene reloads the current view at `(6,19)`,
   positions Bowie/Peter at `(11,25)` / `(11,26)` and the guards at `(10,21)` / `(13,21)`, and
   initializes the visitors before fading in. Guard 150 approaches and challenges them; guard 151 moves
   inward and states the restriction. Peter approaches and objects. Paired jumps and another
   approach lead to the guards' recognition, apology and welcome to Peter.
2. **Peter asks what happened (1622–1625).** After oscillating and moving across the guards,
   Peter expresses concern, asks about their guarding and asks what happened. Guard 151 reports
   a recent devil invasion. The movements and sleeps stage the exchange rather than establishing
   visible expressions or elapsed scene time.
3. **The guards' account and Peter's connection (1626–1631).** Peter reacts and approaches.
   Guard 150 describes the army, a broken barrier and Volcanon's claimed victory. Peter resets
   and moves back toward Bowie, connecting their own battle to that army. Guard 150 asks about
   other devils. These six requests attribute the explanation to their speakers; they are not
   independent confirmation of the reported events.
4. **Calling the guards back (1632–1633).** The guards face each other, nod and start moving
   away along separate paths. Bowie/Peter face down; Peter jumps, calls them to stop, and says
   those devils were defeated. Bowie nods. The camera then moves to `(6,22)` with an explicit
   scrolling wait.
5. **Replies, caution and an invitation (1634–1638).** Each guard moves inward, faces up and
   responds in turn. They face each other for guard 150's caution and guard 151's nod. Guard 151
   announces a Polca patrol and asks Peter to see King Bedoe. The script then requests another
   staged withdrawal, with speed changes, asymmetric waits and sleeps.
6. **Peter turns to Bowie (1639–1640).** Peter's last two requests invite Bowie onward and
   introduce the king. The scene attempts Peter following Bowie, then explicitly places guard 150
   at `(34,51)`, Up, and 151 at `(35,51)`, Up, before ending.

There is no choice, join, story-flag write or warp inside this program. The patrol statement proves
an expressed intention, not that a patrol reaches Polca. In particular, the final guard placements
return the two lookups to their default coordinates. Departure dialogue and movement requests do
not prove permanent absence. The caller's later F720 handling is a separate effect.

## What the Staging Commands Actually Request

**Confirmed units and services:** one tile is 384 internal 1/16-pixel units, or 24 pixels.
Position commands write current position, destination and facing. Bowie/Peter position/facing
commands probe their own word HP and skip only at zero; guards 150/151 bypass the ally-HP check.
Missing mappings can resolve main 0. No source label alone establishes a valid actor.

The opening current-map reload refreshes map/view services. Its `(6,19)` operands reach internal
2304/7296 before map-area/parallax handling. It does not rebuild entities, rerun init, clear
temporary flags or write a pending warp. It therefore neither resets the selected cast nor
recursively repeats the guard scene. Successful service return does not certify rendering.

Action installation has no general HP or missing-mapping skip. Waited actions poll the selected
entity's `eas_Idle`; a wait for 151 is not a wait for 150. Several paired actions start 150 without
waiting and wait 151, and the Left1/Down5 and Right1/Down5 departure starts both without waiting.
Later installations can replace still-running actions. `eas_Init` restores speed 32, auto-facing,
collision/obstruction, orientation and sprite updates; inline actions set speeds 8/48. Jumps have
their own motion/wait behavior and restore auto-facing. Final position writes do not replace
the action pointer, so they are not a barrier for later movement.

The eight sleeps total 335 requested frames, not scene duration. Text, fading, scrolling and entity
actions add services/waits; debug paths can skip text/sleep work. Camera destinations wait for
scrolling, while target selection does not. Single-text requests include window-close/sleep work.
The terminator's conditional scrolling wait and speed reset do not wait for every entity action.
Existing controlled handler observations certify only their named seams, not this composed scene.

The final `followEntity Peter,Bowie,2` encodes the first operand as word `$0007`. Its handler probes
that word's high byte, so the HP admission is **Bowie0's word HP**, not Peter7's. Bowie at zero
skips the operands. With Bowie alive and valid mappings, selector 2 installs Peter following Bowie
at `(-24,0)`, independently of Peter's HP. Action replacement precedes duplicate-list detection;
missing Peter can resolve main for both operands. There is no following-completion wait, F67
write, join, party admission, player control grant or force-AI change.

This is current entity following. The script does not subsequently rebuild population; later
ordinary entry rebuilds followers from persistent flags. F67 may independently supply Peter then,
but does not prove preservation of this particular relation or action.

## The Init Caller and First Conditional Control

**Confirmed caller order:** the default init runs the guard scene only with F719 clear. After it
returns, the caller writes F719 and independently tests F720. F720 clear returns with the scene's
explicit guard placements; set calls `MoveEntityOutOfMap` for 150–156 in order. That helper writes
internal `$7000,$7000` positions, rather than deleting actors, clearing mappings or recruiting.
Thus F720 set does not suppress an F719-clear scene: its out-of-map movement happens afterward.
F719 set skips the scene while retaining this same F720 handling. F722-selected init instead
uses its independent F799/F724 branches and never writes F719 here.

The selected init returns directly through `RunMapSetupInitFunction`; it does not acquire the
zone-event wrapper's close-window/main-idle tail. Exploration's palette comparison can select
music/fade services next. Only when pending is zero does `WaitForEvent` retarget physical 0 and
call `SetControlledEntityActScript`; ordinary `PLAYER_TYPE=0` selects `eas_ControlledCharacter`.
Another pending request wins that poll. This is the stopping boundary, conditional on service
return and pending state, rather than proof of physical input response, all actions idle or a
usable route to another floor. No king address/audience is consumed here.

## Evidence Review and Remaining Limits

This guide consumes research accepted in [PR #781](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/781)
and the predecessor guide from [PR #782](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/782),
without stacking or successor consumption. Research owns the pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, USA retail identity, exact rows/texts/commands and
[reproduction boundary](../../../../research/chapters/eastern-parmecia/battle12-outcomes-through-bedoe-entry.md#caller-restored-control-and-reproduction).
Bounded review uses accepted prose and selected tracked fixtures; no optional payload generation,
copy, source reinvestigation or runtime acquisition is required.

| Reviewed relation | Existing executable evidence and contract | Review boundary |
| --- | --- | --- |
| Battle outcomes and after return | [sf2-battle-global-data-static-v1](../../../../../tests/fixtures/h2/battle-global-data-static-v1.json), [sf2-battle-routing-data-static-v1](../../../../../tests/fixtures/h2/battle-routing-data-static-v1.json), [sf2-battle-cutscenes-static-v1](../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), [battle lifecycle](../../../contracts/battle/battle-control-lifecycle.md). | Aggregate routing retains `routeTargetsParsed=false`; accepted research supplies exact row 12, HP/F512 wrapper and leader/join/return composition. Generic contracts do not prove this natural victory. |
| Mountain response and Bedoe caller | [sf2-map-init-static-v1](../../../../../tests/fixtures/h2/map-init-static-v1.json) bodies 65/14/15, [map exploration](../../../contracts/world/map-exploration.md). | Body 65 ends after the F714 test/branch; research supplies the helper continuation. Body 14 preserves script→F719→F720, and 15 preserves F799/F724 without the default scene. |
| Exit, setup and actor identity | [sf2-map-data-static-v1](../../../../../tests/fixtures/h2/map-data-static-v1.json), [sf2-map-setup-static-v1](../../../../../tests/fixtures/h2/map-setup-static-v1.json), [sf2-map-entities-static-v1](../../../../../tests/fixtures/h2/map-entities-static-v1.json), [request owner](../../../../research/world/map-event-request-consumption.md). | Summary inventories do not independently parse every selected row/list. Research supplies the selected connection and Luke/regular-lookup join; neither proves walking or current actors. |
| Script staging and following | [sf2-map-script-engine-static-v1](../../../../../tests/fixtures/h2/map-script-engine-static-v1.json), [sf2-map-entity-gesture-relationship-motion-runtime-v1](../../../../../tests/fixtures/h3/map-entity-gesture-relationship-motion-v1.json), [party/roster](../../../contracts/party/party-roster-state.md). | Public command-family/handler facts and controlled HP/follow seams are not a whole-scene transcript. Research retains 122 records, inline actions, real units, high-byte HP admission and current-follow limits. |

The [map-init verifier](../../../../../src/sf2tool/h2/map_init.py) and
[fixture schema](../../../../../schemas/h2-map-init-static-fixture.schema.json) retain provenance and
complete delimited operation bodies. The [map-script verifier](../../../../../src/sf2tool/h2/map_script_engine.py)
and [schema composition](../../../../../src/sf2tool/h2/map_script_schema.py) retain separate command-family
facts and their consumers. Review found no Layer A correction necessary for this explanation.
Existing narrow reproduction routes include `uv run sf2 h2 map-init`, `map-script-engine`,
`map-data`, `map-setup`, `map-entities`, `battle-global-data`, `battle-routing-data` and
`battle-cutscenes`; these are routes, not claims of fresh execution in this prose-only slice.

**Unknown:** natural collision-free mountain/Bedoe travel, supplied flags and maximum/current HP,
actual mappings/followers/party/AI, uninterrupted service and pending completion, visible camera/
fade/text/motion/audio timing, persistence beyond the rebuilt-entry boundary and continuous
playability. Dialogue and connectivity do not close those questions or authorize acquisition.
The guide ends at initial conditional control and completes neither the campaign nor the design epic.

Continue with [Bedoe's king address and first audience](bedoe-king-address-through-first-audience.md)
for the independently admitted report, guard dispersal, Luke/Peter/Bowie introductions and invitation
to Volcanon. That guide distinguishes actual hiding from fresh-entry out-of-map movement and preserves
shared actor positions, current following and conditional control; it establishes no mandatory scene
order, shrine travel or recruitment.
