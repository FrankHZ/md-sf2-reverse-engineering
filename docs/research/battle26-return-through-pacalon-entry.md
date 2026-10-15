# Battle26 return through conditional Pacalon entry

Evidence date: 2026-10-14. **Confirmed:** Battle26 victory returns a pre-scene seed
to a fresh exploration entry. That rebuild discards Higins's temporary scene
following without removing his force membership. The eastern Map72 warp
requests a scrolling transition to Map71, substituted with Map78 under F530;
this transition retains the live population and has no immediate battle scan.
Either overworld has two no-scroll Pacalon entrances. Their return to MainLoop
can admit Battle31 under F431; only the no-battle alternative initializes
Map02 exploration. **Unknown:** natural walking reach, actual supplied state,
completed services, rendered transition, usable town control and persistence.

## Agreement and evidence boundary

Standard rigor for [Issue #1017](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1017)
under [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
The state joins cross battle return, actor rebuilding, scrolling, mutable
events and battle admission. Accepted starting base is
`0e8d30887a401552228e94058507b7ef2c8e9a0a`. This document is the sole tracked
write; there is no shared-path or stacked dependency. Independent synthesis
and UI692 supply no evidence.

Consume accepted [Battle26 outcomes and Higins recruitment](./battle26-outcomes-and-higins-recruitment.md),
[activation](./battle26-activation-and-first-turn-dispatch.md),
[initialization](./battle26-initialization-and-roster.md),
[admission](./battle26-admission-and-geshp-confrontation.md),
[Battle25 return](./battle25-outcomes-through-ketto-entry.md),
[Luke-init predicates](./polca-departure-through-luke-recruitment.md),
[party/follower continuation](./ribble-may-through-battle10-admission.md),
[common maps](./common-maps.md), [common scripting](./common-scripting.md),
[request state](./map-event-request-state.md) and
[request consumption](./map-event-request-consumption.md).
Stop at the first conditional Map02 event/control read, or the Battle31
diversion before execution. Town conversations, shops, church, items, zone
interactions, castle travel and Battle27/31 initialization/outcomes are excluded.

Source paths are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The selected source checkout is clean at that pin. The accepted USA retail
ROM is SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
`sf2enums.asm`/`sf2const.asm` own numeric meanings. Fnnn denotes game flag
nnn; map indices and battle indices are separate. Short `map/` and `entity/`
paths below are under `code/common/scripting/`. Table rows are zero-based;
address ranges are half-open.

Minimum evidence is selected source, actual fixture consumers and the retained
18,285,979-byte map-init/script pair. Its accepted 0.083s comparison is reused,
not a fresh run. Selected joins are linear, budgeted below one minute/512 MiB
with small local receipts. No acquisition, runtime, extraction regeneration,
maintained checker or new schema is needed. The collision boundary below is
the selected source consumer and map resource, not a new decoded-cell or route
simulation claim. Replan contradictions or supporting-tool expansion.

## Returned seed and fresh exploration lifetime

**Confirmed:** the accepted outcome owner establishes that victory seeds
PARAM2 with the current map (nominally72), PARAM3/4 with the first active
member's combatant coordinates plus battle crop, and PARAM5 with the mapped
entity's facing, **before** the rescue scene. Scene Bowie `(19,16)`, camera
`(13,11)` and Higins's moves do not replace those bytes. Reached completion
clears426/sets526;476 is retained. First rescue can set joined19/active51 and
append a temporary physical follower; held526 skips the scene. The wrapper's
JoinForce0 is neither a second Higins join nor a seed repair.

Defeat/Egress/Angel Wing instead return the supplied savepoint-derived seed
and retain their reached costs/flags. They are not forced back to72. A resumed
F88 battle uses its supplied state. This document does not repeat those outcomes
or assume an ordinary victory when the returned map/coordinates differ.

`code/gameflow/mainloop.asm:alt_MainLoopEntry` calls SwitchMap and enters
ExplorationLoop with no d4 sign test and no intervening CheckBattle. Thus a
returned72 is not immediately re-admitted to Battle26 by a surviving unlock.
`code/common/maps/mapinit_0.asm:SwitchMap` scans six-byte records by word map
identity; the first matching set flag replaces d0 and returns. Its complete
`data/maps/global/flagswitchedmaps.asm` table is3/F640→4,71/F530→78,
58/F999→59. There is no72 or2 replacement and no reverse78→71 rule. A
MAP_CURRENT255 request is not first resolved by this table.

`code/gameflow/exploration/explorationfunctions_2.asm:ExplorationLoop`
performs the following order:

1. Clear pending event type; subtract20000 from STEP_COUNTER and clear it if
   the signed branch fails. This is word arithmetic/CCR behavior, not a general
   unbounded integer clamp. A nominal30000 becomes10000; small ordinary
   nonnegative counters become0. The producer's remaining parameter bytes are
   not all cleared by the event-type write.
2. Call HealLivingAndImmortalAllies, request all-palette fade-out, set view
   target byte-1, and save the requested map word.
3. For an explicit low map byte other than255, write CURRENT_MAP and
   CURRENT_BATTLE255, load tilesets, wait for fade, clear VInt functions,
   obtain the selected setup entity list, initialize entities/mapsprites,
   clear temporary F256–383, and set80.
4. Initialize exploration sprite counters; LoadMap with the saved map word
   and d0=-1; install base VInts; call the selected setup init. Compare the
   palette state and conditionally request PlayMapMusic/FadeInFromBlack.
5. Set movement sound and enter WaitForEvent. Pending zero installs control
   on physical0; a pending event bypasses that installation.

Healing scans all30 allies, not just joined/active members: Peter7 and Lemon28
are always eligible; others require nonzero word HP. Eligible actors receive
max HP/MP, status masked to STUN|POISON|CURSE, and stat recomputation. Higins19
is not immortal: dead19 remains dead, living19 is healed regardless of F19.
The persistent three status bits are retained only if already present. Escape
MP cost or defeat-restored Bowie HP is therefore not necessarily the value
after this later healing call.

`InitializeMapEntities` scales the supplied main coordinate **words** by384,
calls ClearEntities, initializes flag followers, consumes the fixed setup list,
and declares main physical0 with Bowie sprite semantics and supplied facing.
ClearEntities resets actor records/mappings and the walking-scratch pointer;
the old scene scripts and transient follower chain are not reused. The fresh
follower list is terminated after declarations. F19/F51 and the story/battle
flags survive the temporary-flag clear. There is no Higins row in the follower
table or Map72's empty fixed list, so recruitment does not create a lasting
exploration Higins actor. A missing zero mapping can resolve an unpopulated
logical actor to physical0; it is not evidence of a separate invisible Higins.

Map72 has one default setup and `msEntitiesEnd` only for fixed entities.
It is an overworld and not follower-excluded. Under65, the Caravan prefix is
included before ordinary followers. Its init at$4FF2E runs the accepted
Luke scene only when716 is set and joined10 clear; otherwise it returns.
This does not test526 or automatically request Battle26. The supplied alternate
history belongs to the linked Luke owner; it can alter the post-init seed/state.

BattleLoop clears PLAYER_TYPE; the ordinary Battle26 outcome does not change
it, so the selected return uses numeric mode0. Altered modes remain distinct.
A CURRENT255 exploration entry still clears pending, adjusts the counter,
heals, fades, updates main/raft, loads the current map and runs init, but does
not rebuild actors, replace CURRENT_BATTLE or clear temporary flags. Negative
coordinate low bytes skip that branch's main-coordinate update. Direct
script LoadMap and scrolling are different lifecycles.

## Eastern warp and the scrolling consumer

**Confirmed:** selected Map72 warp row8 at$C1C3E is source `(63,255)`,
scroll RIGHT, destination71 `(1,255)`, facing RIGHT0. Its eight encoded bytes
mean X63/any Y, transition byte$10, map71, X1/Y$FF, facing0/padding0.
`sf2mapmacros.asm` defines scroll as$10+direction; no-scroll is0.
No526/426/476, Higins membership, HP or party-size test guards this row.
Earlier matching rows retain precedence; a table record does not establish
arrival at the edge or compulsory eastward travel.

The ordinary walking consumer `entityscriptengine_2.asm:esc02_controlCharacter`
reads the current layout and requires masked kind$1000 before calling
`exploration.asm:WarpIfSetAtPoint`. That producer divides internal coordinates
by384, scans rows in order, treats **negative source bytes** as wildcards,
and writes event type1, PARAM1–4 as a longword, PARAM5/padding as a word, plus
WARP_SFX. The table's word$FFFF terminator is checked first. Wildcard matching
is not a bounds or collision check. The caller separately handles movement,
obstruction, flags and travel; kind$1400 produces a zone event instead.

ProcessMapEvent clears type before dispatching the saved event value. The
nonzero PARAM1 branch of ProcessMapEventType1_Warp:

- Idles physical0; reads destination map byte; if it is71, checks530 and
  substitutes78 when set. Writes CURRENT_MAP **before** transition services.
  This is explicit code, not a call to SwitchMap or its generic table.
- Calls ProcessMapTransition. It selects current-map tiles/blocks/layout and
  uses PARAM1 &3 to dispatch the scrolling direction. The selected RIGHT path
  adjusts plane counters and sprite-table projection, queues DMA and waits for
  VInts. It neither declares a new map population nor enters MainLoop.
- For nonnegative destination coordinate bytes, updates main position and
  destination; negative bytes preserve that coordinate. Y is read after clr.w
  d0, so selected$FF leaves live Y/YDEST untouched. X is read with move.b into
  the post-transition d0 and multiplied as a word: the exact write is the low
  word of `((d0.w & $FF00) | PARAM3) * 384`, not an implicit byte zero-extension.
  X1 is the record's request; altered service/register state must not be
  normalized into a guaranteed world-coordinate word.
- Samples actual main X/Y with signed division by384 and its current facing;
  calls DeclareRaftEntity; returns to the existing exploration loop.

The scrolling branch never copies PARAM5 into entity facing. RIGHT is both
the transition direction and recorded facing, but normal facing comes from
the moving main actor; an injected mismatched facing record is not applied.
It also does not submit/clear WARP_SFX through the no-scroll sound branch,
heal, subtract20000, clear temporary flags, reset CURRENT_BATTLE, rerun map
setup/init, or call CheckBattle. Reached writes survive an interrupted service.
On return the existing loop sets movement sound and calls WaitForEvent again:
pending zero reinstalls the mode-selected control script; a newly pending
event bypasses that installation. No fresh exploration entry intervenes.

Retaining population does **not** mean every position is preserved.
DeclareRaftEntity returns immediately for numeric mode2 (PLAYERTYPE_RAFT).
Mode1 (PLAYERTYPE_CARAVAN) skips follower relocation. Other modes walk the
current EXPLORATION_ENTITIES list and place each listed physical actor's X/Y
and destination at the sampled main tile, then apply the raft gate. Scripts,
mapping identities and walking scratch remain live; no flag-based list is
reconstructed. Duplicated or altered physical IDs are consumed as supplied.

The raft gate requires64. Matching RAFT_MAP declares physical31 from masked
raft X/Y bytes, Left, raft sprite/standing script, lookup offset63; mismatch
clears that lookup and moves raft off-map. Clear64 returns without this repair.
Physical raft31 is distinct from logical FOLLOWER_B31/lookup31. A newly set
follower flag during scrolling does not itself allocate its actor; changing530
after the map choice does not retroactively replace the loaded map.

## Map71/78 resources, absent setups and encounter boundary

**Confirmed:** neither71 nor78 appears in `data/maps/mapsetups.asm`.
`mapsetupsfunctions_1.asm:GetCurrentMapSetup` reaches the table terminator and
returns ms_Void/$FFFF. GetMapSetupEntityList returns that sentinel;
RunMapSetupInitFunction skips the call. This is absence of a setup, not a
populated default setup. A hypothetical explicit71/78 exploration entry still
performs shared ClearEntities/follower/main/raft initialization, with no fixed
NPC list. The selected scroll does not even call those setup consumers: its
population is the one retained from72.

`data/maps/entries.asm` selects distinct Map71s0_Blocks/Map71s1_Layout and
Map78s0_Blocks/Map78s1_Layout. Both headers use palette0, tilesets101–105.
Both area records are main `(0,0)..(63,63)`, other origins0,0, parallax256,256
for both layers, zero autoscroll, type254 and MUSIC_MAIN_THEME. Their flag and
step tables are just endWord. Different layouts remain different collision
inputs; identical areas/warps do not make the drought/restored worlds identical.

ProcessMapTransition calls LoadMapBlocksAndLayout and reads/scales the first
area rectangle before direction dispatch. It does **not** call LoadMapArea:
area/music/animation metadata is not refreshed through the full-load consumer.
Map72's selected area has the same stated rectangle/layer properties. Retained
or altered area state remains a premise; loading new layout is not an implicit
fresh camera/area/population initialization. LoadMapBlocksAndLayout also honors
actual CURRENT_BATTLE: ordinary255 avoids its battle-crop copy; supplied
non255 can apply the supplied crop even on this scrolling path.

Walking continues to use the loaded layout's collision/type bits, current
actor destinations, player mode and pending event state. No decoded path from
the battle return seed or eastern edge to the town doors is established here.
Map71/78 have **no** entries anywhere in the ordered BattleMapCoordinates
table. Their missing setup also makes RunMapSetupZoneEvent skip callback
selection, then perform its common VInt/main-stop wait. There is no implicit
random-battle default supplied by ms_Void. This does not rule out an externally
supplied event or a later warp to another battle-bearing map.

Map72 before departure retains its own accepted zone/counter behavior: its
Battle26 helper checks526, requests immediately when clear, or uses the
counter/range8 retry when set; accepted Map72 ordinary scans order8,24,26 by
unlock408/424/426. The selected border scroll performs none of those scans.
A previous pending event, a differently reached zone, or a later no-scroll
CURRENT request can therefore lead to a different path. No encounter cadence
or uninterrupted walk follows from the endpoint coordinates alone.

## Pacalon request and the Battle31 diversion

**Confirmed:** the first two eight-byte warp records are identical in both
overworlds:

| Map / row addresses | Source | Destination |
| --- | --- | --- |
|71 rows0/1: $C0694/$C069C|`(21,15)` / `(22,15)`|No-scroll Map02 `(21,30)`, Up1|
|78 rows0/1: $C69E4/$C69EC|`(21,15)` / `(22,15)`|No-scroll Map02 `(21,30)`, Up1|

These records have no530/526 or membership gate. They use the same layout-kind
producer and mutable single event slot. No path traversal or other exit is
implied. The no-scroll consumer submits and clears WARP_SFX, idles main,
unwinds the exploration caller, clears d0–d4 **words**, reads map2, samples
actual main coordinates, replaces nonnegative destination **low bytes** with
21/30, reads facing1, and returns d4=0. Sampled high coordinate bytes are not
erased by those replacements; nominal normal coordinates yield `(21,30)`.
Changed sentinel/high-byte/pending state must use the actual operations.

MainLoop now takes its ordinary `@Start`: SwitchMap, **then CheckBattle**.
SwitchMap leaves2 unchanged. The full45-row BattleMapCoordinates scan has one
Map02 candidate: row31, crop `(0,0,48,32)`, trigger `(255,255)`, unlock431.
Rows for other maps are skipped even if unlocked; the crop is output data,
not a trigger containment predicate. The wildcard matches the supplied entry
coordinates regardless of21/30.

| Supplied state at this scan | Disposition |
| --- | --- |
|431 clear, either531|No candidate; d7=-1; enter Map02 exploration.|
|431 set,531 clear|Select31 and write its crop; main calls BattleLoop.|
|431 set,531 set|Select31, write crop, clear431, and **still admit31 on this call**.|

530 is not this admission gate;526 and Higins F19/F51 are not tested here.
Battle26 victory does not newly set431 or530. CheckBattle resolves a supplied
CURRENT255 from CURRENT_MAP, but an intact selected town record is explicit2.
The Battle31 branch stops here before initialization/execution. Neither a
completion flag nor the town name warrants an unconditional exploration claim.

## Conditional Map02 population and first event/control read

**Confirmed:** with no admitted battle, explicit2 uses the fresh exploration
order above. Current map becomes2/current battle255; healing and actor rebuild
occur again. F530 clear selects ms_map2 at$5E86C; set selects
ms_map2_flag530 at$5E884. Retained map-init routeJoins sourceOrder117/118
and pointerTableJoins2/3 bind both to ms_map2_InitFunction at$5EAA6.
Both six-pointer tables contain entities, entity events, zone events,
descriptions, item events and init; selecting an event pointer does not execute
that interaction. Population, entity-event and description pointers differ;
zone/item/init pointers are shared.

Map02 is not an overworld or follower-excluded map: no Caravan prefix is
added by65. The complete ordinary follower row order from
`data/scripting/entity/followers.asm` is:
66/Sarah,67/Peter,72/Kazin,73/Slade,74/B/Astral,66/Chester,68/B/Oddler,
75/Luke,69/C/Fairy,70/B/Astral,74/A/King,71/Lemon,74/D/Minister,76/Zynk,
77/A/Old Man,82/Frayja,83/A/King,83/D/Minister,84/Rohde.
A/B/C/D are logical30/31/158/159, lookup offsets30/31/62/63.
Each enabled row allocates the next physical actor from1, writes its lookup,
installs the next follower script and appends its physical ID. Flags admit
rows independently of force membership/HP; duplicates allocate separately
and later alias writes can replace the lookup without removing earlier actors.

Ally sprite selection uses actual class and outside-battle HP: zero HP becomes
blue flame; living Rohde11 without joined11 uses his NPC sprite; otherwise the
class/table rules apply. Fixed NPC sprites below have no ally HP/class test.
No follower row names Higins19; persistent F19/F51 does not recreate his
temporary rescue actor. Fresh clearing removes stale mappings before this
allocation; later supplied/corrupt mappings and CURRENT reloads do not acquire
that guarantee.

Let n be the number of actual ordinary follower declarations. Each following
fixed-list row j declares physical `n+1+j`, logical NPC `128+j`, lookup `32+j`.
All19 rows across the alternatives have regular NPC sprite indices30–239,
so no ally-dedup or special-sprite allocation branch changes these formulas.
They describe successful bounded declarations, not a capacity guarantee for
arbitrary mass flag enablement. Follower script/storage capacity is finite
and the initializer has no bounds check. Main remains physical0.

Complete F530-clear list, `$5E89C..$5E8EE` (ten8-byte rows plus sentinel):

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

Complete F530-set list, `$5E8EE..$5E938` (nine8-byte rows plus sentinel):

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

Sprite names have MAPSPRITE_ prefix. For example with only67/70/84 enabled,
Peter/Astral/Rohde occupy1/2/3; clear530 NPCs occupy4–13, set530 NPCs4–12.
With no followers they occupy1–10 or1–9. Merchant/priest logical indices
change with the selected list; they are not stable physical identities.

The common declaration consumer sets coordinate/destination, clears motion
fields and stores facing/sprite/script. Walking macros encode `$FF,originX,
originY,range` in the action longword; SetWalkingActscript copies50 bytes and
patches origin/range words. Clear530 uses150 bytes for three walkers, set530
uses100 for two. Their eas_Walking/random-step consumer requests bounded
steps subject to layout, actor destinations and slope handling, not a measured
route or immutable rectangular clamp. eas_Init establishes motion/collision
and Up orientation before Idle; orientation is distinct from declared facing.
VInt action execution can advance actors before the later event read.

The main actor is declared from the **returned** coordinate words/facing,
nominally `(21,30)`, Up, with Bowie class/HP-derived sprite and eas_Idle.
It is not the battle's first active member instantiated as town main. The
fresh raft tail checks numeric mode2 first: it makes main a raft, stands/hides
followers and declares separate raft31 at `(64,64)`. Other modes use the64/
RAFT_MAP gate. Merely entering2 does not board a vehicle or visit a church.

Full LoadMap selects Map02's main area `(0,0)..(47,31)`, secondary foreground
origin `(0,32)`, background `(0,0)`, parallax256, zero autoscroll, type0 and
MUSIC_TOWN. `data/maps/entries/map02/3-flag-events.asm` contains20 ordered
530-controlled1×1 copies from `(49,2)` to its puddle destinations; these are
layout writes, not entity population or a new530 producer. No init request moves the player.
The entire shared `ms_map2_InitFunction` is rts. List selection and init
selection occur separately; changed530 between them does not rebuild the list.

After init returns, palette comparison conditionally calls music/fade, then
WaitForEvent at$2591C reads MAP_EVENT_TYPE. Zero selects view target0 and
SetControlledEntityActScript0: numeric0 installs eas_ControlledCharacter,
numeric1 installs eas_Raft, and other nonzero values install eas_Caravan.
These are actual consumer choices; enum labels alone do not invert them.
Pending nonzero bypasses installation. The loop then reads event type first,
otherwise masks CURRENT_PLAYER_INPUT for A/C. This is the stopping boundary,
not observed usable control or an audit of the resulting town interaction.

## Changed-state acceptance and reproduction

| Changed state or interruption | Required distinction |
| --- | --- |
|Different outcome/return map, active member, crop, facing or pending bytes|Use the actual pre-scene/egress seed; no automatic Pacalon route.|
|426/526/476 or early Map72 unlocks differ|Completion, admission and intro flags remain separate; battle return skips the immediate scan.|
|716 set/10 clear|Map72 init can run the already-owned Luke scene; do not promise unchanged return coordinates.|
|Explicit map versus CURRENT or scroll|Rebuild/temp reset/init and retained-state branches differ.|
|530 differs or changes between reads|Generic map switch, explicit scroll substitution and town list selection are separate reads.|
|Source/destination negative coordinate bytes, altered high bytes or service registers|Wildcard/preserve/word-multiply/low-byte replacement contracts differ; no blanket coordinate normalization.|
|Wrong layout kind, collision, mode, pending replacement, interrupted DMA/VInt/fade|A row or posted event is not proof of completed walking/transition/control.|
|No/duplicate followers, changed aliases/class/HP or insufficient storage|Flags control allocation; mappings/physical IDs and actual sprite consumers matter; no capacity repair.|
|F19/F51 held but no transient actor|Force membership survives fresh rebuild; Higins following does not.|
|Absent71/78 setup or altered CURRENT_BATTLE/area|ms_Void is not a populated default; scroll retains metadata/state and can use a supplied battle crop.|
|431 set with either531|Divert to31; set531 clears unlock only after selecting the same battle.|
|64/65, raft-map or numeric player mode differs|Prefix, physical raft, follower relocation and installed control script remain distinct.|
|Pending event before first town read|Skip control installation and return event; no unconditional town-control claim.|

Load `local/private-inputs.ps1` in the launching process. Reuse the unchanged
pair under the [retained recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary);
do not regenerate its comparison. Direct bounded review should:

1. Check clean source pin and the accepted dependency diff. Join actual
   request-consumption fixture contexts/accesses to source statements and
   `_controls`; compare Map02 route/pointer/init records and Map72 init body
   from the retained pair to their selected complete source bodies.
2. Read MainLoop, SwitchMap, full flag-switch/battle-coordinate tables,
   ExplorationLoop/WaitForEvent/ProcessMapEvent, complete warp consumers,
   ProcessMapTransition/RIGHT/layout loading and actual follower/raft consumers.
   Compare selected warp rows through macros, branch polarity and write widths;
   check no hidden full rebuild/init/battle call on the scrolling path.
3. Compare both complete Map02 lists/pointers, empty init, map/area/flag-copy
   records, follower table and allocation/class/HP/walking/control consumers.
   Recheck changed-state alternatives against those use sites, not row counts.
4. Review the entire prose, UTF-8/LF, local links/anchors, sole-path/private
   boundary and `git diff --check`. On the clean committed head run
   `uv run sf2 verify plan --base origin/main --head HEAD`; record exact-head
   scoped CI in the Issue handoff. Documentation-only scope skips blanket local
   normal/full/Python/.NET/Godot/H3/H4 suites.

**Unknown:** actual outcome/seed/flags, natural route and encounter reach,
live map/layout/area/mappings/follower capacity, numeric mode and actor motion,
post-transition registers, event replacement/order, service completion,
rendering, available input/control and save/load persistence. The optional
source-side compressed-layout read was unavailable; no fresh cell decode or
walkability proof is claimed. None of these limits admits a runtime queue.

Preserve [#1013 final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1013#issuecomment-6182436816),
its [original receipts and diagnostics](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1013#issuecomment-6182394080)
and [PR1016 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1016#issuecomment-6182452296).
They retain head12a6ab08/tree125e38de/baseee85e039, CI38666866738
(scope16s/research33s), original CI38666515728, selected reviews0.129s/0.143s,
independent0.132s and reused pair0.083s. Preserve the linked predecessor
failures/Unknowns and all ignored source/configuration/receipts. Completed
suites are not reset. Freeze a clean pushed Draft PR for independent review;
no merge, cleanup, archive, runtime or next Issue.
