# Elven Village departure through Harpy Pond entry

Evidence date: 2026-10-14. **Confirmed:** three selected village exits return to
Southwest Parmecia; its independently selected west scroll retains the current
population into Devil's Tail. The F520-clear call-for-help scene requests Harpy
Pond at `(11,22)`, Up; the direct entrance requests `(10,24)`, Up. Ordinary
admission tests supplied F420 before pond population/init. Without that battle
admission, both default and F520-alternate setups share the F470-gated initial
prompt. **Inferred:** these scenes introduce the pond rescue. **Unknown:** a
natural route, actual caller state, completed services, usable control and
persistence. This is not an established mandatory village-to-pond itinerary.

## Agreement and provenance

Standard rigor under [Issue #864](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/864)
and the [content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
a bounded static account supporting the content guide and a later separate
approach/admission slice. Accepted starting base:
`d3a17919de9b79288495486b1a470c761b71e97b`. This document is the sole owned path;
there are no shared edits, new machinery or stacked dependencies. Stop at
conditional pond exploration/control, an unrelated pending/admitted continuation,
or Battle20 admission **before its opening**. Exclude Map24, Map51 approach and
exit-warning programs, Elric rescue/recruitment, F751 passage opening, Fairy
Cave/Creed, Battle20/21 content, combat, terrain reconstruction and runtime
acquisition. Material coupling beyond those boundaries requires replanning.

Consume accepted [village entry](./battle19-outcomes-through-elven-village-entry.md),
[local interactions](./elven-village-conversations-and-local-exploration.md),
[Caravan travel](./caravan-boarding-and-southwest-parmecia-travel.md),
[raft/controller selection](./polca-raft-boarding-through-south-parmecia-entry.md),
[request writes](./map-event-request-state.md),
[request consumption](./map-event-request-consumption.md) and
[random-battle structure](./map-event-random-battle-state.md).
All source paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained payloads bind it and USA
retail ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Short map paths begin at `data/maps/entries/`; entity consumer filenames begin
at `code/common/scripting/entity/`. Fnnn means flag nnn; indices are zero-based.
Coordinates, speeds and waits are source operands, not observed positions, elapsed time or presentation.

Reuse the 18,285,979-byte retained map-init/map-script-engine pair and bounded
selected source/fixture reads. Linear comparisons were budgeted below 512 MiB
and one minute, with small prose/receipts, no regeneration or game-material
upload; they passed in 0.160 seconds. Reassess before material resource growth.

## Village exits and conditional Southwest Parmecia return

**Confirmed:** the complete `data/maps/entries/map23/6-warp-events.asm` orders
these rows first; each writes no-scroll 0, Map68, `(48,31)`, Down:

| Row | Matching input tile | Selection boundary |
| --- | --- | --- |
| 0 | Any X, Y 1 | North exit; takes precedence at a corner matching another row. |
| 1 | X 0, any Y | West exit. |
| 2 | Any X, Y 30 | South exit. |

The later east `(31,24)` row leads to Map24 and is excluded. The two current-map
stairs have their accepted local owner. `exploration.asm:WarpIfSetAtPoint`
(under `code/gameflow/exploration/`) divides supplied internal coordinates by 384,
scans in order using negative-byte wildcards, writes event 1/parameters 1–6 and
WARP_SFX. Walking and vehicle actions reach it through their own movement checks
and working-layout `$1000` branch. Neither the row nor the handler establishes
terrain access, transport eligibility or a natural path from village control.

`explorationfunctions_2.asm:ProcessMapEvent` clears pending before dispatch.
Its no-scroll consumer submits/clears WARP_SFX, idles physical 0, unwinds the
exploration return, samples current coordinates, replaces nonnegative requested
X/Y and returns `(68,48,31,Down,d4=0)`. It does not normalize PLAYER_TYPE.
`code/gameflow/mainloop.asm` then runs SwitchMap and CheckBattle before fresh
ExplorationLoop. The complete `data/maps/global/flagswitchedmaps.asm` contains
no source-map 68, 67 or 51 substitution. The complete 45-row
`data/battles/global/battlemapcoords.asm` table and
`code/common/maps/getbattle.asm:CheckBattle` give the following selected rows:

| Map | Sole row / battle | Area origin, size | Trigger / supplied unlock / completion |
| --- | --- | --- | --- |
| 68 | 19 | `(5,19)`, 48x32 | Any X/Y; F419; F519. |
| 67 | 21 | `(21,0)`, 32x42 | Any X/Y; F421; F521. |
| 51 | 20 | `(0,0)`, 18x26 | Any X/Y; F420; F520. |

The unlock and trigger bytes admit battle, not containment in the area rectangle.
A set completion flag clears its corresponding unlock **after selection without
canceling this call's admission**. On the village exit, F419 set therefore
selects Battle19 even with F519 set; stop on that unrelated admission. F419 clear
allows fresh Map68. The exits themselves set none of those flags.

Fresh supplied-map exploration clears pending, subtracts 20000 from STEP_COUNTER
with signed-word negative-result clamp to 0, heals living/immortal allies, fades,
loads tilesets and writes CURRENT_MAP=68/CURRENT_BATTLE=255. It rebuilds actors,
clears temporary flags 256–383, sets F80, loads the map and runs init. Map68 has
one setup, no fixed actors, and return-only init. Population allocates flag-selected
followers from physical 1 and main 0 separately; main 0 is absent from the fresh
EXPLORATION_ENTITIES list. F65 plus overworld 68 selects Caravan/FOLLOWER_A first,
then the normal table. F65 clear does not supply that row or repair mode 1.

The accepted entry/travel owners retain the complete follower table and raft
tail. With only F65/F67/F68 among follower selectors, physical 1/2/3 are Caravan,
Peter and Oddler/FOLLOWER_B. Later F74/F77/F83 rows can overwrite FOLLOWER_A's
lookup; F70 can overwrite Oddler's FOLLOWER_B lookup. Follower allocation tests
flags, not HP or force membership. Fresh mode 2 puts the raft sprite on main 0,
parks the current follower list at `(64,64)` with standing actions, and declares
separate raft 31 there; other modes declare raft 31 only for F64 and matching saved
RAFT_MAP. Saved raft position and PLAYER_TYPE remain supplied. Fresh mode 1 does
not replay boarding. Pending zero eventually permits the current-mode controller;
this does not prove usable input or progress to the west border.

## Independent west scroll into Devil's Tail

**Confirmed:** `map68/6-warp-events.asm` row 4 is X 0/any-Y, scroll Left, Map67,
destination X 62/Y 255, facing Left. Its earlier row 3 is any-X/Y 0 to Map73; thus
at `(0,0)` the north row wins. Rows 0–2 are the three specific interior entrances;
row 5 is the east border. The selected west row assumes no earlier match.

`ProcessMapEventType1_Warp:loc_259CC` handles nonzero parameter 1 inside the
existing ExplorationLoop. It idles 0, writes CURRENT_MAP=67 and calls
`code/common/maps/mapload.asm:ProcessMapTransition`. That routine loads selected
tilesets and blocks/layout, installs scrolling VInts, dispatches the parameter 1
low direction bits to `overworldMapTransition_Left`, performs its plane/sprite
scroll work and returns. The sole special map-substitution check in this scroll
consumer is drought Map71/F530; it is not selected by 67.

After transition the consumer writes main 0 current/destination X=`62*384`;
negative Y 255 leaves main 0's Y unchanged. It samples current coordinates/facing
and calls DeclareRaftEntity. The row's facing byte is **not applied** by this
branch, so Left is a producer operand, not an unconditional post-scroll facing.
The scroll does not unwind to MainLoop, run SwitchMap/CheckBattle, heal, reduce
STEP_COUNTER, replace CURRENT_BATTLE, rebuild population, clear temporary flags,
or invoke Map67 init. Current actors, mappings, follower lists and mode survive.
It also bypasses the no-scroll WARP_SFX submission/clear path.

`code/common/scripting/map/mapfunctions.asm:DeclareRaftEntity` returns immediately
for mode 2. Mode 1 skips current-follower relocation; other modes put every existing
EXPLORATION_ENTITIES entry at the sampled destination, changing current and
destination coordinates without rebuilding the list. For non 2 modes, F64 clear
returns; matching saved map 67 declares raft 31 at masked saved coordinates, while
a mismatch clears lookup 63 and parks raft 31 at raw `$70007000`. It does not write
saved RAFT_MAP/X/Y or PLAYER_TYPE. A current-only follower therefore survives a
scroll even if it has no fresh setup row. No newly supplied follower flag is
materialized merely because CURRENT_MAP changed.

Consequently supplied F421 does not cause an immediate Battle21 scan at this
scroll. A separate ordinary return/reload to 67 would scan row 21 and admit it
when F421 is set, even if F521 also clears that unlock afterward; stop there.
F419 belongs to 68 and cannot substitute for F421 on Map67. A fresh Map67 setup, if
independently selected, has one pointer table, an empty fixed entity list and
`ms_map67_InitFunction` (`0x4FD4A`) containing only `rts`; it uses the shared
fresh overworld/follower rules. That structural fact does not recast scrolling
as fresh setup or establish a Battle21 trajectory.

## Selected Devil's Tail zone and actor bindings

**Confirmed:** walking's `$1400` branch in
`code/common/scripting/entity/entityscriptengine_2.asm:loc_5294` writes event 6
and word X/Y parameters from destination plus candidate movement divided by 384.
This is distinct from the `$1000` warp branch, and from the Caravan controller,
which has no corresponding zone producer. The selected consumer initializes
physical 0, passes those words to RunMapSetupZoneEvent and returns. The wrapper
resolves CURRENT_MAP's setup, scans ordered rows (255 wildcard, `$FD` default),
invokes the selected callback, closes portrait/text, waits one VInt and waits
for physical 0 current position to equal its destination. It does not prove that
layout or natural movement ever submitted a selected coordinate.

`data/maps/entries/map67/mapsetups/s3_zoneevents.asm` has the complete selection:

| Rows | Supplied coordinate | Target and selected boundary |
| --- | --- | --- |
| 0 | `(57,25)` | Map67_ZoneEvent0: separate F751/F13/Elric-HP passage branch; not executed here. |
| 1–4 | `(56,26)`, `(57,26)`, `(58,24)`, `(58,25)` | Map67_ZoneEvent1: F520 set returns; clear executes cs_4FB64. No F470/F420 test. |
| Default | Other coordinates | Map67_DefaultZoneEvent passes BATTLE_DEVIL_TAIL21 to CheckRandomBattle; shared request mechanism only, no Battle21 content investigation. |

ZoneEvent0 returns when F751 set, F13 clear or Elric HP 0; its other continuation
is expressly excluded. Likewise direct warp rows to Creed/Fairy Cave are
excluded. Their existence does not make those scenes part of this slice. The
default callback's existing step/random/completion gate can produce a reload
request; it is not an immediate CheckBattle on every scroll or zone invocation.
Stop if an unrelated request takes ownership. Do not execute the excluded path
merely to manufacture a continuous route.

`sf2enums.asm` resolves Bowie 0, Peter 7, Elric 13, FOLLOWER_A=30 and FOLLOWER_B=31.
The selected call-for-help scene uses current lookups, not a membership search
or a guarantee that physical 1 is Caravan. With the fresh Map68 example above and
unchanged state across scroll, A/B/Peter/Bowie resolve to 1/3/2/0. With changed
flags after population the existing lookup remains authoritative. With F65
absent, A may have no actor; conflicting follower flags may bind A/B to someone
else. Dialogue's Oddler/Peter names express intended content, not a repair of
those supplied mappings.

## Complete call-for-help program

**Confirmed:** `cs_4FB64` at `0x4FB64`, source lines 62–102, contains 39 top-level
records. The retained map-script-engine program and direct source agree; inline
movement is included below. Text comes from `data/scripting/text/gamescript.txt`.
Each range preserves source order; B means FOLLOWER_B and A means FOLLOWER_A.

| Records | Ordered operations |
| --- | --- |
| 0–5 | Set text cursor 2543; wait B Init, A Init, Peter Init, B Jump; B single text 2543 `$09EF`, modifier 0: “Wait!” `{W1}`. |
| 6–10 | Set B destination `(60,27)` and wait; csWait 5; install B 2xRightLeft without idle wait; csWait 120; camera destination `(53,24)` and scroll wait. |
| 11–18 | Peter destination `(59,27)` and wait, face Right; Peter 2544 `$09F0` modifier `$C0` asks what is wrong with Oddler `{W1}`. Bowie destination `(60,26)` and wait, face Down; B face Up; B 2545 `$09F1` modifier 0 addresses `{LEADER}...{NAME;7}` and asks whether they can hear `{N}`/`{W1}`; Peter 2546 `$09F2` modifier `$C0` asks what `{W1}`. |
| 19–24 | csWait 5; install Peter 2xRightLeft; csWait 120; Peter face Down; Peter 2547 `$09F3` modifier `$C0` says he hears nothing `{W1}`; B inline moveDown 1, endActions, wait for idle. |
| 25–29 | csWait 5; install B 2xRightLeft; csWait 120; B nextText 2548 `$09F4` modifier 0 says somebody cries for help `{W2}`; B single 2549 `$09F5` modifier 0 locates it “over here” `{W1}`. |
| 30–38 | B destination `(57,25)` and wait, position `(63,63)`, Down; csWait 40; Peter destination `(57,25)` and wait, position `(63,63)`, Down; Bowie destination `(57,25)` and wait, position `(63,63)`, Down; request warp to Map51 `(11,22)`, Up; end. |

This program does not set/clear F520, F470, F420, F751, F13, follower flags or
PLAYER_TYPE; it does not join Elric, create him or open a passage. `(63,63)` is
a relocation, not HideEntity: mappings/list entries are retained. No scene-local
completion flag prevents another admitted F520-clear invocation.

### Immediate movement, wait, camera and text semantics

**Confirmed:** `code/common/scripting/map/mapscriptengine_1.asm` and `_2.asm`,
`sf2cutscenemacros.asm` and immediate entity consumers distinguish these commands:

- `csc15` resolves logical lookup, installs action/timer and optionally waits for
  the exact eas_Idle pointer. It has no HP/presence gate. In
  `data/scripting/entity/eas_actions.asm`, Init zeroes acceleration factors,
  sets speed 32/32, disables acceleration/deceleration, enables obstructability
  and collision, enables autofacing, orients Up, disables immersion, sets size 24,
  refreshes and jumps idle. It is not recruitment or fresh actor allocation.
- Jump disables autofacing, uses relative motion `(0,-1)`, wait 5, `(0,1)`, wait 5,
  `(0,0)`, restores autofacing and jumps idle. 2xRightLeft sets Right/Left/Right/
  Left with a sprite refresh and wait 30 after each, then jumps idle. The map
  csWait 120 is a separate sleep request, not an action-completion assertion.
- `csc29` setDest has no HP gate: it writes tile-scaled destinations, absolute
  travel and signed velocity 32 on both axes. These operands lack the high-bit
  no-wait modifier, so WaitForEntityToStopMoving polls position equality through
  VInts. It bypasses the walking terrain/collision producer. `csc19` setPos and
  `csc23` facing use AdjustScriptPointerByCharacterAliveStatus: ally 0–29 HP 0
  skips that command; A/B 30/31 bypass that HP test. Position updates current/
  destination/facing and refreshes; no action/list/membership deletion occurs.
- `csc2D` compiles each selected inline move into relative destination opcode 4,
  wait-destination opcode 1 and a final jump to idle. It clears action FLAGS_A
  bits 5/6 and waits for idle for entityActionsWait; there is no HP gate. The
  opcode 4 consumer uses current position plus signed tile displacement; its
  destination-conflict check is disabled by cleared bit 5 and it bypasses walking
  layout checks. The movement tail uses current action speeds; actual completion
  still depends on entity processing. The caller does not validate presence.
- `csc32` disables entity camera following, sets camera tile destination and waits
  for view scrolling. `csc24` restores a mapped camera entity without an HP check
  or its own scroll wait. `csWait` dispatches Sleep with the low byte; debug P2
  Start can set SKIP_CUTSCENE_TEXT and bypass these waits and text. Script entry
  clears skip state; end conditionally waits for view scrolling when dialogue
  state requires it and clears camera scroll speed.
- `textCursor` sets the dialogue index; ordinary nextText/single commands consume
  and increment it. Single additionally closes portrait/text and sleeps 10;
  nextText leaves that closure for continuation. Modifier `$C0` combines mirrored
  and right-side portrait bits. Speaker portrait/speech lookup follows the current
  mapped mapsprite. `{N}`, `{W1}` and `{W2}` remain the source text controls;
  substitutions, rendered cadence and service completion are unobserved. There
  is no YesNoPrompt or choice result in either selected scene.

`GetEntityAddressFromCharacter` masks the logical ID and indexes the current
lookup; it does not validate membership or actor presence. ClearEntities leaves
unassigned lookup entries 0, so an absent A/B/Peter/Elric can alias main 0 rather
than safely skip. A lookup value of 255 is likewise not a safe absence check here. Peter
HP 0 does not skip his action/move/text records but skips his byte-ID position/
facing records. Bowie HP 0 has analogous placement/facing effects. Earlier writes
can remain when a wait/service fails to finish; there is no transactional rollback
or automatic cast repair. These are conditional source behaviors, not observed
failure scenarios or a claim that dead characters naturally reach the scene.

## Independent pond requests and admission before init

**Confirmed:** `csc07_warp` writes pending 1, no-scroll 0, map 51, X 11, Y 22 and Up.
It does not move immediately, exit the script or write WARP_SFX. After script/
zone-wrapper return, the ordinary no-scroll consumer uses that request and the
currently supplied sound state; another pending write can replace it. Separately,
`data/maps/entries/map67/6-warp-events.asm` row 3 at `(57,25)` requests map 51
`(10,24)`, Up through WarpIfSetAtPoint, including WARP_SFX. Earlier rows at
`(26,22)`, `(51,22)` and `(57,23)` do not compete there; later X 63 border does not.
This direct warp is independent of ZoneEvent0 at the same coordinate: the
selected producer/layout kind decides which event occurs. No automatic passage
scene or prerequisite call-for-help is inferred from the shared coordinate.

Both no-scroll returns reach normal MainLoop admission. No map 51 substitution
applies. Supplied F420 selects Battle20 row 20 at either entry coordinate, with
no F470/F13/HP/membership prerequisite. F520 set clears F420 after selection but
still admits 20. Stop before BattleLoop/opening; the pond exploration init has
not run. The selected scenes/exits do not themselves set F420. This document
does not activate the later approach program to obtain it.

## Fresh pond population and shared initial prompt

**Confirmed:** with no battle selected, the shared fresh ExplorationLoop runs
as described above, now CURRENT_MAP=51/CURRENT_BATTLE=255. In `data/maps/mapsetups.asm`,
default ms_map51 (`0x5C27C`) and sole F520 alternative ms_map51_flag520 (`0x5C294`)
differ only in entity pointer. Both actual six-entry tables in
`data/maps/entries/map51/mapsetups/` share entity-event, zone, description, item
and **ms_map51_InitFunction** pointers. There is no separately selected alternate
init to infer from a filename. Pointer binding does not execute zone programs.

Default `s1_entities.asm` at `0x5C2AC` has exactly four fixed rows:

| Row / logical identity | Initial tile/facing | Mapsprite/action binding |
| --- | --- | --- |
| 0 / Elric 13 | `(6,6)`, Down | Ally 13 through GetAllyMapsprite; eas_WhirlsInWater. |
| 1 / NPC 128 | `(63,63)`, Up | HARPY 108; eas_Init. |
| 2 / NPC 129 | `(63,63)`, Up | SOULSOWER 75; eas_Init. |
| 3 / NPC 130 | `(63,63)`, Up | SOULSOWER 75; eas_Init. |

The alternate `s1_entities_520.asm` at `0x5C2CE` immediately terminates. Elric's
ally mapsprite operand uses ally lookup 13 rather than consuming NPC 128; the
shared population skips an ally row whose lookup is already nonzero, instead
of declaring a duplicate. The admitted normal follower table has no Elric row.
Thus with only F67/F68 selected, Peter/B are physical 1/2, default Elric 3 and
NPCs 4–6; alternate has none of those four fixed actors. Main 0 is separate. Neither
fixed Elric nor the three NPCs is appended to the fresh follower list, and
placing ally 13 does not set joined F13 or active membership.

Map51 is not an overworld or excluded-follower map, so fresh population uses
the normal follower table even with F65 set; no Caravan is added solely by F65.
F67/F68 and conflicting supplied follower flags keep their accepted meanings.
GetAllyMapsprite uses supplied class/HP (outside battle HP 0 selects blue flame),
not joined membership as a general admission rule. In
`data/scripting/entity/eas_battleneutralentities.asm`, WhirlsInWater enables
immersion and doubled animation speed, then cycles Right/Up/Left/Down with
refresh/wait 10 at each and loops. This is an initial action binding, not an
observed drowning animation or rescue. Mode 2 uses the same fresh raft/hidden-
follower tail; mode 1 is not normalized or reboarded. Temporary 256–383 clearing
and F80 occur before init; F420/F470/F520, F13 and follower flags survive it.

### Complete shared F470-gated prompt

**Confirmed:** `ms_map51_InitFunction` (`0x5C3CA`) tests F470 and returns if set;
clear executes `cs_5C3D8` (`0x5C3D8`), then returns. It does not test F520 or
actor existence, set F470 or set F420. With F470 set, either population skips the
scene and retains the corresponding incoming `(10,24)` or `(11,22)` main tuple,
subject to shared setup. With F470 clear, either population requests these 24
records (source lines 20–47), including both inline moves:

| Records | Ordered operations |
| --- | --- |
| 0–6 | Cursor 2550; wait B Init, Peter Init; position Bowie `(11,22)`, B `(10,22)`, Peter `(9,22)`, all Up; fadeInB calls FadeInFromBlack. |
| 7–12 | B inline moveUp 2/endActions with idle wait; csWait 5; install B 2xRightLeft; csWait 120; B face Up; B 2550 `$09F6` modifier 0 identifies the direction `{N}`/`{W1}`. |
| 13–17 | Peter inline moveUp 2/endActions with idle wait; Peter 2551 `$09F7` modifier 0 points “over there” `{W1}`; camera destination `(2,2)` and scroll wait; csWait 50; Elric 2552 `$09F8` modifier `$C0` calls “Help me!” `{W1}`. |
| 18–23 | Peter nextText 2553 `$09F9` modifier 0 identifies an elven boy stuck in the pond `{N}`/`{W2}`; Peter single 2554 `$09FA` modifier 0 says he is tired and proposes pulling him out `{N}`/`{W1}`; camera follows Bowie; Peter follows Bowie, relative 2; B follows Peter, relative 2; end. |

Under consistent live mappings/HP and completed actions, the placement/inline
operands put Bowie at `(11,22)`, B at `(10,20)` and Peter at `(9,20)` before the
new follower actions run. That is a conditional derivation, not a fixed control
snapshot: subsequent following can move them, and HP/lookup/partial execution
can change earlier results. In particular, F520-set/F470-clear still runs the
same script with no fixed Elric; its text lookup is not suppressed or repaired.
No selected prompt operation joins/rescues Elric, moves his fixed row out of the
pond, reveals the three parked NPCs, unlocks 20, sets F470/F520/F751, changes mode or
writes saved raft position. Re-entry with F470 still clear can repeat the prompt.

`csc2C_followEntity` deserves its actual operand join: it tests the **first byte
of the word-encoded follower ID** through the alive helper. Both selected IDs 7
and 31 have high byte 0, so the test is Bowie's HP, not Peter's or B's. If Bowie
HP 0, each whole follow command skips. Otherwise each maps follower and target,
reads relative 2 from table_FollowerPositions as `(-24,0)`, and calls
`entityfunctions_2.asm:AddFollower`. This installs a 42-byte copy of eas_Follower1
with physical target/offset operands, appends the follower to EXPLORATION_ENTITIES
only if absent, and writes neither force membership nor follower flags. The
`data/scripting/entity/eas_main.asm` template sets speed 32/32, zero acceleration,
disables acceleration/deceleration/obstructability, enables the map/entity
uncollidable bits, and loops follow/wait-destination.
The actual `entityscriptengine_2.asm:esc03_follow` still tests layout: it keeps
its existing destination while the maximum current-axis separation is at most
`$1E0`; otherwise it computes a facing-relative offset from the target destination,
tries a zero-lateral fallback (the same offset for these selected commands),
then the target destination when layout remains blocked.
It then sets travel/velocity using the follower speeds. The map-script follow
command does not wait for that newly installed action to settle before return. Relative 2 is not a guaranteed
world-coordinate endpoint. Missing mappings can retarget main 0; no validation
or duplicate story-state repair is added here.

After init, the shared loop conditionally submits map music/fade. Pending zero
permits WaitForEvent to follow physical 0 and install the current-mode controller:
mode 0 eas_ControlledCharacter/opcode 2, mode 1 eas_Raft/opcode 7 (actual Caravan),
mode 2 eas_Caravan/opcode 8 (actual raft); other non 0/non 1 values take the last
branch without validity repair. The prompt changes neither mode nor that policy.
A pending request takes precedence over controller install. Stop at this
conditional control boundary, before any Map51 approach/exit-warning callback.

## Reproduction and remaining limits

Load `. ./local/private-inputs.ps1` in the same process as controlled commands.
Check the pinned checkout's revision and clean status; use the unchanged
[retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
for map-init equality, `_verify_map_script_engine_fixture` and source
`_cutscene_facts` equality. These passed; they are not a new composed runtime
receipt or a fresh H1/ROM acquisition.

| Evidence owner | Selected direct reproduction and limit |
| --- | --- |
| Retained `local/derived/map-script-engine-static.json` | programCorpus.programs IDs cs_4FB64 and cs_5C3D8: compare all 39/24 commands, their sourcePath/sourceLine/macros/arguments and inline payloads to complete pinned bodies. Public summary counts alone are not a selected-program comparison. |
| Retained `local/derived/map-init-static.json` | primarySourceBodies 64/74; routeJoins 101/102/120 and pointerTableJoins 101/102/116 bind Map51 default/alternate and Map67. Direct source/pointer joins establish shared init and scroll's bypass of it. |
| `tests/fixtures/h2/map-events/zone-programs.json` | programs 122–124 bind complete Map67 selection targets. Compare 123's complete F520 branch and 124's direct shared-helper boundary; 122 is retained excluded-path context, not passage activation. |
| `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-event-request-consumption-static-v1.json` | Shared setup/population/request context; direct complete selected warp/zone/pointer/entity tables and immediate consumers establish the content and lifecycle distinctions. |
| `battle-routing-data-static-v1.json` | Aggregate routing context, not a claimed complete per-row fixture guard. Read the complete battle table and CheckBattle to establish sole map 68/67/51 rows and admission order. |

Review source with comments excluded for opcode/operand equality; resolve enums,
macros and jump-interface aliases at the actual call sites. Keep all selected
source programs, inline actions, external action targets and immediate consumers
in the review, without following excluded story continuations. Counterexamples
include corner warp precedence, F419 supplied on exit, F421 supplied on scroll
versus ordinary return, F420 supplied before pond init, all four F520/F470
combinations, mismatched/missing followers, HP 0, mode 0/1/2, stale saved raft map,
debug text skip and replaced pending requests. These constrain static claims;
they are not executed scenarios or a natural travel proof.

**Unknown:** natural terrain/access and route, actual flags/HP/membership/mappings,
transport/raft state, zone-versus-warp producer selection, completed action/text/
fade/camera services, prompt presentation, responsive control and persistence.
No new runtime is admitted for these gaps under
[ADR0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../decisions/0016-remake-start-evidence-deferral.md).

Preserve [PR862's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/860#issuecomment-6162442630)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/862#issuecomment-6162494122),
including their retained pair, original/rebased results, corrected inspections
and linked PR857/854/849/846/842 failures. Lookup misses and truncated reads are
not absence evidence; locate and reread actual owners. This Issue/PR handoff
records current inspection corrections, exact Git/check/CI/process identities.
Acceptance is complete owned-prose/source/provenance/link/private review,
`git diff --check`, clean committed configured
`uv run sf2 verify plan --base origin/main --head HEAD` and actual final-head CI.
No blanket local normal/full/engine/Godot/H3/H4 suite is selected solely for prose.
Leave a clean frozen Draft PR for independent main-gate review; no cleanup.
