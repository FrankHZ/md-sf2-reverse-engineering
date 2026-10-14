# Harpy Pond departure through Fairy Cave entry

Evidence date: 2026-10-14. **Confirmed:** the pond's external exit requests
Devil's Tail at `(57,26)`, Down. A separately submitted zone event at `(57,25)`
can run Elric's passage scene when F751 is clear, F13 is set and Elric has
nonzero HP. The scene changes live map blocks, hides its newly declared Elric
and requests Fairy Cave `(29,3)`, Left; F751 is set only after script return.
Map32 has no battle row or map-specific setup: the actual setup fallback is
`ms_Void`, with shared fresh population and conditional controller handoff.
**Inferred:** the dialogue and block change depict Elric opening the passage.
**Unknown:** natural traversal, supplied state, completed services, rendered
presentation, usable input and persistence. These are conditional source
boundaries, not a proven continuous post-battle journey.

## Agreement and provenance

Standard rigor under [Issue #876](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/876)
and [content epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
one bounded static account for the content guide, with no new verification
machinery or runtime acquisition. Accepted starting base is
`4f9414ded2be0caa73210b310df99a8aedaa47a8`. This document is the sole owned path;
there are no shared edits or stacked dependencies. Stop at conditional cave
entry control, an unrelated pending continuation, or Battle21 admission before
its opening. Exclude cave exploration, chest/search/interaction content, later
traversal, Creed content and battle opening/combat/outcomes. The exit-coordinate
comparison below exists only to distinguish arrival from a new exit producer.
Material coupling beyond this boundary requires replanning.

Consume accepted [Battle20 outcomes and Elric membership](./battle20-outcomes-and-elric-recruitment.md),
[pond entry/population and the complete F520 call-for-help scene](./elven-village-departure-through-harpy-pond-entry.md),
[pond approach and exit warning](./harpy-pond-approach-through-battle20-entry.md),
[request writes](./map-event-request-state.md),
[request consumption](./map-event-request-consumption.md),
[random-battle request structure](./map-event-random-battle-state.md) and
[transport/controller selection](./polca-raft-boarding-through-south-parmecia-entry.md).

All source paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The assigned read-only source is clean at that pin. Retained payloads bind that
revision and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Short map paths begin at `data/maps/entries/`; entity consumer filenames begin
at `code/common/scripting/entity/`, map-script/setup/follower consumers at
`code/common/scripting/map/`, and exploration consumers at
`code/gameflow/exploration/`. Fnnn means flag nnn; rows and command indices
are zero-based. Positions, waits and sound IDs are operands, not measured
locations, elapsed durations or audible results.

Reuse the unchanged 18,285,979-byte map-init/map-script-engine payload pair and
bounded source/fixture reads. Linear comparisons were budgeted below 512 MiB
and one minute, using the existing environment, with only small receipts/prose
and no regenerated payloads or game-material publication. They passed in
0.081 seconds. Reassess before material resource or scope growth.

## Pond exit and conditional fresh Devil's Tail entry

**Confirmed:** the complete `map51/6-warp-events.asm` has one row: any X, Y25,
no-scroll, Map67 `(57,26)`, Down. The separate zone rows `(9,24)` and `(10,24)`
run the accepted F470-clear exit warning; F470 set bypasses it. The Y25 warp
row itself tests neither F470 nor F520. A warning at Y24 is not a condition
encoded in the external exit row, and this does not establish that walking
naturally reaches Y25 while the warning is active.

`code/gameflow/exploration/exploration.asm:WarpIfSetAtPoint` divides supplied
internal coordinates by 384, scans the external warp table in order, uses
negative-byte coordinate wildcards, and writes event 1, parameters 1–6 and
WARP_SFX. Ordinary walking reaches it through its movement checks and the
working-layout `$1000` branch of `entityscriptengine_2.asm`; `$1400` instead
submits zone event 6 with word coordinates. Vehicle controllers have their own
producer conditions. No table match alone proves layout access or input reach.

`explorationfunctions_2.asm:ProcessMapEvent` clears pending before dispatch.
The no-scroll type 1 consumer submits and clears WARP_SFX, idles physical 0,
unwinds to MainLoop, samples the player's current coordinates and substitutes
nonnegative requested X/Y. Here it returns `(67,57,26,Down,d4=0)` without
normalizing PLAYER_TYPE. `code/gameflow/mainloop.asm` then runs SwitchMap,
CheckBattle and only then fresh exploration if no battle was selected.

The complete `data/maps/global/flagswitchedmaps.asm`, consumed by
`code/common/maps/mapinit_0.asm:SwitchMap`, has no Map67 or Map32 substitution.
The complete 45-row `data/battles/global/battlemapcoords.asm` has one Map67 row:
slot 21, area `(21,0)`, size 32x42, trigger X/Y both 255. In
`code/common/maps/getbattle.asm:CheckBattle`, supplied F421 admits Battle21
regardless of area containment; `(57,26)` being outside that rectangle's X
extent does not reject it. F521 set clears F421 after selection but does not
cancel this admission. Stop before the opening. F421 clear allows exploration;
neither the exit row nor passage scene manufactures F421/F521.

For the fresh supplied-map branch, ExplorationLoop clears pending, subtracts
20000 from STEP_COUNTER with signed-word negative-result clamp to 0, heals
eligible living/immortal allies, fades, sets CURRENT_MAP=67/CURRENT_BATTLE=255,
loads tilesets and rebuilds entities. It loads mapsprites, clears temporary
flags 256–383, sets F80, loads map/view, installs base VInts and invokes map init.
F13/F470/F520/F751 and follower selectors are outside the temporary reset.

`data/maps/mapsetups.asm` selects Map67's sole pointer table with no flag
alternate. `map67/mapsetups/s1_entities.asm` is empty, and
`s6_initfunction.asm:ms_map67_InitFunction` at `0x4FD4A` is just `rts`.
The actual `j_GetMapSetupEntities` alias targets GetMapSetupEntityList, which
uses GetCurrentMapSetup. `mapfunctions.asm:InitializeMapEntities` still clears
old entities/lookups, rebuilds followers, declares Bowie physical 0 at the
requested tuple, terminates the follower list and runs the raft population tail.
An empty fixed list does not mean an empty map population.

The full `data/scripting/entity/followers.asm` and
`followersfunctions_1.asm:InitializeFollowerEntities` select by flags, not
membership or HP. Map67 is in `data/maps/global/overworldmaps.asm`: with only
F65/F67/F68 among follower selectors, physical 1/2/3 are Caravan/A, Peter and
Oddler/B. F66 adds Sarah and Chester; other supplied rows retain table order.
F74 declares Astral as B before F68's Oddler row; F70 can overwrite B again.
F74/F77/F83 can overwrite A's lookup.
The complete table has no Elric row. Active or reserve F13 membership therefore
does not declare him here, and the prior pond scene's temporary actor/following
is not retained by this fresh population.

PLAYER_TYPE and saved raft state remain supplied. Fresh mode 2 puts the raft
sprite on physical 0, parks current followers at `(64,64)` with standing actions
and declares separate raft physical 31 there. Other modes declare that raft
only for F64 and matching saved RAFT_MAP=67, at masked saved coordinates. Fresh
mode 1 does not repeat boarding. The population tail is
`followersfunctions_2.asm:sub_44404`; it neither repairs arbitrary modes nor
changes the saved raft location. Conditional music/fade and pending-zero
WaitForEvent may then install the current-mode controller. This arrival tuple
does not invoke the zone callback at `(57,26)` merely by sharing its coordinates.

## Separate Map67 zone selection and passage admission

**Confirmed:** the complete `map67/mapsetups/s3_zoneevents.asm` orders:

| Rows | Supplied zone coordinate | Callback and branch |
| --- | --- | --- |
| 0 | `(57,25)` | Map67_ZoneEvent0: F751 set returns; otherwise F13 clear returns; otherwise Elric HP 0 returns; else execute cs_4FC32, set F751 after return, then return. |
| 1–4 | `(56,26)`, `(57,26)`, `(58,24)`, `(58,25)` | Map67_ZoneEvent1: F520 set returns; clear executes the complete accepted cs_4FB64 call-for-help scene. |
| Default | Other coordinates | Map67_DefaultZoneEvent calls CheckRandomBattle with Battle21. Retain the accepted step/random/completion/request boundary; stop on an unrelated continuation. |

ZoneEvent0 calls `j_GetCurrentHp` with ally 13 and tests the returned word for
zero. It does not test active-party F45, F520, F470, a fairy conversation, class,
transport mode, another follower or terrain. Joined reserve Elric is eligible;
missing F13 and HP 0 are not repaired. Supplied F751 bypasses every later test
and does not replay or immediately reapply the scene's block command.

The producer remains walking's separate `$1400` event 6 path. The type 6 consumer
applies Init to physical 0, reads the two word coordinates and calls
`mapsetupsfunctions_1.asm:RunMapSetupZoneEvent`. That wrapper obtains the current
setup, matches ordered coordinate rows or `$FD` default, calls the callback,
closes portrait/text, waits one VInt and waits for physical 0 current position to
equal its destination. This wrapper is significant after script return; neither
the submitted tile nor init proves Bowie's exact starting position in the scene.

The F520-clear branch is the already accepted complete program, including its
current A/B/Peter lookups, text 2543–2549, movements/relocations and final Map51
request. F520 controls that different callback only. F520-clear/F13-set/nonzero-HP
is not excluded by the passage predicate, and text thanking a fairy creates no
additional prerequisite. No natural reach claim is made for supplied combinations.

## Complete selected passage program

**Confirmed:** cs_4FC32 starts at `0x4FC32`, source lines 103–143. All 33 retained
top-level records agree with source macros/arguments/lines; four inline action
bodies are also read in full. B below is FOLLOWER_B=31; Bowie=0, Peter=7,
Elric=13 from `sf2enums.asm`.

| Records | Ordered operations |
| --- | --- |
| 0–3 | Set text cursor 2593; newEntity 13 at `(57,25)`, Up, mapsprite operand 255; install/wait Elric Init; Elric inline Up 1, endActions, wait for idle. |
| 4–7 | Face Bowie, Peter, B Up, in that order; Elric single text 2593 with modifier 0. |
| 8–14 | Wait 30; sound SECRET_PATH_ROCK (96); wait 30; face Elric Down; install/wait Elric Jump twice separately; Elric single text 2594, modifier 0. |
| 15–21 | Set quake 1; sound BIG_DOOR_RUMBLE (93); wait 20; face Elric Up; wait 20; sound DESOUL_HOVERING (110); copy blocks from `(0,0)`, size 3x2, to `(56,22)`. |
| 22–26 | Wait 30; set quake 0; wait 30; face Elric Down; Elric single text 2595, modifier 0. |
| 27–32 | Elric inline Up 1/endActions with idle wait; hide 13; Bowie inline Up 1/endActions with idle wait; Bowie inline Up 1/endActions **without** idle wait; request Map32 `(29,3)`, Left; end. |

`data/scripting/text/gamescript.txt` supplies these actual literal text joins;
the content is paraphrased rather than reproducing the transcript:

| Decimal / hex | Attribution and controls |
| --- | --- |
| 2593 / `0A21` | Elric says a fairy told him how to move the rock; one `{N}`, then `{W1}`. |
| 2594 / `0A22` | Elric declares success and thanks the fairy; `{W1}`. |
| 2595 / `0A23` | Elric calls the leader onward; `{LEADER}`, `{W1}`. |

All three use the current mapped Elric's portrait/speech resolver. Modifier 0
does not set mirrored/right bits. `{LEADER}` resolves the active-force leader;
it is not an unconditional Bowie literal. There is no choice/refusal, join,
heal, class/status reset or persistent follower-selection command. The dialogue
does not establish an earlier fairy interaction or natural history.

### Allocation, actor mappings and service limits

**Confirmed:** `mapscriptengine_1.asm:csc2B_initializeNewEntity` reads the ally
word and four bytes, supplies eas_Init, then calls
`entityfunctions_1.asm:InitializeNewEntity`. For ally 13 it replaces the sprite
operand 255 through GetAllyMapsprite. That helper uses the current HP/class and
`data/stats/allies/allymapsprites.asm` row 13=ELRIC_SPECIAL (32): ordinary ACHR (5)
selects 30, SNIP (21) selects 31, BRGN (22) selects 32. In exploration HP 0 would select
blue flame, but this callback's earlier nonzero-HP check normally excludes it.
No class or HP write accompanies allocation.

InitializeNewEntity scans lookup bytes 0–62 using signed byte comparison from
zero, chooses maximum+1, and overwrites lookup 13 with that physical index.
It does not check whether Elric already exists, reuse his old actor, check party
membership or validate capacity. Lookup 63's raft is outside that scan; negative
byte values do not raise the maximum. A supplied duplicate can leave an old
physical actor present while later commands address the new mapping. This is
distinct from the fixed-population duplicate check in InitializeMapEntities.
DeclareNewEntity writes current/destination coordinates at tile×384, facing,
sprite, layer/action, zero motion/acceleration and initial flags; the caller
updates properties. It does not add Elric to EXPLORATION_ENTITIES or set F13/F45.

In the minimal fresh F65/F67/F68 example, the newly allocated Elric is physical 4;
Bowie/Peter/B resolve to 0/2/3. With F65 absent and only F67/F68, Elric is physical 3
and Peter/B are 1/2. These are conditional examples, not fixed actor numbers.
Supplied extra/conflicting flags, old scroll-preserved population or invalid
lookups can change them. GetEntityAddressFromCharacter follows the current
lookup; a cleared 0 can alias physical 0, and 255 is not validated as safe absence.
Naming B in the script does not prove that its actor is Oddler.

Immediate consumers retain the following boundaries:

- csc15 installs the selected action/timer and polls for exactly eas_Idle when
  requested. `data/scripting/entity/eas_actions.asm:eas_Init` sets speed 32/32,
  zero acceleration factors, disables acceleration/deceleration, enables
  obstructability/collision/autofacing, orients Up, disables immersion, sets
  size 24, refreshes and idles. Elric's explicit Init wait precedes his first move.
- csc23 facing uses AdjustScriptPointerByCharacterAliveStatus: HP 0 allies 0–29
  skip the operation; B (31) bypasses it. Thus Bowie/Peter facing depends on their
  own HP and mappings. Init, actions, text and hide do not inherit this HP gate.
- csc2D clears action flags 5/6 and compiles each Up 1 into relative-destination
  opcode 4, destination wait and a final eas_Idle jump. `esc04_moveToRelativeDest`
  takes the actor's current position minus 384 Y; with flag 5 cleared it bypasses
  the same-destination scan. It does not run walking's warp/zone producers.
  With consistent actors and a completed move, Elric first reaches `(57,24)`.
  The later Up 1 reaches `(57,23)` only if the intervening actions leave him at
  `(57,24)` and that move completes before hide. Those coordinates alone do
  not invoke the external warp table. Bowie is not explicitly positioned here;
  his two Up 1 commands are relative to his supplied/current actor position.
- eas_Jump disables autofacing, requests relative `(0,-1)`, waits 5, requests
  `(0,1)`, waits 5, requests `(0,0)`, restores autofacing and jumps to idle.
  These are timed destination requests through the same opcode 4 consumer,
  not three destination barriers or proof of returning to an exact start tile.
  The script waits each Jump's idle pointer, not an observed trajectory.
- csc00 waits for view scroll, resolves current portrait/speech, displays the
  text, increments the cursor, closes windows and sleeps 10. ExecuteMapScript
  clears debug skip at entry; debug plus player 2 Start can set it, skipping
  these displays/cursor increments and csWait sleeps. Action waits remain.
- csc05 submits each sound command without a sound-completion barrier. csc33
  with operands 1/0 directly writes QUAKE_AMPLITUDE, without the gradual-amount
  branches. `exploration.asm:FinalizeScrollDataUpdate` consumes nonzero amplitude
  for scroll-data jitter; the writes do not prove rendered shake or audio.

`csc2E_hideEntity` resolves the mapped physical Elric and calls HideEntity. It
parks current/destination at raw `$70007000`, writes 255 into every lookup alias
to that physical entity, and removes it from the follower list if present.
It does not clear F13, remove a force member, heal him or delete a distinct old
duplicate. His new actor was not automatically added to the follower list.

## Live block change, completion flag and request return

**Confirmed:** csc34 reads the source/size/destination byte pairs and calls
`exploration.asm:CopyMapBlocks`. It copies six layout words from the 3x2 rectangle
at `(0,0)` into `(56..58,22..23)` in the live 64-wide RAM layout (row stride 128
bytes), including the words' flags, then requests both view-plane updates.
It writes no F751 and does not independently prove passability or presentation.

`map67/3-flag-events.asm` contains one flag-block row with the same source,
size and destination, gated by F751. `code/common/maps/mapload.asm:LoadMapBlocksAndLayout`
loads base blocks/layout, scans those flag rows and calls CopyMapBlocks for set
flags. This is how a later load reapplies the change. Map67 setup selection and
its return-only init are separate. Supplying F751 after a map is already loaded
does not itself execute the flag-block consumer; the skipped zone is not a
repair path for the current live layout.

`mapscriptengine_2.asm:csc07_warp` writes event 1 and parameters 1–5 as
`(no-scroll=0,map=32,X=29,Y=3,facing=Left)`, then returns to the script. It does not dispatch
the map change, set WARP_SFX or rewrite parameter 6. At csc_end, ExecuteMapScript
conditionally waits for view scrolling if the dialogue-window index is nonzero,
clears camera scrolling speed and restores registers. It is not an all-actor
barrier. Only then does Map67_ZoneEvent0 set F751 and return.

The zone wrapper next closes windows, waits one VInt and waits for **physical 0's
position/destination equality**, not Bowie's final action pointer or every
actor. The last Bowie action is asynchronous; the request is already published
before this wrapper finishes. The one-VInt service plus equality check must not
be promoted to a universal assertion that both upward tile moves completed,
especially with supplied mappings or unfinished services. A stalled wrapper
can leave F751 set and the request pending without reaching cave consumption.

If the retained request is still pending when exploration resumes, WaitForEvent
returns it without reinstalling the controller. ProcessMapEvent clears pending
and the no-scroll consumer submits/clears the **supplied** WARP_SFX, idles 0,
unwinds and returns `(32,29,3,Left,d4=0)`. It uses the explicit requested X/Y
regardless of how far Bowie's last move got. If a later producer replaced the
single pending slot, the consumer follows that actual request instead. There
is no request queue, transactional rollback or guarantee that publishing the
warp means it has been consumed.

| Supplied or partial state | Bounded consequence |
| --- | --- |
| F751 set at zone entry | No scene, spawn, block copy or cave request; current F13/HP remain untouched. A later map load independently reapplies the flag blocks. |
| F751 clear; F13 clear or Elric HP 0 | Return with no scene or repair. Battle20 completion alone cannot satisfy these tests. |
| F751 clear; joined, living Elric, including reserve | Scene admitted without F520 or a fairy-history check. Later service completion remains conditional. |
| Service stops before block command | Earlier actor/dialogue/action changes may exist; block change, final request and F751 are not established. |
| Block command completed, script not returned | Live change can exist with F751 clear; later quake reset, hide and request depend on where execution stopped. No rollback is present. |
| Script returned, wrapper or later service unfinished | F751 can be set before cave load/control; final motion, pending-slot retention and consumption remain separate. |
| Repeated/supplied duplicate actor or missing B/Peter | Allocation/lookup semantics still apply; successful passage admission does not normalize the entire cast. |

## Independent external cave requests and Map32 entry

**Confirmed:** the complete `map67/6-warp-events.asm` orders Creed `(26,22)`,
cave `(51,22)`, cave `(57,23)`, pond `(57,25)`, then east-border scroll. Only the
two selected cave rows are followed here:

| External row | Request tuple |
| --- | --- |
| 1: `(51,22)` | no-scroll, Map32 `(2,3)`, Right |
| 2: `(57,23)` | no-scroll, Map32 `(29,3)`, Left |

WarpIfSetAtPoint tests their exact X/Y and ordered match, with no F751/F13/HP/F520
test. Its caller's movement/layout conditions still apply. The row at `(57,23)`
shares the scene's destination tuple but is a separate producer; the opposite
entrance establishes no westward route. At `(57,25)` the external table has a
pond warp while the zone table has the passage callback. The working-layout
producer branch distinguishes them; the coordinates do not invoke both.

For either selected external request or the retained script request, ordinary
MainLoop SwitchMap leaves 32 unchanged, and CheckBattle returns no battle: no
Map32 row exists anywhere in the complete battle table, even with supplied
F421. This is not admission to Battle21 through the cave.

Inventory showed Map32's ordinary map-data files but no mapsetups directory.
The decisive evidence is the complete `data/maps/mapsetups.asm`: it skips 31→33
and contains no later 32 row. `mapsetupsfunctions_1.asm:GetCurrentMapSetup` scans
to the table terminator and returns `ms_Void: dc.w $FFFF`. GetMapSetupEntityList
returns that sentinel unchanged; InitializeMapEntities sees its first byte 255
and skips fixed rows, while still rebuilding shared followers and Bowie.
RunMapSetupInitFunction sees the same word and returns without a callback.
This resolves the preflight's missing guessed Map32 setup paths through the
actual table/consumer; filename absence alone was not used as semantic evidence.

Fresh ExplorationLoop performs the same pending clear, step adjustment, eligible
healing, fade/tileset/population/mapsprite/temp-flag/F80/map-load sequence described
above, now with CURRENT_MAP=32 and CURRENT_BATTLE=255. The script's Elric actor
and any previous map-only following are discarded. F13/F45 and F751 persist
across this temporary-flag reset; Map32's flag-block list is empty. No map-specific
init story is substituted for ms_Void.

Map32 is absent from the complete overworld-map list, so F65 does not prepend
the Caravan follower. Other selected follower rows remain in force, including
Peter/F67 and B/Oddler/F68, and their possible lookup overwrites. With only those
two flags, Peter/B become physical 1/2; there is still no Elric follower row.
The raft tail uses the supplied mode/F64/saved RAFT_MAP=32 exactly as above.
Neither the scene nor cave setup boards, disembarks or repairs mode 1/2. The
requested `(29,3)`, Left initializes Bowie physical 0; the opposite entrance
instead supplies `(2,3)`, Right.

After the return-only init fallback, exploration conditionally submits map
music/fade. Pending 0 lets WaitForEvent follow physical 0 and call
`entityfunctions_2.asm:SetControlledEntityActScript`: mode 0 selects walking,
mode 1 selects eas_Raft (actual Caravan control), other modes select eas_Caravan
(actual raft control). This conditional installation is the entry-control
boundary, not an observation of responsive input or a guarantee for arbitrary
supplied modes. An unrelated pending request takes precedence.

The complete `map32/6-warp-events.asm` contains exit coordinates `(2,3)` and
`(29,3)`, requesting Map67 `(51,23)` and `(57,24)`, both Down. Arrival at these
tiles does **not** itself call WarpIfSetAtPoint: fresh population/init and
WaitForEvent are not external-warp producers. Walking's control routine first
requires an admitted directional movement candidate and the relevant layout
branch before its table lookup; movement without such a candidate skips that
path. Vehicle producers likewise remain separate. No immediate bounce is
proved by the coordinate overlap. Stop here, before any later cave movement,
exit selection or content.

## Reproduction and remaining limits

Load `. ./local/private-inputs.ps1` in the same process as each controlled tool
launch and verify the assigned source pin/clean status. Reuse the unchanged
[retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init equality, `_verify_map_script_engine_fixture` and source `_cutscene_facts`
equality all pass. No retained payload or fixture was rewritten.

| Evidence owner | Direct review needed beyond retained equality |
| --- | --- |
| `tests/fixtures/h2/map-script-engine-static-v1.json`, retained `local/derived/map-script-engine-static.json` | Select programCorpus.programs id=cs_4FC32; compare all 33 source-line/macro/argument records and all four Up 1/endActions bodies. Read text 2593–2595 and resolve macros/enums and selected immediate command/action consumers. |
| `tests/fixtures/h2/map-init-static-v1.json`, retained `local/derived/map-init-static.json` | Complete MapSetups and ms_Void consumers; actual Map67 pointers/empty fixed list/init; full follower/overworld tables; fresh population, raft and controller branches. |
| Accepted request/admission owners and their fixtures | Map51 external row versus warning; complete Map67 zone/external tables and selected predicates; csc07, script return, wrapper barrier, pending replacement, no-scroll return, complete switch/battle tables and Map32 exit producer distinction. |
| Selected direct source | InitializeNewEntity/GetAllyMapsprite/DeclareNewEntity/lookups/HideEntity; csc34/CopyMapBlocks; F751 flag-block row and LoadMapBlocksAndLayout, separate from post-return F751. |

Use the exact paths, labels and slots above, resolve actual jump aliases, strip
comments for instruction comparison and read the complete selected programs.
Retained equality does not automatically guard every prose claim; direct
source/consumer review supplies the selected joins. No new original-game
runtime observation, terrain decoding or verification infrastructure is claimed.

**Unknown:** natural route, supplied flags/membership/HP/class/lookups/mode/raft,
completed movement/text/sound/fade/scroll/input services, rendered rock/passage,
usable control and persistence. The independent flags, reserve/dead/missing
actors, duplicate allocation, debug skip, partial return and overwritten pending
slot are static discriminating cases, not executed scenarios. These Unknowns
do not automatically authorize runtime work under
[ADR0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../decisions/0016-remake-start-evidence-deferral.md).

Preserve [Issue872's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/872#issuecomment-6171350805)
and [PR874 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/874#issuecomment-6171383624),
including their retained comparisons and linked prior failures. Current exact
Git identities, inspection failures/corrections, direct checks, actual final CI
and process state belong in Issue876's handoff. Acceptance is complete selected
source/prose/provenance/link/private review, whitespace checks, clean committed
configured `uv run sf2 verify plan --base origin/main --head HEAD` and final-head
CI. No blanket local normal/full/engine/Godot/H3/H4 suite is selected solely
for this prose. Freeze a clean pushed Draft PR for independent integration;
no merge, cleanup or successor selection is authorized here.
