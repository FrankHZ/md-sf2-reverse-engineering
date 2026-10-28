# Battle09 outcomes through Ribble's welcome

Evidence date: 2026-10-13. **Confirmed:** Battle09 victory returns to Map69 at a
battle-derived position; neither end/after script sends the party to Ribble. A separate
Map69 warp connects to Ribble, **Map8**, at `(15,30)`, Up. Its two welcome zones have
different gates: the initial accusation checks F708/F258, while the mayor checks only
F708. The mayor script explicitly rebuilds residents, copies four door blocks, sets
F709, restores any saved roof rectangle and reinstalls a current Peter-follow relation;
only after it returns does its caller set F708. **Inferred:** these are the next
narrative welcome scenes after the encounter named `BATTLE_TO_RIBBLE`. **Unknown:**
natural traversal, actual actors, service completion and a playable continuous route.

## Agreement and provenance

This is the Standard-rigor agreement for
[Issue #756](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/756), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Separate outcome, request, setup and asynchronous entity consumers require explicit
boundaries. Sole tracked owner: this document. Accepted starting base:
`343c1e03d92841e592f857baf33cc35baaf16fa0`; no stacked dependency or shared-path write.
Continue the accepted [Battle09 admission](./new-granseal-departure-through-battle09.md)
at its outcomes; stop after the mayor caller and first restored exploration control.
Combat simulation/rosters, May's recruitment body, town-wide NPC/shop/treasure content,
shrine/raft travel, Battle10/11, translation, design, engine/UI and runtime acquisition
are excluded. This is original-game research, not a remake capability claim.

Source paths below are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM`, pinned `master` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained `map-init` and
`map-script-engine` payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Their fixture comparisons and the existing cutscene-source comparison pass. No new
extraction or runtime observation is claimed. Fnnn means flag nnn; battle rows are
zero-based source IDs. Map8 setup paths are under `data/maps/entries/map08/mapsetups/`.
Script record numbers below are one-based indices into retained
`programCorpus.programs[].commands`, including sleeps and terminators; nested entity
actions are not additional map-script records.

Acceptance is direct pinned source/row/handler/list/fixture review, retained comparisons,
document/link/private/diff checks, clean committed planner and actual final-head CI.
No parser, fixture or comparison platform is needed. The two retained payloads total
18,285,979 bytes: one process, linear selected reads/comparisons, estimated below 512 MiB
and a one-minute comparison budget. Outputs are prose and small ignored review receipts.
Reassess before expanding routes, data or semantics; Unknowns are not a capture queue.

## Battle09 outcome and first exploration return

**Confirmed:** `code/gameflow/battle/battleloop_1.asm` checks deaths/counts after an
action and again after after-turn effects. `battleloop/countremainingcombatants.asm`
counts each faction's nonnegative-X, nonzero-HP combatants and forces the ally count to
zero when Bowie's HP is zero. Defeat is tested before enemy-count-zero victory.
Battle09 therefore requires remaining counted allies and no counted enemies, not just
the death of enemy slot 128.

`data/battles/global/enemyleaderpresence.asm` row 9 is zero. The defeated wrapper in
`code/gameflow/battle/cutscenes/battleendcutscenesstart.asm` requires Bowie alive,
slot 128 dead and F509 clear before dispatching row 9 of
`data/battles/cutscenes/battleendcutscenes.asm`: `ms_Empty`. Its shared end tail reads
the zero leader-presence row and does not append all remaining enemies to the death
list. F509 set skips that script and still reaches the zero-row check. These are wrapper
effects, not a special Battle09 winning condition.

| Outcome | Ordered state and returned destination |
| --- | --- |
| Victory | `BattleLoop_Victory` in `battleloop_2.asm` heals living/immortal allies, seeds parameter 2 from `CURRENT_MAP`, gets the first battle-party member after `UpdateForce`, adds battle-area X/Y to that member's combatant coordinates, and takes facing from its mapped entity. Parameter 1 becomes zero. It calls the after wrapper, clears F409, sets F509, then returns parameters 2–5 as map/X/Y/facing with `D4=1`. |
| Defeat | `BattleLoop_Defeat` calls `UpdateBattleUnlockedFlag`, presents text 363, restores Bowie's HP to max, halves gold by unsigned right shift, obtains the egress tuple and returns `D4=-1`. Battle4's special upgrade branch does not apply to 9. F509 is not newly set. |
| Egress / Angel Wing | `battlefunctions/battlefunctions_1.asm` consumes the spell MP or item respectively, invokes flash/text services, calls `UpdateBattleUnlockedFlag`, unwinds the action frame, gets the egress tuple and returns `D4=0`. This path does not apply defeat's HP restoration/gold halving or victory's completion write. |

The after wrapper (`afterbattlecutscenesstart.asm`) tests F509. Clear dispatches
`data/battles/cutscenes/afterbattlecutscenes.asm` row 9, also `ms_Empty`; set skips it.
There is no Battle09 `cs_afterbattle.asm` to infer or recreate. The empty interpreter
terminator can still wait for view scrolling when a dialogue window exists and clears
scroll speed; it does not write a new map tuple. In both branches the wrapper tail in
`afterbattlecutscenesend.asm` calls `JoinForce` with row 9 of
`data/battles/cutscenes/afterbattlejoins.asm`, value 0 (Bowie), included by
`layout/sf2-07-0x044000-0x064000.asm`. The duplicate global-path table agrees. Despite
the table's unused-feature comment, this call executes: it sets
F0, updates force and conditionally sets F32 if capacity permits. It neither joins May
nor changes Peter's membership. The [battle lifecycle](./battle-loop.md),
[cutscene owner](./battle-cutscenes.md) and `code/common/stats/battleparty.asm` own these
shared behaviors; dead non-immortal allies are not resurrected by victory healing.

Battle09's area origin is `(0,0)` on Map69, so ordinary victory returns that map with
the chosen member's battle coordinates and facing, not a fixed town entry. The chosen
member is not hardcoded as Bowie. The [predecessor](./new-granseal-departure-through-battle09.md)
owns the row and placement boundary; no roster or final member position is assumed here.
`UpdateBattleUnlockedFlag` clears F409 only when F509 was already set (repeat battle).
On first-battle defeat/egress, F409 remains as supplied and F509 remains clear.

`battleloop/getegresspositionforbattle.asm` has no Battle09 override. It selects the
carried `EGRESS_MAP`, then `code/common/maps/egressinit.asm:GetSavepointForMap` supplies
that map's saved-point row, or `(1,1)`, Up on no match. Battle entry already sets F399,
so the pre-first-battle bedroom fallback is not the normal Battle09 lineage. F64 may
also cause the shared raft-reset side effect; that does not make Ribble the destination.
No particular last savepoint is established by this slice.

`code/gameflow/mainloop.asm:alt_MainLoopEntry` calls `SwitchMap` after battle and then
enters exploration **without another `CheckBattle` on that immediate return**. Thus a
carried F417 does not immediately intercept victory's return. Map69 has no switched-map
replacement. Supplied-map `ExplorationLoop` clears pending, adjusts the step counter,
heals living/immortal allies, sets current map/not-in-battle, rebuilds entities/sprites,
clears temporary flags 256–383, sets F80, loads the map and calls init. Map69's default
fixed list is empty and its init is `rts`; flag-selected followers can still exist.
After services, pending zero allows `WaitForEvent` to install control. BattleLoop's
`PLAYER_TYPE=0` supplies ordinary character control if no intervening writer changes it.
The returned D4 distinguishes outcomes but does not force a town destination.

## Conditional Map69 connection to Ribble

**Confirmed:** Map69 `s3_zoneevents.asm` distinguishes specific X23/24/25, wildcard Y,
from its default. With F509 set the default returns without requesting Battle09; it
does not clear a pre-existing F409. The specific handler still calls the Battle17
random helper, with its own F517/step/RNG rules. At a later Map69 admission,
`CheckBattle` prioritizes row 9 (F409) over row 17 (F417); completion clears an already
selected unlock but does not skip that row. These controls remain with the
[admission owner](./new-granseal-departure-through-battle09.md#map69-zone-partial-reload-and-battle09-selection)
and [random-battle owner](./map-event-random-battle-state.md). No Battle17 content audit
is implied. F509-set default behavior is not proof of an uninterrupted walk.

`data/maps/entries/map69/6-warp-events.asm` contains the selected exact row `(44,7)`:
no scroll, `MAP_RIBBLE=8`, `(15,30)`, Up (`sf2enums.asm`). Ordinary walking must first
find masked layout attribute `$1000` at the target; `WarpIfSetAtPoint` then matches the
row and writes type 1, its complete parameter tuple and warp SFX. The no-scroll consumer
in `explorationfunctions_2.asm` plays/clears the SFX, idles main, removes the exploration
return frame and returns the tuple to `MainLoop`. See the
[request-consumption owner](./map-event-request-consumption.md) for the mutable pending
slot, signed-coordinate overrides and caller lifetime. Here both destination bytes are
explicit positive values. Surviving request state is required; a table connection is
neither collision/path evidence nor a forced victory destination.

Map8 has no flag-switched replacement or battle-map row. This selected request therefore
takes ordinary supplied-map entry: main starts `(15,30)`, Up; temporary F258 is cleared
before init; living/immortal healing, followers, layout/flag copies and init run before
the pending/control boundary. The init itself requests no welcome script or warp.
Neither the warp row nor either welcome handler checks F509, so a first victory is not
an encoded prerequisite for a supplied Ribble visit. Physical access remains Unknown.

## Map8 setup, actors and independent flag lifetimes

**Confirmed:** `data/maps/mapsetups.asm` gives Map8 default `ms_map8` and one override,
F708 → `ms_map8_flag708`. `GetCurrentMapSetup` in
`code/common/scripting/map/mapsetupsfunctions_1.asm` evaluates the flags on each lookup.
The pointer tables share zone, description, item and init targets but select different
entity lists and entity-event targets. The `_518` entity-event filename is not an F518
setup gate; only the actual F708 row selects that pointer table. Dialogue bodies there
are outside scope.

| Population source | Bounded actor mapping |
| --- | --- |
| Default `s1_entities.asm` | Two fixed rows: event character 128, old man at `(14,7)`, Down; 129, young man at `(13,26)`, Right. Both use `eas_Init`. In the selected scenes they are the mayor and the accuser, as supported by the dialogue context. |
| `s1_entities_708.asm` | Fourteen source rows: thirteen ordinary NPC rows plus ally May. Relevant retained rows are old man `(37,8)`, Down; young man wandering about `(14,23)`; and May wandering about `(18,27)`. This is a source-list count, not a fixed physical-slot or visible-actor count. |
| Flag-selected followers | `InitializeMapEntities` clears entities and calls `InitializeFollowerEntities` before reading the fixed list. `data/scripting/entity/followers.asm` includes F67 → Peter. Map8 is not an overworld map, so F65 does not select the overworld caravan prefix here. Other follower flags remain inputs. |

The entity initializer maps ally rows by ally ID and ordinary NPC rows from event index
128 upward; followers consume physical slots first. Character 128/129 are therefore
lookup identities, not raw physical slots 128/129. Ally rows already represented in the
lookup can be skipped. Bowie's main entity occupies physical slot 0. The accepted
[Peter predecessor](./peter-castle-audience-through-town-return.md) supplies F7, AI control
and F67; F39 active membership remains conditional. Visible following, joining, active
battle membership and AI control are four separate claims.

`s6_initfunction.asm:ms_map8_InitFunction` (`0x563A2`; retained primary body 12) tests
F8. When set it calls `MoveEntityOutOfMap(ALLY_MAY)`, whose shared helper resolves the
current mapping and writes position `(0x7000,0x7000)`. Clear returns. It neither joins
May, sets F8, proves a recruitment conversation, nor verifies a meaningful present
mapping. In the default two-NPC setup there is no authored May row. This supplied-state
case must not be silently upgraded into confirmed hiding of a visible actor.

Map8 `3-flag-events.asm` has four F709 block-copy rows for doors and an unrelated F731
tree row. `LoadMapBlocksAndLayout` applies each row only when its flag is set. F708
chooses population/events; F709 chooses these door copies. Neither implies the other for
arbitrary supplied states. May's F8 is independent of both. No layout or collision is
decoded, and F731's later content is excluded.

## Welcome zones and the initial accusation

**Confirmed:** `s3_zoneevents.asm` (`0x561AE`) has these ordered dispatch rows:

| Zone / handler | Gate, script and writes after its return |
| --- | --- |
| `(15,28)` and `(16,28)` → `Map8_ZoneEvent0` | If F708 set, return; else if F258 set, return; else run `cs_563B2`, then set F703, then F258. |
| `(14,8)` → `Map8_ZoneEvent2` | If F708 set, return; otherwise run `cs_56422`, then set F708. No F703/F258/F709 test. |
| Default | Return. |

The ordinary movement producer requires zone attribute `$1400`; row coordinates alone
do not prove invocation. Type 6 applies main's init actscript and passes request X/Y to
`RunMapSetupZoneEvent`. This selects one handler, closes portrait/text afterward, waits
one VInt and waits for physical entity 0 to stop. It does not join every actor's action
or clear a request written during the handler. Pending zero at the next `WaitForEvent`
is the first opportunity to reinstall control after either welcome.

F258 is an entry-local suppression flag, reset by ordinary supplied-map entry. Leaving
and reentering before F708 is set can run the first scene again; remaining in the same
entry after F258 is set skips it. F703 is persistent and reused: Map14's init runs
`cs_58FA4` and sets F703 in the earlier plank/deck sequence. The Ribble handler does
not check it. F703 already set therefore neither suppresses the accusation nor proves
Ribble was visited. F258 and the first scene are not prerequisites for the mayor.
Running the mayor first is allowed by the local flag gate, but its actor staging and
natural reach are not thereby established.

`scripts.asm:cs_563B2` (`0x563B2`, lines 4–33) has **16 records: 15 opcode commands
and one terminator**, including three dialogue calls, with no sleep records. Records
1–4 set cursor 1291, disable 129's auto-facing, wait for four steps right, and display
1291. Records 5–10 set speed `(40,40)` through an inline actscript, wait for two steps
left, face Down, display 1292, wait for two steps up and display 1293. Records 11–16
reset the actor, set speed `(48,48)`, face Up, wait for eight steps up, explicitly place
129 at `(15,7)`, Down, then end. The young man suspects the newcomers and calls the mayor.
That final placement is a command, not proof that all earlier movement was traversable.

All three text IDs 1291–1293 (`0x050B..0x050D`) use character 129 and `{W1}`; 1292/1293
also contain `{N}`. The text comes from `data/scripting/text/gamescript.txt`. Source
physical line count is not command count, and nested move/`ac_` bodies are accounted for
within their outer records. Caller flag writes occur only after interpreter return.

## Mayor scene through explicit reload and control

**Confirmed:** `cs_56422` (`0x56422`, lines 34–166) has **88 records: 81 opcode
commands, six sleeps and one terminator**, with 27 dialogue calls, IDs 1294–1320
(`0x050E..0x0528`). Five sleeps are 30 and the last is 50. The complete source file's
177 physical lines also include the previous program, comments and the helper alias;
they are not 177 commands.

| Records | Ordered scene content and boundary |
| --- | --- |
| 1–7 | Set cursor 1294, initialize Peter, set his destination `(14,10)` (the normal `setDest` form waits for movement), face him Up, disable Bowie/Peter auto-facing, set 129's speed `(48,48)`. No join, F67 write or population repair occurs. |
| 8–31 | Five 30-unit sleeps precede five passes of Bowie moving Down once and mayor 128 bumping/moving Down once. Peter gets a non-waiting Down action in passes 2–5. The mayor's action is waited; Bowie's/Peter's launched actions are not individually joined by those commands. |
| 32–42 | Mayor challenges them; 129 moves Left1/Down4/Right1/Down1 with a wait, then reports soldiers and joins the accusations. Text 1294–1303 supplies the conflict. |
| 43–59 | Initialize 129/Bowie/Peter; launch mayor Up1 without waiting and 129 Up1 with waiting. Peter jumps, challenges the accusation (1304–1307), moves Left1/Up1, then another Up1 around the mayor's 1308 response. Bowie and the NPCs are explicitly faced during the exchange. |
| 60–75 | Peter explains the disaster-driven arrival (1309–1312), identifies himself as a phoenix (1314), and the NPCs change their response (1313, 1315–1320). Mayor/accuser jump twice: each mayor jump is non-waiting and each accuser jump is waited. No explicit choice or refusal branch occurs. |
| 76–88 | Fade out; load `ce_55FBE`; initialize Bowie; make four block copies; set F709; call `j_csub_40F2`; follow Peter→Bowie with offset selector 2; sleep 50; fade in; terminate. The caller then sets F708. |

The exact dialogue actor/control grouping is: 1294 mayor; 1295–1296 accuser;
1297–1300 mayor; 1301–1302 accuser; 1303 mayor; 1304–1307 Peter; 1308 mayor;
1309–1312 Peter; 1313 mayor; 1314 Peter; 1315 mayor; 1316 Peter; 1317–1318 mayor;
1319–1320 accuser. `{W2}` occurs on 1295, 1297–1299, 1301, 1305, 1309–1311,
1317 and 1319; all other entries use `{W1}`. `{N}` retains line breaks; 1305 has
`{LEADER}` and 1314 has `{NAME;7}`. These are authored text controls, not measured
wait durations. `nextText` leaves the shared text/portrait lifecycle open;
`nextSingleText` closes it and sleeps 10 after display. Both increment the cursor on
the normal display path. Debug text-skip bypasses display/cursor increment and sleep
records, so the ID sequence above describes the normal, non-skipped stream.

The reload is an explicit pointer operation, not a setup selection. `pointertable.asm`
ends with `ce_55FBE: mainEntity 14,13,UP`; `data/maps/mapsetupsstorage.asm` places
`s1_entities_708.asm` immediately after it. `csc42_loadMapEntities` reads three main
words then passes the following address to `InitializeMapEntities`, disabling VInts
and display/interrupts around the rebuild and mapsprite load. It consequently creates
main `(14,13)`, Up, flag-selected followers and the fourteen-row population **while
F708 is still clear** in this normal call. It does not run `LoadMap`, map init, temporary
flag clearing or a new warp. In particular it does not rerun F8-dependent May hiding.
An F8-set supplied state at this reload cannot be described as guaranteed hidden May.
Later setup lookups use F708 after the caller sets it; later ordinary entry rebuilds
from that selected table and runs init again.

Records 79–82 copy `(source X,Y; width,height → destination X,Y)` in this order:
`(32,2;1,1→25,12)`, `(32,2;1,1→22,24)`, `(32,2;1,1→7,25)`,
`(32,3;2,1→6,15)`. `csc34_setBlocks` invokes `CopyMapBlocks` and sets both view-plane
update bits. Record 83 sets F709, whose four flag-event rows reconstruct the same
door changes on a later map load, in their own table order. F708 alone does not do this.
The step table's mayor-door `(14,9)` copy is separate; the four welcome copies do not
include that door. Copied working blocks and queued refresh do not prove visible doors
or passable collision.

Record 84's alias resolves from `scripts.asm:j_csub_40F2` to
`code/gameflow/exploration/exploration.asm:csub_40F2` (`0x40F2`). It tests
`word_FFAF42`: zero returns; nonzero clears it, reads saved destination X/Y and
decremented width/height at `0xFF5C00..0xFF5C06`, restores saved words beginning at
`0xFF5C08` into the working layout at `0xFF0000` (64 words per row), sends
`MUSIC_NOTHING` and sets plane-update bit 0. `PerformMapBlockCopyScript` is the
snapshot/copy-or-clear producer; the busy value is its matched roof-row ordinal.
Map8's mayor-door roof row is trigger `(14,9)`, source `(255,255)`, size `(7,8)`,
destination `(13,33)`. This is a conditional saved-rectangle restore, not a door unlock,
full map redraw, setup reload or guaranteed music-state change. No natural prior roof
activation is proved. An unexpected saved rectangle could overwrite overlapping working
edits; its actual identity/state remains an input. The existing
[map-block copy lifecycle](./common-scripting.md#confirmed-map-block-copy-showhide-lifecycle)
has bounded H3 helper observations, not a Ribble scene composition.

Record 85 is `followEntity ALLY_PETER,ALLY_BOWIE,2`. The macro in
`sf2cutscenemacros.asm` encodes three words, `$0007,$0000,$0002`.
`mapscriptengine_1.asm:csc2C_followEntity` probes the first word's high byte with
`move.b (a6),d0` before advancing A6. `AdjustScriptPointerByCharacterAliveStatus`
therefore checks character 0/Bowie's HP: zero skips all six operand bytes and returns
without `AddFollower`. Peter's HP itself is not this command's guard. If Bowie is
alive, the handler resolves the current Peter/Bowie mappings and `AddFollower` installs
the follow actscript; selector 2 maps to pixel offset `(-24,0)`, without validating
Peter's HP or presence. The [existing high-byte seam observation](./common-scripting.md#confirmed-map-script-entity-gesturerelationshipmotion-command-family)
is a controlled handler case, not a Ribble runtime observation. The command does not
set F67, join Peter, set F39 or change AI control. Persistent F67 determines Peter's re-instantiation on
later ordinary entry. Other action/facing/destination handlers do not share a universal
dead-actor guard: `GetEntityAddressFromCharacter` performs a lookup without proving
presence, and action-script/sequence waits depend on the mapped actor reaching
`eas_Idle`. Missing Peter or NPC mappings, dead allies and actions launched without
waiting must remain explicit; neither F708 nor the actor names prove successful staging.

After fade-in/terminator, `Map8_ZoneEvent2` sets F708, then the common zone dispatcher
closes portrait/text, waits one VInt and main's movement, and returns to exploration.
There is no explicit new pending request in either selected scene or handler and no
unconditional post-handler clear. At the next `WaitForEvent`, pending zero makes view
follow main and installs `eas_ControlledCharacter` for carried `PLAYER_TYPE=0`; a nonzero
pending event goes to dispatch first. This is the stopping boundary, conditional on
all invoked services returning. It does not prove fade completion, all follower actions,
natural input responsiveness or traversal. Ordinary reentry with F708/F709 set selects
the populated setup, clears F258, reconstructs door copies, then runs the F8-dependent
init. Both welcome handlers remain suppressed despite that temporary reset.

## Coverage, discriminators and reproduction

Existing fixture paths below are under `tests/fixtures/h2/`; named H2 commands have
prefix `uv run sf2 h2`. Coverage means the admitted owner, not a fresh full extraction.

| Relation | Existing boundary and concrete missing composition |
| --- | --- |
| Battle09 predicates/outcomes | `battle-loop-static-v1.json`, `battle-cutscenes-static-v1.json`, `battle-global-data-static-v1.json`, `battle-routing-data-static-v1.json`; commands `battle-loop`, `battle-cutscenes`, `battle-global-data`, `battle-routing-data`. Direct row-9 and shared-tail review plus cutscene-source comparison. Controls: slot 128 dead with other counted enemies; simultaneous faction loss; F509-clear versus repeat defeat/egress; variable first party member and egress map. No new combat execution. |
| Map69→8 request/admission/init | `map-data-static-v1.json`, `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-init-static-v1.json`, `map-event-request-consumption-static-v1.json`; commands `map-data`, `map-setup`, `map-entities`, `map-init`, `map-event-request-consumption`. Retained init body 12 and direct warp row/pointer review. Missing: reachable victory position→warp→zones, actual pending lineage and collision. F417 competition or a request overwrite discriminates a forced-route claim. |
| Welcome flags and scripts | `map-events/zone-programs.json` programs 145/146/147 retain the two handlers and default; `map-script-engine-static-v1.json` retains `cs_563B2` and `cs_56422`. Commands `map-events`, `map-script-engine`. Direct text/control review and retained comparison. Controls: F703 already set, F258 reset on entry, mayor without first scene, F708-set/F709-clear and F709-set/F708-clear. |
| Population, blocks and helper | Existing map-script engine entity-population, entity-action, block-mutation and follow consumers; common scripting's `map-block-copy-lifecycle-v1.json` H3 fixture. Direct alias/helper/storage adjacency review. Missing executable composition: reload before F708, no init after reload, F709 copies then optional cached-rectangle restore, actor waits and pending/control return. Controls: F8 set at explicit reload, F67 clear, Bowie HP zero (follow skipped) versus Bowie alive/Peter HP zero (not skipped by this guard), missing Peter mapping, inactive versus active roof cache. No new H3 run is selected. |

For reproduction, load `. ./local/private-inputs.ps1` in the owning checkout; select
the registered read-only `$upstream`, and verify `git -C $upstream rev-parse HEAD` and
`status --short`. Run the existing
[retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
for map-init/map-script-engine and cutscene source. Select corpus programs by `entryLabel`
`cs_563B2`/`cs_56422`; count `kind=command`, `sleep`, `terminator` separately; compare
their records with the named pinned source lines. Resolve dialogue IDs against four-digit
hex keys in `gamescript.txt`, and compare the exact zone operations and init body 12.
Read the row-9 tables, both wrapper tails, outcome/main-loop consumers, Map69 warp,
Map8 pointer/storage ordering, helper alias/body and pending/control call chain together.

Runtime reach, layout/collision, caller actor state, all service completions, cache
identity and playability remain **Unknown**. Existing separate fixtures do not close
those composed claims, and they do not require a new scene fixture or platform here.
Record exact source/document/link/private/diff checks, failures, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD` and actual final-head CI in the
Issue/PR handoff. Normal/full/engine/H3/H4/Godot runs are not selected for this docs-only
scope. Preserve prior evidence, freeze the clean pushed Draft PR for independent review,
move the Issue to Review / Verify and send the authorized completion callback; no
self-merge, cleanup or next-slice selection.
