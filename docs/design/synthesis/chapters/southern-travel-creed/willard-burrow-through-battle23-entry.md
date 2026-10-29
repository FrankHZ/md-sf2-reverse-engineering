# Willard Burrow Exploration and Battle23 Admission

**Confirmed:** Willard Burrow has three baseline residents, eight local/exit
warp records and a separately admitted confrontation. A completed confrontation
unlocks Battle23 and requests a return to Map27; the caller's scene flag and
the pending-request consumer are later, distinct boundaries. **Inferred:** the
residents threaten to eat the party. **Unknown:** the natural maze route,
supplied actors/state, completed actions/presentation, usable control and
persistence. This guide ends at the BattleLoop call, before initialization.

Continue from that stop with [Battle23 initial formation and suspended entry](battle23-initialization-and-roster.md),
which consumes accepted #932 research and ends before fresh activation/rounds or
the saved individual-turn read. This predecessor retains its admission boundary.

This English guide continues [Floor World's outbound admission](floor-world-interactions-and-exits.md)
using accepted [Willard Burrow research](../../../../research/chapters/southern-travel-creed/willard-burrow-through-battle23-entry.md)
under [#934](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/934)
and [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
It explains original content and conditional consumers, not a maze solution,
desired gameplay/UI policy or remake implementation. Fnnn means flag nnn;
coordinates, speeds and waits are source operands, not measured performance.
Source paths below are relative to the research's pinned `disasm/` root.

## Entry and the Two Fresh Populations

The Floor World exit ordinarily requests Map27 `(2,7)`, Up, subject to the
[coordinate qualification](#coordinate-overrides-retain-the-quotients-high-byte).
MainLoop calls SwitchMap and CheckBattle before exploration. SwitchMap has no
Map27 substitution. With battle-unlocked F423 set, CheckBattle selects
Battle23 directly, even with scene flag F781 clear; the confrontation is not
a required entry hook. With F423 clear, Map27 admits exploration instead.

On an explicit-map exploration entry, ExplorationLoop clears pending type,
adjusts STEP_COUNTER, heals eligible living/immortal allies, requests fade-out
and disables camera following. It sets CURRENT_MAP27/CURRENT_BATTLE255, loads
tilesets, waits fading, clears VInts, selects/initializes entities and loads
sprites. It clears temporary flags256–383 and sets F80; F423/F523/F781 survive
that range. Common entry initializes sprite-frame state, targets physical0,
calls LoadMap, installs base VInts and dispatches init, followed by conditional
music/fade work. A returning init does not alone establish usable control.

### Three Residents or an Empty Fresh Population

**Confirmed:** `data/maps/mapsetups.asm` selects ms_map27 by default and
ms_map27_flag523 when completed-battle F523 is set. Their six-pointer tables
share EntityEvents, ZoneEvents, AreaDescriptions, Section5 and InitFunction;
only the population pointer differs. Every event accessor reselects the
current setup. The complete local init, `s6_initfunction.asm` at
`$59FB6..$59FB8`, is rts: it starts no scene, writes no EGRESS_MAP and repairs
neither battle nor scene flags.

The baseline declares all three rows below, in order:

| Logical resident | Declared position | Facing | Mapsprite | Action |
| --- | --- | --- | --- | --- |
| 128 | `(43,20)` | Left2 | RAT79 | eas_Init |
| 129 | `(43,21)` | Left2 | WILLARD154 | eas_Init |
| 130 | `(43,22)` | Left2 | RAT79 | eas_Init |

Their eight-byte records occupy `$59F72..$59F8A` in s1_entities.asm. That file
has no terminator of its own. `data/maps/mapsetupsstorage.asm:380–389` includes
both pointer tables, then s1_entities.asm immediately followed by
s1_entities_523.asm and the event files. The latter begins with
ms_map27_flag523_Entities at `$59F8A`, the shared msEntitiesEnd. The baseline
therefore consumes three rows and that terminator; the F523 population starts
at the terminator and consumes zero. This depends on actual include adjacency.

InitializeMapEntities clears 49 physical records and 64 lookup bytes, allocates
selected followers from physical1, then residents, and finally Bowie at
physical0 with the requested position/facing and eas_Idle. DeclareNewEntity
writes current/destination, velocity/travel, sprite, facing, physical ENTNUM,
action/timer and flags. Shared SPRITE_SIZE is 16; follower termination and
raft handling follow. These are declaration values, not observed motion.

All three sprites are regular non-ally values 30–239. Their lookup bytes are
32–34: GetEntityAddressFromCharacter masks the logical byte and subtracts 96
for IDs 128–255. There is no joined-force, HP, class or ally-row duplicate skip
on these rows. Logical129 is Willard here, replacing Floor World's young-man
population on an explicit load. These rows establish neither recruitment nor
battle-enemy activation, and are not a Battle23 roster.

### Followers and Aliases Remain Supplied Premises

The scene uses B=FOLLOWER_B31 and Peter=ALLY_PETER7. With only F67/Peter and
F68/B(Oddler), coherent distinct mappings and no overwrite, they occupy
physical1/2 with eas_Follower1/2, and residents128–130 occupy physical3–5.
Ally membership alone does not allocate Peter; F67 is the follower selector.
Peter's HP/class affects his mapsprite, including BLUE_FLAME for a dead
exploration ally. The scene itself neither validates actors nor revives them.

The accepted [complete follower selection](../../../../research/chapters/southern-travel-creed/willard-burrow-through-battle23-entry.md#admission-setup-and-the-complete-population)
orders F66/Sarah, F67/Peter, F72/Kazin, F73/Slade, F74/B(Astral), F66/Chester,
F68/B(Oddler), F75/Luke, F69/C(Fairy), F70/B(Astral), F74/A(King), F71/Lemon,
F74/D(Minister), F76/Zynk, F77/A(Old Man), F82/Frayja, F83/A(King),
F83/D(Minister), F84/Rohde. Map27 is neither a no-follower map nor an overworld
entry, so F65 supplies no Caravan/A prefix. Only selected rows advance the
action cursor. More followers shift resident physical slots; later B/A/D
declarations overwrite their logical mappings while earlier physical records
remain. Arbitrarily broad flag combinations are not proven supported inputs.

A fresh F523-set entry leaves resident lookup 32–34 at zero. Zero resolves to
physical0, rather than safely skipping an absent actor. `$FF` or other invalid
lookup values have no safe missing-actor guard either. Thus F523 set plus F781
clear can admit the scene with its resident commands aliasing main. Even when
the intended residents exist, logical replacement can change whose action,
destination or speaker a command actually consumes.

Fresh-load raft handling makes main a RAFT in mode2, parks followers at raw
`$6000,$6000` with eas_Standing and declares physical31 at `(64,64)`. Other
modes require F64 and RAFT_MAP27 for a saved raft, using saved coordinates
masked by `$7F`. Lookup 63 points to physical31; supplied state can overwrite
that record. C158 uses lookup 62, while D159 shares raft lookup 63 and can have
its mapping replaced by the raft declaration. Neither overlaps residents 32–34.
No selected scene command changes PLAYER_TYPE, raft state or follower membership.

### Current-Map Traversal Retains Actors

A no-battle MAP_CURRENT255 entry retains CURRENT_MAP, population/lookups and
working layout. It skips explicit tilesets, allocation, temporary-flag clearing
and F80, using UpdateMainEntityProperties/DeclareRaftEntity before common
LoadMap/init work. Main's position/facing and sprite are updated; raft handling
returns for mode2, skips follower repositioning for mode1, otherwise repositions
listed followers before handling saved raft presence/absence. Changing F523
does not rebuild existing actors here. Shared return-only init does not repair
the mappings. Fresh population and retained traversal are different boundaries.

## Local Interactions and Eight Traversal Requests

**Confirmed:** the entity-event table has only a flags0 default and returning
callback. Ordinary resident activation still resolves actual portrait/speech,
disables entity VInt work around the callback and closes windows/reactivates it
afterward. Flags0 requests neither facing change nor restoration. The callback
contains no text, recruitment, inventory, flag or request write; talking to
Willard is not the confrontation trigger in this table.

AreaDescriptions is only rts. Its wrapper starts d7=0, so no local description
is handled. Section5 likewise has a default record/rts; its wrapper starts
d6=0, clears speech, stores caller Y in EVENT_RELATIVE_POSITION, closes windows
and waits for physical0. Chest-items and other-items each contain only endWord.
These are empty local surfaces, not universal absence of global item behavior,
search responses, service rewards or other acquisition paths. Neither direct
resident activation nor local item use calls the confrontation.

### All Eight No-Scroll Records

`data/maps/entries/map27/6-warp-events.asm` at `$A93F6..$A9438` contains exactly
these eight complete type0/warpNoScroll records, then endWord. Each has zero
padding after destination facing. Facing is output, not an approach condition.

| Row | Trigger X/Y | Requested map | Destination X/Y | Facing |
| --- | --- | --- | --- | --- |
| 0 | `(2,8)` | FLOOR_WORLD26 | `(59,4)` | Down3 |
| 1 | `(11,10)` | CURRENT255 | `(30,8)` | Right0 |
| 2 | `(30,8)` | CURRENT255 | `(11,10)` | Left2 |
| 3 | `(33,13)` | CURRENT255 | `(12,15)` | Left2 |
| 4 | `(12,15)` | CURRENT255 | `(33,13)` | Right0 |
| 5 | `(54,2)` | CURRENT255 | `(3,3)` | Right0 |
| 6 | `(255,0)` | CREED_MANSION33 | `(15,9)` | Right0 |
| 7 | `(21,255)` | CREED_MANSION33 | `(15,9)` | Right0 |

Rows0–5 match exact coordinates. Row6 matches any X/Y0 and row7 any Y/X21;
if both match, the first wins, with the same destination. There is no reverse
`(3,3)` record. This table alone proves no maze solution, walkability or
completed post-battle route.

In ordinary esc02_controlCharacter, candidate layout class `$1000` under mask
`$3C00` calls WarpIfSetAtPoint. The matcher signed-divides supplied raw
candidate coordinates by 384, scans current-map records in order until word-1,
and treats negative trigger bytes as wildcards. On a match it writes pending
type 1, copies type/map/X/Y to PARAM1–4 and facing/padding to PARAM5–6, and
sets WARP_SFX=SFX_WARP. Table equality does not prove that the movement/layout
caller actually produces this request; other vehicle callers' legality is not
expanded by their shared matcher.

The [pending consumer](#scene-return-and-pending-consumption-precede-battleloop)
and coordinate limit below also apply to these records. SwitchMap has no
26/27/33/255 substitution; CheckBattle has no 26/33 row. Floor World and Creed
therefore admit exploration under ordinary premises, stopping before their
population/init. CURRENT255 resolves CURRENT_MAP27 for battle lookup: clear
F423 retains the original255 request for exploration; set F423 selects Battle23
instead of local traversal. No row/matcher tests F781 or F523 or consumes an
item. F523 affects fresh population or the completed-battle check, not exit
permission. No selected local command writes EGRESS_MAP or guarantees arrival
at Creed after battle.

## A Destination-Derived Zone Admits the Confrontation

**Confirmed:** the specific zone is `msZoneEvent 255,19,Map27_ZoneEvent0` plus
a default return. RunMapSetupZoneEvent compares low d1/d2 bytes, treating `$FF`
as wildcard: any X/Y19 selects this callback. Set F781 returns without running
the scene; clear F781 invokes cs_59FB8 and sets781 only after it returns. A
nonmatch returns through default. Neither F423/F523 nor party/HP gates it.

Y19 describes the request, not necessarily current actor Y or a completed step.
esc02 seeds a candidate from current coordinates, then checks direction,
area/slope and entities before layout classification. At loc_5294, masked
class `$1400` writes event6. Its parameter words come from `(XDEST+d4)/384`
and `(YDEST+d5)/384` using selected step deltas, before remaining terrain/debug
checks and movement assignment. Layout, mode/action, destinations and pending
state therefore remain premises; current-Y equality alone does not admit it.

WaitForEvent prioritizes pending type over A/C input and skips controller
installation when pending. ProcessMapEvent clears the sampled type before
dispatch. Type6 installs eas_Init on physical0 without waiting, reads d1/d2
from PARAM1/PARAM3 and calls the zone wrapper. Trap6 activates entity VInt work
and executes the script, adding no actor validation or story-state repair.

### The Complete Scene in Reader Order

cs_59FB8 at `$59FB8..$5A09E` has 41 top-level commands, indices 0–40. The
[accepted exact command table](../../../../research/chapters/southern-travel-creed/willard-burrow-through-battle23-entry.md#y19-zone-scene-entry-and-the-complete-confrontation)
owns every source-order operand. The phases below cover the entire sequence;
grouped actors still execute commands in order, not simultaneously.

The first phase places B and Peter for their comments about the smell. The
view then shifts to Willard and the two rats for laughter, rotation and jumps,
followed by Peter's reaction and the dinner-time threat. The later flag and
warp commands connect this staged encounter to conditional battle admission;
the text does not itself implement any harm to the party.

| Indices | Ordered content and completion boundary |
| --- | --- |
| 0–9 | Set text cursor 1733; waited Init for B, then Peter; camera speed 48 and waited destination `(22,15)`; waited absolute B destination `(26,20)`; csWait5; start B's two Right/Left turns without joining it; csWait120; B text1733. |
| 10–13 | Wait Peter's absolute destination `(27,20)`; face B Right; shiver Peter; Peter text1734. Facing and shiver are separate from movement. |
| 14–20 | Wait camera destination `(36,17)`; wait resident 129's absolute destination `(41,21)`;129 text1735 with modifier`$C0`; start looping high-speed rotation on 128,129,130 in order; csWait40. |
| 21–32 | Replace rotations with waited Init on 128,129,130 in order; face each Left; start Jump on 128/129 without waiting, then waited Jump on 130; startEntity128,129,130 in order. |
| 33–37 | Peter text1736;129 nextText1737 then nextSingleText1738, both`$C0`; start another Jump on 128 and waited Jump on 130. There is no second Jump command for 129. |
| 38–40 | setStoryFlag 23 writes F423; request warp27 `(0,0)`, Right; csc_end returns. The caller's F781 write and wrapper wait occur afterward. |

### What the Named Actions and Waits Actually Do

eas_Init zeros acceleration factors, sets speed 32/32, disables acceleration
and deceleration, sets entityObstructable, clears entity/mapUncollidable,
enables autoFacing, sets orientUp, clears immersed, sets shared size 24,
updates the sprite and jumps to eas_Idle. Its flag writes set FLAGS_A bit 7
and clear bits 5/6. orientUp sets orientation bits to 0, not facing Up. Init contains
no position reset, destination wait or motion-ON operation. Idle waits 1 and
branches to itself.

eas_2xRightLeft sets facing Right/Left/Right/Left, updating the sprite and
waiting 30 after each, then jumps to Idle. eas_RotateRightHighSpeed sets motion
OFF and autoFacing OFF, then faces Left/Up/Right/Down, updating the sprite and
waiting 1 after each, and loops back. Both change facing/sprite rather than
tile position. Motion OFF writes ANIMCOUNTER=-1; it is not a general velocity
or position halt. The three later Init replacements end rotation in order,
but do not restart its animation counter. startEntity writes ANIMCOUNTER=0;
it allocates no actor.

csc15 replaces the resolved actor's action pointer, seeding its read timer
with physical index. The waited form polls that record's pointer until
eas_Idle; it has no timeout, HP guard, all-actor join or inherent collision-bit
clear. An alias or later replacement changes what the wait proves. Entity
VInt work skips raw X at or above `$7000`, so invalid/parked state can prevent
progress. A waited Init is an action-pointer boundary, not destination arrival.

All three setDest operands have bit 15 clear. csc29 multiplies their absolute
tile words by 384, writes destination/travel and signed axis velocity magnitude 32,
then waits for current X/Y to equal the actor's current destination. It neither
teleports nor resets the action pointer; replacement can change the wait target.
There is no HP guard on these B, Peter and 129 destination calls.

eas_Jump disables autoFacing, executes moveRel`(0,-1)`, waits 5, executes
moveRel`(0,1)`, waits 5, executes moveRel`(0,0)`, enables autoFacing and jumps
to Idle. Each esc04 delta is relative to the **then-current** position, using
current speed bytes, not a population row or previous destination. Completed
Init supplies 32/32 and clears FLAGS_A bit 5; that bit-clear condition bypasses
esc04's destination-conflict branch through HasSameDestinationAsOtherEntity.
Changed bit 5 can make the scan matter. This is no general collision exemption.

Jump contains no ac_waitDest. Its wait5 operations use the action read timer;
moveRel`(0,0)` targets its then-current position. It need not travel a full tile
up/down or return to its initial Y. Only 130's pointer-to-Idle completion is
joined in either Jump group, never 128/129. Equal bodies imply no simultaneous
completion, final position or total scene duration, especially with aliases,
replacement or interrupted VInts.

The facing/start consumers have a selective alive-status guard: true allies
below 30 without an enemy bit skip for zero HP. B31 and 128–130 bypass it,
including every selected facing/start command. Init/action, destination and
shiver calls do not gain that guard from Peter7 as operand. The scene neither
heals, revives nor recruits Peter or the residents.

Peter's shiver saves shared sprite size/counter, uses size 21/counter-1 and
three times sets/clears FLAGS_B bit 3, updating the sprite and sleeping 5 after
each change, then restores the saved values. It implements a sprite effect,
not a smell sensor or harm rule. Camera speed writes VIEW_SCROLLING_SPEED=48;
each destination disables target-following and waits scrolling. The effective
camera consumer scales tiles by 384 and applies view parallax/offsets and
autoscroll guards. These are requests, not observed framing or actor positions.

### Six Texts, Actual Speakers and Window Handling

Without debug skip, cursor 1733 advances through exactly six texts:

| ID | Speaker / modifier | Content meaning | Control |
| --- | --- | --- | --- |
| 1733 | B / 0 | Sniffing and comparing the smell to a ditch | W1 |
| 1734 | Peter / 0 | Asking whether it is the smell of the reported devils | W1 |
| 1735 | 129 / `$C0` | Laughter | W1 |
| 1736 | Peter / 0 | Calling the rats huge | W1 |
| 1737 | 129 / `$C0` | Welcome | W2 |
| 1738 | 129 / `$C0` | Declaring dinner time | W1 |

The research's [literal text table](../../../../research/chapters/southern-travel-creed/willard-burrow-through-battle23-entry.md#six-texts-and-actual-speaker-selection)
retains keys06C5–06CA and `{N}` breaks. **Inferred:** text and impending battle
suggest an eating threat. No selected command implements smell detection,
creature growth, food consumption or party harm.

Actual logical mapping/mapsprite selects portrait and speech. B/Oddler has
portrait 40/bleep 4; B/Astral has 31/6. Peter base has 7/4 and promoted 48/5;
HP/class or changed mappings can alter these. WILLARD154 has no entry in the
complete sprite-dialog table, so its lookup yields DEFAULT(-1)/bleep6 rather
than a Willard portrait. `$C0` requests right-side/mirrored bits 15/14
(`$C081` for 129), without creating an absent portrait; csc1D preserves an
already open portrait. None uses the `$FFFF` no-speaker sentinel. Missing or
aliased lookups can change properties rather than safely suppressing speech.

nextSingleText/csc00 waits scrolling, displays/increments cursor, closes
portrait/text and sleeps 10. nextText/csc02 at 1737 waits scrolling and
displays/increments but leaves windows open for the following csc00. W1/W2
poll directional/A/B/C input with VInt waits; W2 also updates a prompt marker
and requests SFX_VALIDATION on acceptance. W1 has no such sound request.
Literal controls prove neither accepted input nor completed visible/audible output.

ExecuteMapScript clears SKIP_CUTSCENE_TEXT on entry. Debug mode plus P2 Start
can latch it during dispatch: skipped text commands consume operands without
display or cursor increment, and explicit csWait5/120/40 sleeps are bypassed.
Internal shiver sleeps, action timers/waits, flags and warp still run. A latch
partway through changes later cursor values; all six displayed lines remain conditional.

## Scene Return and Pending Consumption Precede BattleLoop

**Confirmed:** setStoryFlag 23 adds battle-unlocked base 400 and writes F423,
not F523/F781. The scene warp then writes type 1 and PARAM1–5=`0,27,0,0,Right0`.
It loads no map immediately, clears no later parameters and sets no WARP_SFX.
Unlike table warps, its sound consumer uses previously supplied WARP_SFX.

csc_end conditionally waits scrolling if a dialogue window remains, clears
VIEW_SCROLLING_SPEED and restores registers. Only after scene return does the
caller set F781. RunMapSetupZoneEvent closes windows, waits one VInt, then
waits for **physical0** current X/Y to equal its current destination before
restoring registers. That is distinct from 130's Idle wait and joins no other
actor. Missing-resident aliases can make scene commands move or replace main,
so wrapper completion remains conditional.

After these returns, ExplorationLoop reaches SetMoveSfx/WaitForEvent. A
surviving pending warp wins over input, skipping controller installation and
camera retargeting to 0. ProcessMapEvent clears sampled type, requests stored
WARP_SFX and clears it, idles physical0, removes its caller return address to
leave ExplorationLoop, and zeros d0–d4. It loads requested map 27, samples main
position/facing through UpdatePlayerPosFromMapEvent, then applies destination
bytes/facing. Under coherent nonnegative local-coordinate premises, it returns
`d0=27,d1=0,d2=0,d3=Right0,d4=0`. Destination init has not run.

### Coordinate Overrides Retain the Quotient's High Byte

Signed raw X/Y divided by 384 first fills coordinate words; a nonnegative
destination byte replaces only the low byte. Raw X=-384 gives quotient
`$FFFF`; destination 0 produces `$FF00`, not `$0000`. This applies to the scene
and all eight table requests. Ordinary destination tuples therefore require
coherent nonnegative local coordinates. On retained MAP_CURRENT exploration,
UpdateMainEntityProperties tests low-byte negativity but multiplies the whole
word by 384 and stores the low product word. It does not normalize an abnormal
high byte. A wildcard battle trigger likewise repairs no malformed coordinate.

### Independent Flags and Conditional Battle Admission

SwitchMap leaves 27 unchanged. CheckBattle selects its sole battle row 23 only
when F423 is set: `27,23,14,27,16,255,255` means map 27, area origin 23/14,
width 27/height 16 and wildcard trigger X/Y. It writes those area bytes. If F523
is set it clears F423 but still returns the already selected index 23. MainLoop
moves that index to d1 and calls j_BattleLoop with d0=27. **This is the stop:**
before before/start hooks, initialization, roster, AI, activation, rounds or outcomes.

| Changed state at the named seam | Conditional structural consequence |
| --- | --- |
| F423 set on initial Map27 admission, either F781 | Direct Battle23 selection; no exploration scene required |
| Fresh F423 clear/F523 clear | Three resident rows, shared returning init |
| Fresh F423 clear/F523 set/F781 clear | Zero resident rows; scene still admits with missing/aliased lookups |
| Y19 request/F781 set | No scene, new F423 write or scene warp; wrapper still returns conditionally |
| F781 set/F423 clear | Ordinary entry stays exploration; no local repair of the suppressed scene |
| Clear F781, coherent actors, completed scene/wrapper, surviving warp | F423, request, scene return, caller F781, physical0 wait, consumption, Battle23 call in that order |
| F523 changes during CURRENT255 traversal | Existing actors retained; accessors reselect shared event tables |
| F423 and F523 set at CheckBattle | Clear F423, retain current Battle23 selection |
| Interrupted scene or wrapper | Earlier actor/text/camera/flag/request effects may remain; later boundaries are not proved |
| Pending request overwritten between production/consumption | Shared current parameters govern, not an immutable original tuple |

There is no rollback across F423, request storage, F781 and the wrapper. An
interruption before scene return can leave F781 clear with earlier effects;
failure after caller F781 but before consumption can suppress the next scene
without proving admission. These are static order/state distinctions, not
reproduced interrupted scenarios. No selected local command sets completed F523,
grants items or guarantees a post-battle Floor World/Creed route.

## Evidence Review and Retained Limits

**Confirmed provenance:** accepted main `53df4b5cbf6d0b75136d5dfa737d125e2f08d551`
contains #928/PR931 and #930/PR933. Research pins
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The [research reproduction owner](../../../../research/chapters/southern-travel-creed/willard-burrow-through-battle23-entry.md#reproduction-and-remaining-limits)
retains source symbols/addresses, exact scene/action/warp joins and unchanged
payload comparisons. This synthesis adds no original observation or extraction.

Adversarial review reads the whole accepted research and predecessor, then
checks the selected public fixture/schema/verifier boundaries below against
the explanation. [Setup selection](../../../../research/world/map-event-flag-route-selection.md),
[interaction state](../../../../research/world/map-event-interaction-state.md),
[request consumption](../../../../research/world/map-event-request-consumption.md) and
[exploration](../../../contracts/world/map-exploration.md) retain their separate claims.

| Claim | Executable owner and selected boundary |
| --- | --- |
| Shared event tables and silent/default records | `sf2-map-events-static-v1:routing-setup`, tests/fixtures/h2/map-events/routing-setup.json: entity row640, zone104/105, item39, six setup/category joins; schemas/h2/map-events-routing-setup.schema.json. |
| Guarded caller and returns | `:entity-programs` program304/rts, `:zone-programs` programs52/53: chkFlg781, bne default, script cs_59FB8, setFlg781, rts; matching entity/zone/target-program schemas and map_events.py/map_events_fixture.py. Corpus order is not gameplay chronology. |
| Actions, text, camera and lifecycle consumers | `sf2-map-script-engine-static-v1`, tests/fixtures/h2/map-script-engine-static-v1.json: action bridge, placement, lifecycle/presentation, dialogue, camera, transition and script-control facts; schemas/h2/map-script-engine-{entity,dialogue,map-control,script-control}.schema.json via map_script_schema.py; map_script_engine.py `_verify_map_script_engine_fixture`. |
| Pending priority/reset/consumption | `sf2-map-event-request-consumption-static-v1`, tests/fixtures/h2/map-event-request-consumption-static-v1.json: explorationLoop/waitForEvent/processMapEvent; matching fixture schema and map_event_request_consumption.py. Retained independent consumers do not prove a naturally connected scene. |

The selected returning programs agree with the stricter script-before-F781
boundary. The 41-command research table is checked as complete indices 0–40;
the phases above include every command, including the second group's missing 129
Jump. Preserve selective guards, alias effects and low-byte arithmetic rather
than assuming general actor completion or coordinate normalization. The local
design checkout has no upstream/payload pair; #928's accepted direct source and
pair results are retained evidence, not a fresh rerun here. Public fixture
inspection does not independently re-prove every source-only action/prose claim.

Existing reproduction routes are `uv run sf2 h2 map-events`,
`uv run sf2 h2 map-script-engine`, `uv run sf2 h2 map-event-interaction-state`
and `uv run sf2 h2 map-event-request-consumption`, with the retained comparison
recipe at the research owner. Do not regenerate payloads for this prose.
Direct acceptance reviews whole owned prose/diff, counts/order, relative
links/fragments/tables/fences, English/private boundary and `git diff --check`,
then the configured clean committed planner and actual final-head CI. No local
blanket Python/.NET/Godot/H3/H4/translation suite is selected solely for prose.

Preserve [#928 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/928#issuecomment-6176708249)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/931#issuecomment-6176742983):
exact Git objects, CI38624813497, unchanged 18,285,979-byte pair/0.080s,
inspection failures and corrected D159/raft63 hypothesis, with linked
#924/#920 predecessor corrections. Preserve
[#930 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/930#issuecomment-6176889129)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/933#issuecomment-6176904018),
CI38626011000 and linked #926/#923 records. These receipts replace no completed
result and need no copied history ledger. No unmerged Battle23 initialization
content is consumed.

**Unknown:** natural arrival/maze/zone/exit reach; actual flags, actors/lookups,
aliases, HP/class/party, layout, mode/action, request and current/destination
state; completed input/text/camera/actions, visible/audible timing, usable
control and save/load persistence. No destination Floor World/Creed init or
Battle23 initialization/outcome is evaluated. [ADR0014](../../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../../../decisions/0016-remake-start-evidence-deferral.md) retain these
limits without an automatic runtime queue. This completes neither the campaign
nor the design epic and changes no remake acceptance boundary.
