# Battle08 outcomes through the New Granseal castle handoff

Evidence date: 2026-10-13. **Confirmed:** first victory runs `abcs_battle08`, whose return
tuple is Map73 `(62,49)`, Left. The battle caller then clears F408, sets F508 and enters
exploration directly. With F700 set and F705 clear, Map73 init runs `cs_50806`, resets all
ally battle stats and requests Map7 `(11,10)`, Up. Its caller sets F705 before exploration
can consume that request. **Inferred:** the scenes depict rescuing a worker and returning
to a dismantled ship and newly built town. **Unknown:** natural outcomes, tunnel access,
actual actor/follower state, completed services and pending-request survival. This is a
conditional source chain, not a continuous runtime journey or remake capability.

## Agreement and provenance

This is the Standard-rigor agreement for
[Issue #736](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/736), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Battle outcomes and two different return consumers require explicit state and ordering.
The sole tracked owner is this document; no shared writes or stacked dependency. Starting
accepted base is `27c14a4da9f4cdc7e1b6322b7ae43c49060880f4`, including
[plank through Battle08 admission](./damaged-ship-plank-through-battle08.md) from
[PR #734](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/734).
Concurrent design synthesis consumes accepted predecessors independently.

Start after Battle08's before/start dispatch. Reuse shared outcome semantics and inspect
Battle08's selected rows, after scene, return fields and Map73 init selection. Follow the
selected later init through its outgoing castle request and immediate main-loop admission;
stop before destination initialization. Castle scenes, Battle09, recruitment, later town
content, combat simulation, traversal/layout acquisition, new fixtures/parsers/platforms,
emulator/engine/UI work and Chinese translation are excluded. Acceptance is direct pinned
source/text/fixture and retained-comparison review, complete document/reference/link/private/
diff checks, a clean committed planner and actual final-head CI. A clean pushed Draft PR
freezes for independent main-gate acceptance and integration.

Original paths below are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM` at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The reused read-only checkout is clean at that pin. Retained `map-init` and
`map-script-engine` payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
their fixture comparisons and the existing cutscene source comparison pass. No new ROM
verification, extraction or runtime observation is claimed. Fnnn denotes flag nnn; command
indices are zero-based retained `programCorpus.programs` records, not frames or inline action
indices. Short map paths use prefix `data/maps/entries/`; bare map-script filenames use
`code/common/scripting/map/`; battle filenames use `code/gameflow/battle/`.
Addresses are static source anchors.

## Outcomes, rows and return fields

**Confirmed:** shared [battle-loop](./battle-loop.md) and
[cutscene](./battle-cutscenes.md) consumers apply. As detailed in the accepted
[Battle07 outcome owner](./battle07-outcomes-and-aftermath-handoff.md#outcome-branches-and-caller-order),
the loop checks deaths/combatant counts after actions and turn-end effects. Zero allies
(including Bowie's zero-HP override) takes precedence over zero enemies. Battle08 is not a
special leader-only victory. BattleLoop sets F399 and clears `PLAYER_TYPE` on entry.

| Selected source / row | Battle08 result |
| --- | --- |
| `data/battles/cutscenes/battleendcutscenes.asm`, row 8 at `0x47BF8` | `ms_Empty`. `ExecuteBattleCutscene_Defeated` requires Bowie alive, enemy slot 128 dead and F508 clear to dispatch it. |
| `data/battles/global/enemyleaderpresence.asm`, row 8 at `0x47C96` | Zero; the shared defeated tail does not append every surviving enemy merely because slot 128 died. |
| `data/battles/global/afterbattlepositions.asm` | Only Battle05 is listed. `ApplyPositionsAfterEnemyLeaderDies` has no Battle08 position/HP override. |
| `data/battles/cutscenes/afterbattlecutscenes.asm`, row 8 at `0x47D04` | `abcs_battle08` (`0x4AA74`), selected by `ExecuteAfterBattleCutscene` only with F508 clear. |
| `data/battles/cutscenes/afterbattlejoins.asm`, row 8 at `0x47D72` | Zero. `EndAfterBattleCutscene` calls `JoinForce(0)` (Bowie) on both executed and skipped after paths. The source's unused-feature comment does not suppress this call; it is not the similarly named legacy global table or a worker/Kiwi recruit. |

| Outcome / input | Writes and resource effects | Returned destination |
| --- | --- | --- |
| Victory, F508 clear | `battleloop_2.asm:BattleLoop_Victory` heals living/immortal allies; seeds event parameters; executes after script and shared join; clears F408, sets F508; returns d4=1. | Script replaces the seed with Map73 `(62,49)`, Left. |
| Victory, F508 set | Same heal, seed, shared join and F408-clear/F508-set; after scene is skipped. | Current Map72, first active party member's final coordinates plus battle-area origin `(1,32)`, and its entity facing. No fixed Map73 return or worker scene is supplied. |
| Defeat | `BattleLoop_Defeat` calls `UpdateBattleUnlockedFlag`, shows fixed text 363, restores Bowie's max HP, halves gold by unsigned shift and returns d4=-1. Battle04's loss exception does not apply. | `GetEgressPositionForBattle` has no Battle08 special case; it uses current `EGRESS_MAP`. No after/join or new F508/F705 write. |
| Egress / Angel Wing | `battlefunctions/battlefunctions_1.asm` charges spell MP / removes the selected item slot, uses fixed text 274 / 275, then updates the unlock flag and returns the egress tuple with d4=0. | Same stored-map selection as defeat; no defeat gold halving or after/join call. |

`UpdateBattleUnlockedFlag` in that battlefunctions file leaves F408 when F508 is clear;
with F508 set it clears F408. Neither defeat nor escape creates completion. With the
predecessor's unchanged `EGRESS_MAP=14` and BattleLoop's F399 set,
`code/common/maps/egressinit.asm:GetSavepointForMap` selects the
`MAP_SHIP_DAMAGED` row of `data/maps/global/savepointmapcoords.asm`: Map14 `(45,7)`, Down.
This is an input-dependent example; other stored maps select their own rows or the `(1,1)`,
Up fallback. The shared F64-gated raft reset and F399-clear game-start fallback retain their
accepted meanings. Outcome tuples pass through `SwitchMap`, not a new battle-admission scan.
No natural defeat/escape reentry is demonstrated here.

The ordinary victory heal and subsequent exploration heal skip dead non-immortal allies;
the [shared heal owner](./battle07-outcomes-and-aftermath-handoff.md#outcome-branches-and-caller-order)
retains its HP/MP and status-mask limits. This differs from the later full-force reset below.
F458 gates the before/start programs independently of F508. F458-set intro skips and F88 suspended entry
(restore seconds, clear F88/AI memory, load battle, resume individual turns) do not themselves
set F508 or suppress a later F508-clear after scene. Supplied suspension validity and natural
combat remain Unknown; no combat engine is re-proved here.

## Battle08 after scene

**Confirmed:** `data/battles/entries/battle08/cs_afterbattle.asm` lines 4-82 contain
`abcs_battle08`, commands 0-60. `ce_4ABBE` declares main `(14,37)`, Up; Sarah `(13,36)`,
Jaha `(12,35)`, Kazin `(15,36)` and Chester `(16,35)`, all Up; Kiwi `(16,34)`, Left.
Worker 128 is `(14,35)`, Up and worker 129 `(14,34)`, Down. Slade has no fixed record.
These are scene declarations, not battle roster or HP facts. Loading the list rebuilds
entities/followers; already mapped allies may skip fixed declarations. NPC references use
`ENTITY_INDEX_LIST`, not physical allocation order. Follower flags remain caller inputs.

| Commands | Complete ordered phase |
| --- | --- |
| 0-8 | Cursor 2468; load Map72, view `(8,32)`; load the list; await Bowie init; stop 129's animation and await custom motion-off, Left orientation, sprite update, idle. F6 set jumps to `cs_4AAB6` (8); clear places Kiwi `(63,63)`, Down at 7. Both fade in. |
| 9-23 | Guide 128 asks about his friend, faces Left, awaits Left 1, faces Up, awaits Up 1, faces Right and reacts. Initialize 129; give him `MAPSPRITE_POSE1`, Left; dialogue mentions a falling rock. Restore worker sprite, face Down, ask where the rock is, initialize again. |
| 24-38 | Wait 5; start 129's `eas_2xRightLeft` without a join; wait 120; face Left, start animation, two replies separated by facing Down. Start 128's `eas_BumpRight` unjoined; await 129's custom speed `(32,32)`/idle, then `eas_4618A`; wait 8; await 129 Right 1, face Down and headshake. |
| 39-51 | Guide explains the rescue; 129 faces Left and describes the rock, unconsciousness and blocked tunnel. Face 129 Right; wait 20; face Kiwi Right even on the F6-clear path; camera destination `(22,32)`, wait 30, then `(8,32)`. |
| 52-60 | Await guide Right 1 and `eas_Jump`; two lines lament the inaccessible treasure; face guide Down and await Down 1; propose returning to the ship; request Map73 `(62,49)`, Left; end. |

The 15 dialogue operands match `data/scripting/text/gamescript.txt` keys `09A4..09B2`
(2468-2482). **Inferred:** the posing, animation and dialogue depict the worker regaining
consciousness; they do not implement healing, death, force joining or item acquisition.
F6-clear relocation is not `hide` or a force-membership change. The scene has no block-copy,
rock-removal or tunnel-traversal operation. The blocked-tunnel and treasure statements are
source dialogue, not a tested collision/path or proof of treasure contents.

Command presence is distinct from effective ally mutation: handlers using
`mapscriptengine_1.asm:AdjustScriptPointerByCharacterAliveStatus` skip their payload for a
zero-HP ally below index 30; NPC references 128/129 bypass that ally check. In particular,
F6 clear selects Kiwi's `setPos` command but does not prove a position write for dead Kiwi.

`data/scripting/entity/eas_actions.asm` defines the reused actions: `eas_2xRightLeft` sets
Right/Left/Right/Left, updating sprites and waiting 30 after each; `eas_BumpRight` disables
auto-facing, requests relative X +1 / wait 5 / -1 / wait 5 / zero movement, restores auto-facing
and idles. `eas_4618A` sets acceleration factors `(1,1)`, deceleration `(-2,ON)` and idles.
`eas_Jump` similarly uses relative Y -1/+1 with waits 5. The script's wait 120 is not an
explicit join of the unjoined facing action. Waited commands concern named entities; they do
not prove simultaneous completion of other actors, camera, audio or text.

## Battle caller to Map73 init

**Confirmed:** the first return follows this order:

1. Command 59 uses `mapscriptengine_2.asm:csc07_warp`: write word event type 1,
   parameter 1 zero and byte parameters 2-5 = `(73,62,49,LEFT)`. It requests a map; it does
   not load it or end the script. Command 60 ends; the interpreter's normal return tail
   conditionally waits for view scrolling and restores registers.
2. The after wrapper calls the shared `JoinForce(0)`. `BattleLoop_Victory` clears F408,
   sets F508, reads parameters 2-5 into d0-d3 and returns d4=1. This consumes the fields
   directly, without `WaitForEvent` or `ProcessMapEventType1_Warp`. No intervening ordinary
   player-controlled exploration is implied; field survival across services is still unobserved.
3. `code/gameflow/mainloop.asm:alt_MainLoopEntry` (`0x75E0`) calls `SwitchMap` and goes
   directly to `ExplorationLoop`, bypassing `CheckBattle`. Map73 is absent from
   `data/maps/global/flagswitchedmaps.asm`, so its tuple is unchanged.
4. `code/gameflow/exploration/explorationfunctions_2.asm:ExplorationLoop` (`0x257C0`)
   immediately clears the old pending word, heals and fades. Explicit Map73 entry sets
   current map / not-in-battle, obtains setup entities, rebuilds them, loads sprites,
   clears temporary flags 256-383 and sets F80. F408/F458/F508/F700/F705 lie outside
   that clear range. It loads the map and base VInts, then calls the selected init before
   any control installation.

`data/maps/mapsetups.asm` has only Map73's default setup. Its
`map73/mapsetups/pointertable.asm` points to the empty fixed list in `s1_entities.asm`
and `ms_map73_InitFunction` (`0x50342`) in `s6_initfunction.asm`, retained `map-init`
primary source body 79. Empty fixed entries still admit flag-selected followers. This init
does not test arrival coordinates, F408, F458, EGRESS_MAP or worker health:

| Inputs at init | Selection and caller write |
| --- | --- |
| F700 clear, any F508/F705 | Run `cs_503A6`, set F700 after return, then return without testing the later branch. The accepted [voyage owner](./voyage-through-damaged-ship-control.md#map73-admission-and-init-selection) owns this alternate content. |
| F700 set, F508 clear | Return without either scene. |
| F700 set, F508 set, F705 set | Return without either scene. |
| F700 set, F508 set, F705 clear | Run `cs_50806`, set F705 after return, then return. |

The intended lineage supplies F700 from the earlier voyage caller and F508 from this victory;
F705 clear is a required input, not a fresh write here. Direct init with F508 already set can
select the later scene without this after script; F700 clear instead reselects the voyage.
Skipping `abcs_battle08` does not itself return to Map73, so the init table must not be attached
unconditionally to every repeat victory. Natural entry and pending survival remain Unknown.

## Map73 later scene and castle request

**Confirmed:** `cs_50806` (`0x50806`) occupies lines 312-384 of the Map73 init file,
commands 0-52, with no script-level branch. `ce_5094A` replaces the current population:
main `(61,48)`, Left; worker 128 `(62,48)`, Left; worker 129 `(63,48)`, Left; ship sprite
130 `(28,49)`, Right. These workers are newly declared references, not persistent identities
proved equal to the previous scene's workers. There are no fixed ally declarations here.
`InitializeMapEntities` still initializes followers, including Map73's F65-dependent overworld
selection and other follower flags. Those mappings and service state remain inputs.

| Commands | Complete ordered phase |
| --- | --- |
| 0-10 | Cursor 2288; load list; await Bowie init, 128 init twice, then Bowie init again (the duplicate calls are real). Install 129 following Bowie, then Bowie following 128, both selector 2. Copy blocks `(42,0)`, size `11x10`, to `(24,48)`; camera follows 128; fade in. |
| 11-14 | Four awaited action lists on 128: Left 4, Up 1, Left 2, Up 2; then Left 1, Up 1, Up 1, Left 8, Down 2; then Left 8, Down 2, Left 3, Down 1, Left 2, Down 2; finally Left 5. These direct the guide and leave follower completion separate. |
| 15-20 | `setPosFlash 130,0,0,LEFT`; worker comments that the ship has been dismantled; face Bowie and both workers Down; camera destination `(25,47)`. |
| 21-34 | At destination `(24,48)`, copy `11x10` from `(53,0)` / wait 15, `(42,0)` / 5, `(53,0)` / 15, `(42,10)` / 5, `(53,10)` / 15, `(42,10)` / 5, `(53,10)` / 30. |
| 35-40 | Worker comments on the new town and old Granseal; set neutral camera; await init for 128, 129, then Bowie, replacing their current action scripts. |
| 41-48 | For 128, then 129, then Bowie: `setDest ...,29,52`, followed by `setPos ...,0,0,DOWN`. Wait 60; narrator describes the town-building work. |
| 49-52 | Request sound fade-out; `resetForceBattleStats`; request Map7 `(11,10)`, Up; end. |

The four dialogue operands match `gamescript.txt` keys `08F0..08F3` (2288-2291).
**Inferred:** the block replacements and dialogue portray dismantling the ship and completing
the town. `csc34_setBlocks` calls `CopyMapBlocks` and marks both view planes for update;
these are working-map copies, not a measured construction interval or a persistent world-state
transaction. No building material/item inventory is consumed. `setPosFlash` alternates the
ship entity's X coordinate while waiting, then uses `csc19_setEntityPosAndFacing` to assign
position/destination `(0,0)`, Left. It is not `hide`, deletion or a combat death.

`csc2C_followEntity` resolves current entities and selector 2 to offset `(-24,0)`, then calls
`code/common/scripting/entity/entityfunctions_2.asm:AddFollower` to install a follower action
script. It does not set persistent follower flags or join the force. Later `eas_Init` installations
replace these scripts; they do not establish the survival of a follow across another map load.
The guide's awaited movement does not validate follower collision or final positions.

`sf2cutscenemacros.asm:setDest` passes each entity word unchanged to `csc29_setEntityDest`.
Here all three words have bit 15 clear, so the handler waits for that entity's position to equal
its destination `(29,52)` before returning. Each following `setPos` overwrites both position and
destination to `(0,0)`, Down. These are sequential named waits and relocations, not three
unjoined motions or a global completion barrier. Actual successful service completion is Unknown.

Command 50 calls `mapscriptengine_1.asm:csc55_resetCharacterBattleStats` →
`resetalliesstats.asm:ResetAlliesBattleStats` (`0x478C6`). As established by the
[voyage reset owner](./voyage-through-damaged-ship-control.md#force-reset-and-entity-list-semantics),
indices 0-29 receive max HP and max MP with no joined/active/alive filter. Existing status is
masked to stun/poison/curse, then stats are rebuilt; equipment subsequently derives curse.
The mask retains existing bits, not newly applied ailments. This can restore zero-HP allies,
unlike the earlier living/immortal heal, but does not recruit or change party membership.

### Pending consumer and stopping boundary

**Confirmed:** command 51 writes type 1, parameter 1 zero and `(7,11,10,UP)` via
`csc07_warp`. After command 52 and interpreter return, **Map73 init sets F705**. The
script itself contains no F705 write. The surrounding exploration loop then performs its
conditional music/fade calls and reaches `WaitForEvent` (`0x2591C`). If type 1 survives,
the nonzero pending read skips control installation. `ProcessMapEvent` clears the word before
dispatch; `ProcessMapEventType1_Warp` takes parameter-1-zero, handles/clears WARP_SFX,
makes entity 0 idle and unwinds exploration. It reads map/facing and the positive X/Y bytes
into d0-d3, returns d4=0 and hands Map7 `(11,10)`, Up to `MainLoop/@Start`
(the main routine begins at `0x75C4`).
The initial request is one shared slot, not a queue: an intervening overwrite/clear can prevent
this consumption even though F705 has already been set.

`sf2enums.asm` identifies Map7 as `MAP_NEW_GRANSEAL_CASTLE`. Unlike the battle outcome
return, this ordinary exploration return runs both `SwitchMap` and `CheckBattle`. Map7 has
no flag-substitution entry and no row in `data/battles/global/battlemapcoords.asm`; d7=-1
selects exploration with the unchanged tuple. **Stop before that destination's ExplorationLoop
initialization.** No Map7 setup/init/castle scene, recruit or later encounter is inspected.

On the intended completed chain, F408 is clear, F508/F700/F705 are set; F458 is carried from
the predecessor's ordinary start (not established by a suspended entry); EGRESS_MAP remains
the supplied value, typically 14. PLAYER_TYPE remains 0 from BattleLoop on this chain.
Other follower/story flags retain their supplied history. Map73 remains current until the next
explicit map-entry consumer; the request is not itself a current-map change. Scene-local
actor/follow state and completed rendering/audio are not guaranteed carried facts. If pending
is zero at the control read, control installation can occur on Map73 instead; no unconditional
castle arrival or player-ready castle state is claimed.

## Coverage, gaps and reproduction

Fixture names below are under `tests/fixtures/h2/`; narrow command names have prefix
`uv run sf2 h2`. These are existing coverage boundaries, not freshly executed full rails.

| Relation / owner | Reused coverage and concrete gap |
| --- | --- |
| Outcome checks, flags, healing, joins and selected rows | `battle-control-static-v1.json`, `battle-loop-static-v1.json`, `battle-cutscenes-static-v1.json`, `battle-global-data-static-v1.json`, `battle-routing-data-static-v1.json`; [battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md), [routing](./battle-routing-data.md). Commands use those basenames. **Covered** shared consumers; direct source supplies row 8 and the absent position override, not routing's `routeTargetsParsed=false` summary. Controls: slot-128-only death, zero Bowie HP, F508 skip and F88 resume. |
| After/init programs, lists, follows and reset | `map-script-engine-static-v1.json` programs `abcs_battle08`, `cs_50806`; `map-init-static-v1.json` body 79; `map-setup-static-v1.json`, `map-entities-static-v1.json`; [common scripting](./common-scripting.md), [map inventory](./map-data-inventory.md). Commands: `map-script-engine`, `map-init`, `map-setup`, `map-entities`. **Covered** source operations; retained comparisons pass. Inline actions/lists, text and setDest waits were read directly. Controls: F6 clear, conflicting follower mappings, dead ally reset, F700-clear and F705-set selectors. |
| Battle fields versus exploration pending consumer | `map-event-request-consumption-static-v1.json`, `common-maps-static-v1.json`, `common-scripting-static-v1.json`; [request consumption](./map-event-request-consumption.md). Commands use those basenames. **Covered** separate consumer shapes. Missing executable composition is after-return fields → caller completion flags → Map73 selector → later request → F705 → pending consumption. **Reuse/extend** these owners only if that composition is required; no new scene fixture/platform is justified. |
| Minimal future composition and runtime boundary | Public-safe facts would bind source/caller IDs, input flags, actor mappings, outcome/return tuple, selected init, reset ordering and consumed tuple. Counterexamples are repeat victory's Map72 seed, alternate EGRESS_MAP, F700-clear voyage, F705-set skip and an overwritten request. **Unknown:** natural battle outcome, tunnel traversal, actual actor/follower state, completed services and request survival. No automatic runtime queue or new acquisition is authorized. |

Bounded reads reuse two payloads totaling 18,285,979 bytes (about 18.3 MB), without rewriting
them. Comparisons are linear in input size; one process, under-512-MiB estimated peak and a
one-minute comparison budget suffice. Outputs are this prose and small ignored review/planner
receipts. No full transcript, layout, roster or capture is published. Replan if dependencies
require acquisition or pass the outgoing-castle boundary.

Load `. ./local/private-inputs.ps1` in the owning checkout. Run the accepted
[retained-payload and cutscene-source comparison](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
with locally selected `$upstream`; all three comparisons pass. Verify the checkout with
`git -C $upstream rev-parse HEAD` and `git -C $upstream status --short`. Keep its machine
path private. Select the two program IDs above in retained `programCorpus.programs`; compare
each `sourceLine`, `macro` and `arguments` to pinned source and read embedded actions/lists.
The selections have 61 + 53 commands and 15 + 4 dialogue IDs. Check `cs_4AAB6` at command 8,
text keys `09A4..09B2` / `08F0..08F3`, init body 79, row-8 targets, savepoint row, reset,
setDest/follow handlers and both return consumers at the paths/labels above.

Record direct prose/source/reference/link/private checks, `git diff --check`, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD`, actual final-head CI and completed
failures in the Issue/PR handoff. Documentation-only scope selects no local normal/full/engine/
H3/H4/Godot suite. Preserve ignored inputs/results; freeze the clean pushed Draft PR for
independent review. No self-merge, environment cleanup or next Issue is included.
