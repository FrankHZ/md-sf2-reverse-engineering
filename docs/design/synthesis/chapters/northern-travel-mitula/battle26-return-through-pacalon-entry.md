# Battle26 return through conditional Pacalon entry

Higins asks for help in Pacalon, but his recruitment does not carry the party
there. **Confirmed:** victory first returns the position prepared before the
rescue scene. Fresh exploration then rebuilds the actors, removing Higins's
temporary following while preserving his force membership. A selected eastern
border can scroll into the neighboring overworld; entering Pacalon is another
request, and that request can admit Battle31 before town exploration begins.
These are connected state transitions, not proof of an uninterrupted walk.

This guide continues [Battle26 outcomes and Higins recruitment](battle26-outcomes-and-higins-recruitment.md)
using complete accepted [return/entry research](../../../../research/chapters/northern-travel-mitula/battle26-return-through-pacalon-entry.md)
and its predecessor owners. It is Layer B English synthesis under
[#1022](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1022)
and [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Standard rigor fits the separate return, population, scrolling and admission
lifetimes. Pacalon interactions remain with their separate accepted owner.
The endpoint is the first conditional Map02 event/control read, or the Battle31
diversion before execution. Town interactions and castle admission are excluded.
Continue with [Pacalon conversations, supplies and the castle handoff](pacalon-interactions-and-castle-handoff.md)
for both resident-event alternatives, services, descriptions, possible pickups
and outgoing warp consumption, stopping before destination admission/init.

**Unknown:** natural reach, actual supplied state, decoded walkable route,
service completion, rendering, usable control and save/load persistence.
The guide changes no remake acceptance or original-observation frontier.
Fnnn denotes game flag nnn; map and battle numbers are separate domains.
Coordinates below are tile requests unless an internal word/byte operation is
specified. Source paths and numeric identities remain with the research owner.

## The return position belongs to the outcome

**Confirmed:** ordinary victory prepares PARAM2 with the current map, nominally
72; PARAM3/4 with the first active member's combatant coordinates plus the
battle crop; and PARAM5 with that member's mapped entity facing. This happens
before the rescue. Bowie's staged `(19,16)`, the camera's `(13,11)` and Higins's
scene moves do not replace that seed. Reached completion clears F426 and sets F526;
476 remains the separate battle-introduction flag. Held F526 skips rescue, while
the shared JoinForce0 tail still runs. It is neither another Higins join nor a
repair of the return position.

Defeat, admitted Egress and Angel Wing instead use their supplied
savepoint-derived return, with their own gold, HP, MP, item and raft writes.
They need not return to 72. Resumed F88 entry likewise retains its supplied
state. The [outcome owner](battle26-outcomes-and-higins-recruitment.md#completion-retreat-and-the-returned-handoff)
owns those alternatives; victory's scene does not normalize them.

All returns reach `alt_MainLoopEntry`, which calls SwitchMap and then
ExplorationLoop, without testing the returned d4 sign or calling CheckBattle
between them. A surviving Battle26 unlock therefore does not immediately
re-admit that battle at this return boundary. This differs from the ordinary
MainLoop entry used by a later no-scroll warp.

SwitchMap scans word map identities in six-byte records and returns the first
matching replacement whose flag is set. The complete table is Map3/F640→4,
Map71/F530→78 and Map58/F999→59. There is no Map72 or Map02 replacement,
and no reverse 78→71 rule. It does not first resolve MAP_CURRENT255.

## Fresh exploration rebuilds who is present

**Confirmed:** for the explicit returned map, ExplorationLoop runs this order:

1. Clear pending event type; subtract 20000 from the word step counter, clearing
   it when the signed branch fails. Nominal 30000 becomes 10000; small ordinary
   nonnegative values become 0. This is word/CCR arithmetic, not an unbounded
   integer clamp. Clearing type does not clear every remaining parameter byte.
2. Heal living/immortal allies, request all-palette fade-out, set view target
   byte-1 and save the requested map word.
3. For a low map byte other than 255, set CURRENT_MAP and CURRENT_BATTLE255,
   load tilesets, wait for fade and clear VInts. Obtain the selected setup list,
   initialize entities/mapsprites, clear temporary F256–383 and set F80.
4. Initialize exploration sprite counters, call LoadMap with the saved map word
   and d0=-1, install base VInts and run the selected setup init. Palette state
   then conditionally requests map music and fade-in.
5. Set movement sound and enter WaitForEvent. Only a zero pending event permits
   control-script installation on physical 0; pending nonzero bypasses it.

The healing call scans all 30 ally slots, including unjoined allies. Peter7 and
Lemon28 are always eligible; others require nonzero word HP. Eligible slots
receive maximum HP/MP, retain only already-present STUN/POISON/CURSE bits and
recompute stats. Living Higins19 is healed regardless of F19; dead19 stays dead.
Thus an earlier escape MP cost or defeat's restored Bowie HP need not describe
the later exploration values. Membership itself grants no immortality.

InitializeMapEntities scales the supplied main coordinate **words** by 384,
clears actor records, mappings and walking allocation, builds flag followers,
consumes the fixed setup list, and declares Bowie as physical 0 with the supplied
facing. It terminates the newly built follower list. Scene scripts and the old
physical follower chain are discarded. F19/F51 and the relevant story/battle
flags survive the temporary-flag clear.

This is why Higins can remain recruited without walking behind Bowie. Neither
the ordinary follower table nor Map72's empty fixed list has a Higins row.
Join19's persistent membership and the rescue's temporary AddFollower are
different authorities. A missing logical mapping can resolve to physical 0;
that is not a separate invisible Higins actor.

Map72 is an overworld, is not follower-excluded, and has one default setup.
F65 adds the Caravan prefix before ordinary followers; its fixed list is only
`msEntitiesEnd`. Its init at `$4FF2E` runs the already-owned
[Luke recruitment](../../../../research/chapters/eastern-parmecia/polca-departure-through-luke-recruitment.md#fresh-population-raft-declaration-and-init-guard)
only with F716 set and joined F10 clear. It tests neither F526 nor a Battle26
completion premise. Altered history can therefore change the state/coordinates
before control is considered; this guide does not repeat that scene.

BattleLoop clears PLAYER_TYPE and the ordinary Battle26 outcome leaves it
unchanged, giving this selected return numeric mode 0. Supplied modes remain
separate. A CURRENT255 exploration entry still clears pending, adjusts the
counter, heals/fades, updates main/raft, loads and runs init, but does not
rebuild actors, replace CURRENT_BATTLE or clear temporary flags. Negative
coordinate low bytes skip its main-coordinate update. Direct script LoadMap
and scrolling each have different lifetimes again.

## The eastern border requests a scroll

**Confirmed:** Map72 warp row 8 at `$C1C3E` matches `(63,255)`, where the negative
source byte means any Y. It records scroll RIGHT, destination 71 `(1,255)` and
facing RIGHT0. The eight-byte row encodes transition `$10`, map 71, X1/Y$FF,
facing 0 and padding 0; the macro uses `$10+direction` for scroll and 0 for
no-scroll. No426/476/526, Higins membership, HP or party-size test guards it.
Earlier matching rows still win. This does not direct the party east or prove
arrival at the edge.

Ordinary walking first reads the live layout and requires masked kind `$1000`
before calling WarpIfSetAtPoint. That producer divides internal coordinates
by 384, scans in order, checks the word `$FFFF` terminator first and treats
negative source bytes as wildcard axes. It writes type 1, PARAM1–4 as a
longword, PARAM5/padding as a word and WARP_SFX. Wildcards do not perform
collision or bounds checks. Movement has its own obstruction/mode/flag gates;
kind `$1400` instead requests a zone event. A posted request and an accomplished
walk remain distinct.

ProcessMapEvent clears type before dispatching the saved event value. Its
nonzero-PARAM1 warp branch idles main and reads the destination byte. For 71 it
explicitly checks F530 and substitutes 78 when set, writing CURRENT_MAP before
transition services. This is a separate code branch from SwitchMap's generic
table; changing F530 later does not retroactively select another loaded map.

ProcessMapTransition loads the chosen map's tiles/blocks/layout and dispatches
on PARAM1 &3. RIGHT adjusts plane counters and sprite projection, queues DMA
and waits for VInts. It does not return through MainLoop or construct a new
population. Destination bytes then have deliberately different width rules:

| Operand | Actual scrolling write and limit |
| --- | --- |
| Y$FF | Y is read after clearing d0.w. Its negative byte preserves live Y/YDEST. |
| X1 | A nonnegative byte replaces only d0's low byte after transition; `mulu.w` consumes `((d0.w & $FF00) \| PARAM3)`. The stored coordinate is the product's low word after multiplying by384. The record's X1 is not an unconditional zero-extended world-coordinate word. |
| PARAM5 facing0 | The scrolling branch never copies it into entity facing. Normal facing comes from the moving main actor; an injected conflicting facing is not applied. |

It samples actual main X/Y by signed division by 384 and current facing, then
calls DeclareRaftEntity. That call retains scripts/mappings but can move actors:

| Numeric mode / raft state | Reached behavior |
| --- | --- |
|2, PLAYERTYPE_RAFT|Return immediately; no follower relocation or raft repair.|
|1, PLAYERTYPE_CARAVAN|Skip follower relocation, then use the raft gate.|
|Other modes|Walk the existing EXPLORATION_ENTITIES list, placing each listed physical actor's position/destination at the sampled main tile, then use the raft gate.|
|64 clear|Return without raft repair.|
|64 set, RAFT_MAP matches|Declare physical31 from masked raft X/Y bytes, Left, raft sprite/standing script, lookup offset63.|
|64 set, RAFT_MAP differs|Clear that lookup and move the raft off-map.|

Physical raft 31 is distinct from logical FOLLOWER_B31/lookup31. Duplicate or
altered physical IDs are consumed as supplied. No flag-based follower list is
rebuilt; newly setting a follower flag does not allocate its actor here.
Walking scratch, installed scripts and mapping identities stay live while
ordinary followers' positions/destinations are refreshed.

There is no healing, counter subtraction, temporary clear, CURRENT_BATTLE
reset, setup/init or battle scan on this scrolling path. It also omits the
no-scroll branch's WARP_SFX submit/clear. On return the existing exploration
loop sets movement sound and re-enters WaitForEvent; zero pending reinstalls
the mode-selected control script, while a new event bypasses installation.
Interrupted DMA, fade, action or VInt services do not roll back reached writes.

## A loaded overworld is not a decoded route

**Confirmed:** neither Map71 nor Map78 has a map-setup entry. GetCurrentMapSetup reaches
the terminator and returns `ms_Void/$FFFF`; the entity-list consumer returns that
sentinel and init dispatch skips its call. This is no populated default setup
or default random encounter. A hypothetical fresh explicit 71/78 entry would
still build shared followers/main/raft but have no fixed NPC list. The selected
scroll calls none of those setup consumers and retains Map72's live cast.

The two maps select distinct Map71s0_Blocks/Map71s1_Layout and
Map78s0_Blocks/Map78s1_Layout. Both headers use palette 0/tilesets 101–105;
their areas specify `(0,0)..(63,63)`, other origins `(0,0)`, both parallax
pairs 256/256, zero autoscroll, type 254 and MUSIC_MAIN_THEME. Their flag and
step tables terminate immediately. Different layouts remain different collision
inputs even when these area properties and town warps agree.

The transition reads/scales the first area rectangle, but does not call
LoadMapArea to refresh area/music/animation metadata. Map72's selected area
has the same stated rectangle/layer properties; altered retained state is
still a premise. New layout does not imply fresh area/camera initialization.
LoadMapBlocksAndLayout also honors live CURRENT_BATTLE: ordinary 255 avoids
the battle-crop copy, while supplied non 255 can apply its crop during a scroll.

Neither71 nor 78 appears anywhere in BattleMapCoordinates. Missing setup makes
zone dispatch skip callback selection before its common VInt/main-stop wait.
It does not invent a random-battle default, nor exclude a supplied event or
later transfer elsewhere. Map72 before departure retains its own accepted
zone/counter rules: the Battle26 helper requests immediately with clear F526,
or uses the counter/range 8 retry with held F526; ordinary scans order 8/24/26
under F408/424/426. The border scroll performs none of those scans.

Actual walking continues to consume layout bits, actor destinations, mode
and pending state. No decoded path from the battle seed to the edge, or from
that edge to Pacalon, is established. The optional source-side compressed-layout
read was unavailable in accepted research; selected resource/consumer evidence
does not close cell-level walkability, encounter cadence or natural reach.

## Pacalon's doors precede a separate battle decision

**Confirmed:** both overworlds have these identical first two no-scroll rows:

| Source map / row addresses | Source tiles | Requested entry |
| --- | --- | --- |
|71 / `$C0694`, `$C069C`|`(21,15)`, `(22,15)`|Map02 `(21,30)`, Up1|
|78 / `$C69E4`, `$C69EC`|`(21,15)`, `(22,15)`|Map02 `(21,30)`, Up1|

They have no F530/526 or membership gate, and use the same layout-kind producer
and mutable single event slot. The no-scroll consumer submits/clears WARP_SFX,
idles main and unwinds exploration. It clears d0–d4 **words**, reads map 2,
samples main coordinates and replaces nonnegative destination **low bytes**
with 21/30, reads facing 1 and returns d4=0. Those replacements do not erase
sampled high coordinate bytes; `(21,30)` is the nominal ordinary result.
Changed sentinel, high-byte, service or event state uses the actual operations.

This return enters ordinary MainLoop `@Start`: SwitchMap, then CheckBattle.
Map2 is not switched. The ordered 45-row scan has one Map02 candidate, row 31:
crop `(0,0,48,32)`, trigger `(255,255)` and unlock 431. Other-map rows are skipped;
the crop is output data, not a containment test. Its wildcard trigger does not
require the nominal 21/30 position.

| State at this scan | Selected continuation |
| --- | --- |
|431 clear, either531|No candidate, d7=-1; enter Map02 exploration.|
|431 set,531 clear|Select31, write the crop and call BattleLoop.|
|431 set,531 set|Select31, write the crop, clear431 and **still admit31 on this call**.|

F530 chooses content, not this battle gate. F526 and Higins F19/F51 are not
tested; Battle26 victory does not newly set F431 or 530. A supplied CURRENT255
would be resolved for CheckBattle, but the intact town row requests explicit 2.
The Battle31 branch stops before initialization/execution. Even held F531 does
not warrant an unconditional town-exploration promise.

## Town population is conditional content

**Confirmed:** only the no-battle alternative starts fresh explicit Map02
exploration. It repeats the healing/reset/rebuild sequence, sets current map 2
and current battle 255, then selects `ms_map2` at `$5E86C` with clear F530 or
`ms_map2_flag530` at `$5E884` with set F530. Both six-pointer tables select
entities, entity events, zones, descriptions, item events and init. Population,
entity-event and description pointers differ; zone/item/init pointers are shared.
Selecting an interaction pointer is not executing it.

Map02 is not overworld or follower-excluded, so 65 adds no Caravan prefix.
Ordinary follower rows remain independent flags in this complete order:

`66/Sarah, 67/Peter, 72/Kazin, 73/Slade, 74/B/Astral, 66/Chester,
68/B/Oddler, 75/Luke, 69/C/Fairy, 70/B/Astral, 74/A/King, 71/Lemon,
74/D/Minister, 76/Zynk, 77/A/Old Man, 82/Frayja, 83/A/King,
83/D/Minister, 84/Rohde`.

A/B/C/D are logical 30/31/158/159 and lookup offsets 30/31/62/63. Each enabled
row allocates the next physical actor from 1, writes its lookup, installs its
next follower script and appends its physical ID. Membership/HP does not gate
these rows. Duplicate flags/aliases can allocate multiple actors and later
overwrite a lookup without deleting earlier actors. There is no Higins19 row.
Fresh clearing removes stale mappings; CURRENT reload and later altered state
do not inherit that guarantee.

Actual class and outside-battle HP select ally sprites: zero HP becomes blue
flame; living Rohde11 without joined F11 uses his NPC sprite; otherwise class/table
rules apply. The following fixed NPC rows use regular sprites 30–239, with no
ally HP/class or special-sprite/dedup branch. Let n be the number of actual
ordinary followers. Row j then declares physical `n+1+j`, logical `128+j`,
lookup `32+j`; main stays 0. These successful declaration formulas promise no
capacity safety: follower/script storage is finite and initialization has no
bounds check for arbitrary mass flag enablement.

Complete clear F530 list, `$5E89C..$5E8EE`, ten eight-byte rows plus sentinel:

| j / logical | X,Y | Facing | Sprite | Script or walking origin/range |
| --- | --- | --- | --- | --- |
|0 /128|21,22|Down3|CENTAUR_MALE233|21,22/range1|
|1 /129|34,26|Down3|CENTAUR_MALE233|eas_Init|
|2 /130|8,21|Right0|CENTAUR_FEMALE234|eas_Init|
|3 /131|23,4|Up1|CENTAUR_MALE233|eas_Init|
|4 /132|16,23|Down3|CENTAUR_FEMALE234|eas_Init|
|5 /133|34,7|Down3|CENTAUR_FEMALE234|34,7/range1|
|6 /134|31,15|Down3|CENTAUR_MALE233|31,15/range1|
|7 /135|9,8|Down3|MERCHANT193|eas_Init|
|8 /136|15,5|Down3|MERCHANT193|eas_Init|
|9 /137|41,8|Down3|PRIEST192|eas_Init|

Complete set F530 list, `$5E8EE..$5E938`, nine eight-byte rows plus sentinel:

| j / logical | X,Y | Facing | Sprite | Script or walking origin/range |
| --- | --- | --- | --- | --- |
|0 /128|22,25|Down3|CENTAUR_MALE233|22,25/range1|
|1 /129|41,26|Up1|CENTAUR_MALE233|eas_Init|
|2 /130|30,17|Right0|CENTAUR_MALE233|eas_Init|
|3 /131|8,21|Right0|CENTAUR_FEMALE234|eas_Init|
|4 /132|34,7|Down3|CENTAUR_FEMALE234|34,7/range1|
|5 /133|21,7|Down3|CENTAUR_MALE233|eas_Init|
|6 /134|9,8|Down3|MERCHANT193|eas_Init|
|7 /135|15,5|Down3|MERCHANT193|eas_Init|
|8 /136|41,8|Down3|PRIEST192|eas_Init|

Sprite names omit MAPSPRITE_. With only 67/70/84 enabled, Peter/Astral/Rohde
occupy 1/2/3, then NPCs 4–13 or 4–12. With no followers, NPCs occupy 1–10 or 1–9.
The merchants/priest shift logical indices between lists and physical indices
with followers; their visible roles are not stable physical identities.

Declaration writes position/destination, clears motion and stores facing,
sprite and script. A walking macro encodes `$FF,originX,originY,range` in the
action longword; SetWalkingActscript copies 50 bytes and patches origin/range
words. Three clear F530 walkers use 150 bytes; two set F530 walkers use 100.
Their random-step consumer is constrained by layout, actor destinations and
slopes, not a demonstrated route or immutable rectangular clamp. eas_Init
establishes motion/collision and Up orientation before Idle; orientation
differs from declared facing. VInt action execution can advance actors before
the later event read, so the table is not an observed frozen town frame.

Main is Bowie physical 0, with class/HP-derived sprite, eas_Idle and **returned**
coordinate words/facing, nominally `(21,30)`, Up. It is not a town incarnation
of the battle seed's first active member. Fresh raft handling checks numeric 2
first, makes main a raft, stands/hides followers and declares separate raft 31
at `(64,64)`; other modes use 64/RAFT_MAP. Entering town neither boards a vehicle
nor proves a church visit.

## Empty init ends at an event read

**Confirmed:** full LoadMap selects Map02 main area `(0,0)..(47,31)`, secondary
foreground origin `(0,32)`, background `(0,0)`, parallax 256, zero autoscroll,
type 0 and MUSIC_TOWN. Its 20 ordered F530-controlled 1×1 layout copies use source
`(49,2)` and puddle destinations. These mutate layout; they do not produce F530
or select the population. The complete shared `ms_map2_InitFunction` at `$5EAA6`
is `rts`. It requests no player movement. List/init selection occurs separately;
changing F530 between those reads does not rebuild the selected list.

After init, palette comparison conditionally requests music/fade. WaitForEvent
at `$2591C` then reads pending MAP_EVENT_TYPE. Zero sets view target 0 and calls
SetControlledEntityActScript0: actual numeric 0 installs eas_ControlledCharacter,
numeric 1 installs eas_Raft, and other nonzero values install eas_Caravan.
These are the consumer's choices, even though mode 1/2 enum labels used earlier
name Caravan/Raft. Pending nonzero bypasses installation. The loop reads event
type first, otherwise masks CURRENT_PLAYER_INPUT for A/C.

**This is the endpoint:** a conditional first event/control read, not observed
usable town control. It establishes no conversation, shop/church result, item,
zone interaction, castle admission or Battle31 execution.

## Changed states and retained evidence

| Changed premise or interruption | Required reading |
| --- | --- |
|Outcome, first active member, crop, facing or return map differs|Use the actual pre-scene or egress seed; no automatic route to Pacalon.|
|426/476/526 or early Map72 unlock differs|Keep completion, introduction and admission distinct; battle return skips immediate CheckBattle.|
|716 set/10 clear|Existing Luke init may change the post-init state/position.|
|Explicit map, CURRENT or scroll|Rebuild, temporary reset, healing/init and retained actor lifetimes differ.|
|530 changes between reads|SwitchMap, explicit scroll substitution, town population and layout writes are separate consumers.|
|Negative coordinate bytes, high bytes or post-service registers differ|Apply wildcard, preservation, low-byte replacement and word multiplication at their own boundaries.|
|Layout kind, collision, mode, pending replacement or service progress differs|A row or posted request is not completed travel/control; reached writes persist.|
|Followers, aliases, class, HP or storage differ|Allocate from flags, preserve actual mappings and sprite rules; do not assume capacity repair or lasting Higins following.|
|Missing setup or supplied CURRENT_BATTLE/area differs|No default population/encounter is invented; scrolling retains metadata and can use supplied battle crop.|
|431 set with either531|Admit31; held531 clears unlock only after selecting it.|
|64/65, raft-map or numeric mode differs|Prefix, raft actor, follower relocation and installed control script are separate decisions.|
|Pending nonzero at first town read|Skip control installation and return the event, without a usable-control promise.|

The roadmap's adversarial review consumed complete accepted #1017, earlier
Battle26 outcome/entry owners, [Luke init](../../../../research/chapters/eastern-parmecia/polca-departure-through-luke-recruitment.md),
[membership/follower lifetime](../../../../research/chapters/eastern-parmecia/ribble-may-through-battle10-admission.md),
[common maps](../../../../research/world/common-maps.md), [common scripting](../../../../research/world/common-scripting.md),
[request state](../../../../research/world/map-event-request-state.md),
[request consumption](../../../../research/world/map-event-request-consumption.md), and the
[map/exploration](../../../contracts/world/map-exploration.md) and
[party/roster](../../../contracts/party/party-roster-state.md) contracts.
Selected public payloads, schemas and actual consumers were checked at these
boundaries:

| Fixture / actual consumer | Proof used and stricter limit |
| --- | --- |
|`sf2-common-maps-static-v1`; [maps.py](../../../../../src/sf2tool/h2/maps.py)|`_map_facts` guards switch/CheckBattle/load order. Generic completed-match unlock clearing does not suppress that same admission; full-load area order does not apply wholesale to scrolling. Selected rows/resources remain accepted #1017 joins.|
|`sf2-map-init-static-v1`; [map_init.py](../../../../../src/sf2tool/h2/map_init.py)|`_route_joins` and full fixture comparison retain Map02 routes117/118, pointer joins2/3, shared single-rts body and the complete six-operation Map72 init predicate. These public joins do not replace a fresh private source/pair comparison.|
|`sf2-battle-loop-static-v1`; [battle_loop.py](../../../../../src/sf2tool/h2/battle_loop.py)|`lifecycleFacts.betweenBattleHealing` retains Peter/Lemon, dead-slot skip, maximum HP/MP, mask7 and stat update. Actual selected entry/order and nonzero-word HP predicates remain with accepted research.|
|`sf2-map-event-request-state-static-v1`; [map_event_request_state.py](../../../../../src/sf2tool/h2/map_event_request_state.py)|Caller-local writes/reaching definitions stop at their handoffs. Generic39-program counts do not prove a selected natural route or completed consumer.|
|`sf2-map-event-request-consumption-static-v1`; [map_event_request_consumption.py](../../../../../src/sf2tool/h2/map_event_request_consumption.py)|Actual ExplorationLoop/WaitForEvent/ProcessMapEvent/raft context accesses and `_controls` retain reset/read/clear/branch widths and order. No callee algorithm, live state or rendered outcome follows from those bounded shapes alone.|
|`sf2-map-script-engine-static-v1`; [map_script_engine.py](../../../../../src/sf2tool/h2/map_script_engine.py)|Lifecycle and follower form/handler/source joins distinguish direct loads and temporary following. They do not supply the private Map02/72 program pair, a fresh exploration observation or follower capacity safety.|
|`sf2-map-lifecycle-runtime-v1`; [map_lifecycle.py](../../../../../src/sf2tool/h3/map_lifecycle.py)|Five controlled handler cases and first/final layout-word markers are bounded evidence, not all layout cells, border scrolling, Pacalon reach or fade completion.|
|`sf2-force-state-active-party-runtime-v1`; [force_state_active_party.py](../../../../../src/sf2tool/h3/force_state_active_party.py)|Nine handler cases retain list lag and empty/existing/duplicate follower behavior. They do not prove natural Higins following, fresh town population or persistence.|

Owning reproduction routes are `uv run sf2 h2 common-maps`, `map-init`, `battle-loop`,
`map-event-request-state`, `map-event-request-consumption`, `map-script-engine`,
and bounded `uv run sf2 h3 map-lifecycle` / `force-state-active-party`.
These were inspected, not rerun as private-source/runtime comparisons. Broader
question queues do not erase accepted selected source facts; selected facts do
not close their natural-route/presentation questions. No blocking disagreement
was found. A future contradiction goes to the research owner, not a Layer B repair.

[#1017's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1017#issuecomment-6182642561)
and [PR1020 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1020#issuecomment-6182664667)
retain the pinned source/fixture/request/list review, final head `bbcf476f`,
tree `e84ff8c6` / base `89233108`, author 0.071s and independent 0.062s checks, original
0.065s/corrected 0.064s receipts and exact-head CI38668375749
(scope 15s/research 25s). Original CI38668208926 and its pre-rebase head/tree/base
remain distinct. The unchanged 18,285,979-byte pair's accepted 0.083s receipt is
reused by provenance; it is not regenerated or newly compared here.
Optional missing compressed-layout data, the renamed inspect.py shadow,
literal 640 parser correction, guessed filenames, literal-wildcard errors and
truncated inspections remain in that handoff with linked predecessor results.

[#1018's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1018#issuecomment-6182597910)
and [PR1019 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1019#issuecomment-6182612250)
retain the design environment/source/pair limits, nine-schema/dialogue/table/link
checks, exact-head CI38668093688, corrected common-stats field lookup and all
linked completed diagnostics/failures. Local private source and retained pair
remain unavailable here. No acquisition/copy, fresh private comparison or
runtime claim is made; completed suites are not reset.

This slice's Issue handoff records complete prose/changed-state/five-path/diff,
NPC-table and public-fixture/schema checks, navigation/links/anchors, UTF-8/private
boundary, git diff --check, configured clean-head planner and actual exact-head
CI. Documentation scope skips blanket normal/full/Python/.NET/Godot/H3/H4 and
translation suites. **Unknown:** all actual outcome/seed/flag/HP/mapping/capacity,
numeric mode, motion, post-transition registers, event replacement, decoded
route, service completion, rendering, input/control and persistence premises.
They do not authorize a runtime queue or expand the remake frontier.
