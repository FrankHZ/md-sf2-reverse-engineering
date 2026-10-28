# Voyage and landing through damaged-ship control

Evidence date: 2026-10-12. **Confirmed:** the departure request for Map73 `(26,13)`, Right
has no map substitution or matching encounter. Its F700-clear init runs `cs_503A6`, including
voyage, landing and the scripted plank/Jaha scene. Two in-script loads change `CURRENT_MAP`
to Map14 without invoking that map's setup init. On script return, the Map73 caller sets F700;
the surrounding exploration loop then reaches its first pending-event/control read on Map14.
**Inferred:** dialogue and staging depict arrival in Parmecia and preparations to dismantle the
ship. **Unknown:** natural arrival, service completion, actual actor state and a zero pending
event at that read. This is static ordering, not a continuous runtime journey or remake capability.

## Agreement and provenance

This is the Standard-rigor agreement for
[Issue #728](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/728), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Cross-map init, entity remapping and caller/control ordering require explicit boundaries.
The sole tracked owner is this document; no shared-path writes or stacked dependency. Accepted
starting base is `e0dfb67e9e64ac2b53260ecb3ae3f4d4c6b3537a`, consuming
[earthquake-town exit through docks departure](./earthquake-town-through-docks-departure.md).
Concurrent design synthesis consumes that accepted predecessor independently.

Start with the predecessor's outgoing Map73 tuple after F74/F640 clear. Follow admission,
setup and F700-clear init through the first post-init control/event read. Include both F6
branches, all inline actions, required subroutines and three scene entity lists. F700-set,
F508 and F705 establish selector counterexamples only. Stop before player-controlled plank
progression, Battle08, town construction or later campaign. No new extraction, fixture,
parser, platform, emulator, engine/UI work or Chinese translation is included. Acceptance is
direct pinned-source/text/fixture review, retained comparisons, document/link/private/diff checks,
a clean committed planner and actual final-head CI, followed by a frozen Draft PR for independent
main-gate review and integration.

Original paths below are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM` at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The reused read-only checkout is clean at that pin. The retained `map-init` and
`map-script-engine` payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`; both existing fixture
comparisons pass. No fresh extraction, ROM verification or runtime observation is claimed.
Addresses are static source/H1 anchors. Fnnn denotes flag nnn. Short `mapNN/...` paths have
prefix `data/maps/entries/`; bare map-script/setup/follower filenames have prefix
`code/common/scripting/map/`. Command indices below are zero-based records in retained
`programCorpus.programs[id=cs_503A6]`, not frames or inline entity-action indices.

## Map73 admission and init selection

**Confirmed:** the predecessor's type-1 no-scroll consumer returns `(73,26,13,RIGHT)` to
`code/gameflow/mainloop.asm:MainLoop/@Start` (`0x75C4`), with F74/F640 explicitly cleared.
`SwitchMap` has no Map73 entry in `data/maps/global/flagswitchedmaps.asm`. In
`data/battles/global/battlemapcoords.asm`, no first-column map value is 73.
`code/common/maps/getbattle.asm:CheckBattle` (`0x799C`) first compares map identity, then checks
unlock flags and trigger coordinates only for a matching row. It therefore returns no battle
for this request regardless of those flags. In particular, the Battle08 row names Map72, not
Map73. Neither a map name nor F508's comment substitutes for this admission check.

`code/gameflow/exploration/explorationfunctions_2.asm:ExplorationLoop` (`0x257C0`) clears the
pending event, heals living/immortal allies and fades out. With explicit map 73, it writes
`CURRENT_MAP=73` and the not-in-battle value, loads tilesets, gets the selected setup population,
initializes entities at the supplied position/facing, loads sprites, clears setup temporary flags
256-383 and sets F80. It loads the map, installs base VInts and calls `RunMapSetupInitFunction`.
Map73's `3-flag-events.asm` and `4-step-events.asm` are terminators only.

`data/maps/mapsetups.asm` gives Map73 a single default setup, with no flag-selected replacement.
`map73/mapsetups/pointertable.asm` (`0x50312`) points to an empty fixed-entity list and
`ms_map73_InitFunction` (`0x50342`) in `s6_initfunction.asm`. Empty fixed entities do not exclude
followers: `InitializeMapEntities` calls `InitializeFollowerEntities` first. F74 clear removes
that flag's royal follower declarations, but other follower flags and player type remain inputs.
There is no first player-control handoff before init in this path.

The init body, also retained as `map-init` primary source body 79, selects:

| Input at init | Ordered result |
| --- | --- |
| F700 clear, regardless of F508/F705 | Execute `cs_503A6`; **after it returns**, set F700; return. |
| F700 set, F508 clear | Return without either script. |
| F700 set, F508 set, F705 set | Return without either script. |
| F700 set, F508 set, F705 clear | Execute later `cs_50806`; after it returns set F705; return. |

The last row names the later town-building owner in the same source file; its scene is excluded.
An F700-set early return (F508 clear or F705 set) leaves this entry on Map73, not on the damaged
ship. Conversely,
F700-clear does not also fall through into the F508/F705 check after setting F700. No F74, F640,
party-ready or completed-Battle08 predicate gates the first script. Its natural first-arrival
lineage is supplied by the predecessor, not proven by an arbitrary direct invocation of this init.

## Force reset and entity-list semantics

**Confirmed:** command 1 is `resetForceBattleStats`, dispatched by
`mapscriptengine_1.asm:csc55_resetCharacterBattleStats` (`0x47000`) to
`code/common/scripting/map/resetalliesstats.asm:ResetAlliesBattleStats` (`0x478C6`). Its DBF loop
uses `COMBATANT_ALLIES_COUNTER=29` for indices 0-29, without a joined/active/alive filter. For
each, it reads current HP, then gets max HP and assigns current HP; gets max MP and assigns
current MP; masks existing status with `STUN|POISON|CURSE` (7); writes status; then calls
`UpdateCombatantStats`. It does not add those three bits. The latter routine
(`code/common/stats/updatecombatantstats.asm`) rebuilds stats and derives curse from equipped
items: the intermediate mask is not a promise that a supplied curse bit survives to final status.
This reset can restore a zero-HP ally; it does not recruit anyone or change battle-party membership.
Existing handler-local runtime evidence is owned by [common scripting](./common-scripting.md);
it does not establish natural execution of this voyage.

`csc42_loadMapEntities` (`mapscriptengine_1.asm`) reads a list's three `mainEntity` words,
then calls `mapfunctions.asm:InitializeMapEntities` and reloads entity sprites. That consumer
clears entities, initializes followers, declares fixed records and declares main entity 0.
Ally records use their ally mapping; an already-mapped ally is skipped rather than placed again.
Ordinary NPC records append to `ENTITY_INDEX_LIST+NEXT_ENTITYDEF` and are referenced from 128.
`GetEntityAddressFromCharacter` resolves through that list, not the physical allocation index.
The three lists at the end of the first script are therefore distinct actor contexts:

| List and command | Main placement | References required by this scene |
| --- | --- | --- |
| `ce_506DE` (`0x506DE`), 4, on Map73 | `(63,63)`, Up | 128 Astral, 129 Minister, 130 King Granseal, 131 merchant/captain, all `(63,63)`; 132 ship `(5,5)`, Right. Sarah, Jaha, Kiwi and Slade use ally references, with fixed records at `(63,63)`. |
| `ce_5072E` (`0x5072E`), 57, on Map14 | `(0,0)`, Up | 128 Minister `(23,30)`, Down; 129-140 are twelve townspeople. The script directs this crowd's facing/animations; it does not retain the voyage ship at reference 132. |
| `ce_5079E` (`0x5079E`), 122, on Map14 | `(40,8)`, Left | Fixed ally records: Sarah `(51,8)` Down, Chester `(41,10)` Down, Jaha `(51,4)` Down, Kazin `(39,8)` Right, Slade `(49,4)` Up, Kiwi `(38,5)` Down. NPCs 128/129 are workers, 130 a young man; 131/132/133 are `MAPSPRITE_OBJECT2` at `(44,6)` Right / `(45,6)` Down / `(46,6)` Left. |

These are authored placements and logical references, not observed final positions. In particular,
F66/F72/F73 can already map Sarah/Chester, Kazin or Slade as followers, causing their fixed records
to be skipped. Both maps admit follower initialization; Map73 is in `data/maps/global/overworldmaps.asm`,
so F65 additionally selects the overworld follower prefix there, whereas Map14 uses the ordinary
list. F74 clear alone does not prove all other follower flags clear or a correct actor cohort.
`eas_Init` and its waits initialize entity behavior; they do not validate force membership.
The script does not normalize `PLAYER_TYPE`; alternate vehicle/raft state remains a counterexample
to assuming an ordinary walking actor. Scene declarations of Kiwi do not set F6.

## Complete first-arrival script

**Confirmed:** `cs_503A6` (`0x503A6`) in `map73/mapsetups/s6_initfunction.asm`, lines 87-271,
contains commands 0-168, including both internal labels `cs_50426` and `cs_50624`.
The ordered phases below retain inline actions, branches and service calls. Dialogue keys
`08A5..08CE` (2213-2254) in `data/scripting/text/gamescript.txt` support the summaries; no
text claim becomes a gameplay predicate. The initial cursor 2239 is replaced with 2213 before
the first text. Both branches later reset the cursor, so skipping Kiwi dialogue does not shift
subsequent text identities.

| Commands | Ordered phase and state effects |
| --- | --- |
| 0-8 | Set cursor 2239; reset ally battle stats; `reloadMap 0,0`; neutral camera; load `ce_506DE`; awaited Bowie `eas_Init`; copy blocks `(0,10)` size `22x10` to `(42,10)`; request `MUSIC_TOWN`; fade in. Reload uses the current Map73 and view operands `(0,0)`, not a new main-entity placement or init dispatch. |
| 9-20 | Cursor 2213; Astral, Bowie, Sarah, Minister and Jaha discuss escape and the mainland. Call `sub_5036E`; narration sends the ship east; wait 120; Sarah feels sick. At 20, F6 clear jumps to `cs_50426` (29). |
| 21-28 | F6-set-only dialogue: Kiwi tries to get Sarah's attention and spots land; Sarah tells Astral, who addresses the King. F6 clear skips all eight lines, including the King exchange. No join or flag write occurs. |
| 29-43 | Both paths reset cursor 2229; Astral and the Minister direct the captain to land despite damage, with timber reuse stated in dialogue. Call `sub_50380`; wait 240 then 205; quake 1; call `sub_5038E`; wait 48; call `sub_5039E`. |
| 44-55 | Await ship reference 132's custom action: speed `(1,1)`, jump to idle. Start its unjoined Right 1 action list; Slade, Sarah and the captain react to impact. Quake 0; `waitIdle 132`; wait 30; `stopEntity 132`; wait 30; reset cursor 2239; arrival narration. The later idle wait explicitly joins that earlier movement. |
| 56-90 | `loadMapFadeIn MAP_SHIP_DAMAGED,18,25`; load `ce_5072E`; awaited Bowie init; fade in. For each reference 129-140, wait 5 then install `eas_2xRightLeft` without a join. Minister 128 faces Left / waits 60 / Right / waits 60 / Left / waits 30, then calls for attention. |
| 91-120 | Wait 20; face 129-133 Right, 134-135 Up, 136-138 Left; wait 20; face 139-140 Down. Minister proposes building a new Granseal and orders ship timber moved ashore. Install `eas_Walking` on 129-140 without joins, then wait 80. This is an instruction and crowd staging, not completed construction. |
| 121-131 | Load Map14 again with view operands `(45,2)`; load `ce_5079E`; awaited Bowie init. Save block `(51,7)` to `(63,63)`, then copy `(51,6)` to `(51,7)`, each `1x1`. Stop animations of 131,132,133. F6 set jumps to `cs_50624` (131); F6 clear executes `hide ALLY_KIWI` first. Both then fade in. |
| 132-147 | Await Jaha Down 1; he objects to dismantling the ship, Sarah faces Up and asks for help with a plank. Await Jaha Down 2; face Down, use `MAPSPRITE_POSE1`, wait 60; effort dialogue. Await `eas_DeactivateAutoFacing`, then custom speed `(64,64)` and jump to idle. |
| 148-157 | Restore the saved `1x1` from `(63,63)` to `(51,7)`; request `SFX_FALLING`; await Jaha Up 4; quake 5; request `SFX_DOOR_OPEN`; wait 10; hide Jaha; copy `(56,0)` size `1x2` to `(51,3)`; quake 0; wait 30. These are block/sprite operations, not a death, departure from the force or inventory transfer. |
| 158-168 | Slade comments; await Slade Right 2; face Up; wait 60; Sarah comments; wait 20; camera destination `(34,4)`; Kazin asks Bowie to bring the plank above deck. Set priority Bowie to 0 and reference 132 to `$FFFF`; end. No player-controlled carrying, warp request, story-flag write or second init is appended. |

The four called subroutines are in the same source immediately before `cs_503A6`:
`sub_5036E` increments layer-2 autoscroll X eleven times, each followed by `Sleep(4)`;
`sub_50380` writes plane-B pixel X `$1200` and Y `$3F60`; `sub_5038E` decrements layer-2
autoscroll X eleven times, each followed by `WaitForVInt`; `sub_5039E` writes that autoscroll
byte to zero. Both loop counts follow DBF initialized to `$A`, not ten iterations. These
assignments do not measure ship displacement or visible scrolling duration.

The named action definitions in `data/scripting/entity/eas_actions.asm` and `eas_main.asm`
keep the crowd behavior distinct: `eas_2xRightLeft` sets Right/Left twice, updates sprites and
waits 30 after each facing before idle; `eas_Walking` enters a repeated random-walk action.
A wait 80 after installing the latter is not a join or proof of timber delivery.
`csc14`, `csc15`, `csc16` and `csc2D` in `mapscriptengine_1.asm` wait on an entity's script
address becoming `eas_Idle` where requested. Their completion depends on entity updates.
`stopEntity` maps to `csc1C_stopEntityAnim`, writing animation counter -1; it is not a general
motion-completion barrier. `hide` calls `entityfunctions_2.asm:HideEntity`: move the entity and destination off-map,
invalidate matching entity-index mappings and remove a matching exploration follower entry.
It does not change combat HP or force/battle-party membership.

`csc34_setBlocks` copies working-map blocks and marks view planes for update. The temporary
`(63,63)` block is a map-layout staging cell, not an item slot. Final `csc53_setPriority`
(`0x46FBE`) resolves each current entity and writes `byte_FFAFB0[entity]` as 0 for operand zero,
1 for any nonzero operand. Thus the final writes concern Bowie and the **middle object**
from `ce_5079E`, not the old ship or a townsman. The source macro's layer names do not prove
rendered overlap; exact appearance and the interpretation of the three objects remain bounded
by the dialogue/staging inference.

## In-script loading, caller return and first control read

**Confirmed:** `mapscriptengine_1.asm:csc46_reloadMap` reloads the current map with d1=-1.
`csc37_loadMapAndFadeIn` starts fade-out and falls through `csc48_loadMap`: load requested
tilesets, wait for fading, call `LoadMap` with map/view operands, restore display/interrupt
processing. The later explicit `fadeInB` commands call the fade-in service. In
`code/common/maps/mapload.asm:LoadMap`, the explicit new-map branch writes `CURRENT_MAP`,
loads blocks/layout and properties; the current-map reload branch reuses current-map data.
Neither command calls MainLoop's substitution/encounter scan, selects the destination's setup
population, clears setup temporary flags, nor invokes `RunMapSetupInitFunction`. Entity
construction is the separately following `loadMapEntities` command.

Both Map14 loads therefore occur inside the original Map73 init call. Map14's flag-copy and
step-copy tables are terminators only. The second explicit Map14 load reloads its layout before
the script's temporary block copies. The Map14 default init (`map14/mapsetups/s6_initfunction.asm`)
has an F703-controlled `cs_58FA4` call and later EGRESS_MAP write, but this in-script loading path
**does not run it**. Those effects must not be attributed to arrival merely because Map14 is current.
No Map14 setup population replaces `ce_5079E` on this return.

At `csc_end`, `mapscriptengine_2.asm:ExecuteMapScript/loc_47234` conditionally waits for view
scroll if a dialogue window remains, clears view-scrolling speed and restores registers.
Control returns to `ms_map73_InitFunction`, which sets F700 and returns. Restoring CPU registers
does not restore `CURRENT_MAP` or the former entity population. `RunMapSetupInitFunction`
(`mapsetupsfunctions_1.asm`) returns to the existing exploration caller, now on Map14.
That caller compares base/current palette entries and conditionally calls map music/fade-in,
then `SetMoveSfx` and `WaitForEvent` (`0x2591C`). The script itself requests Town music and the
two named SFX; it does not establish the final audible state or service completion.

The first `WaitForEvent` read of `MAP_EVENT_TYPE` is the stopping boundary:

- If zero, it sets the camera target to main entity 0 and calls `SetControlledEntityActScript`
  for that entity. `entityfunctions_2.asm` selects `eas_ControlledCharacter` only for
  `PLAYER_TYPE=0`; other player types take its vehicle branches. Its secondary loop then reads
  the pending event again before testing A/C input.
- If nonzero, it skips camera/control installation and enters that secondary event read. A
  surviving nonzero event returns for `ProcessMapEvent`, which clears the word and dispatches it.
  No movement-ready frame is proven in this case.

The entry's earlier clear and the lack of an explicit warp in `cs_503A6` do not prove that this
later read is zero: VInt-driven entity/services can run during waits. F700's source comment says
control is regained, but the write precedes this conditional consumer. Bowie's final authored
placement is `(40,8)`, Left; no subsequent command moves Bowie in this script. The actual state
at the read, motion of other actors, and all wait termination remain unobserved.

Future map event lookups belong to Map14. `data/maps/entries.asm:Map14` supplies its data tables;
`GetCurrentMapSetup` reads `CURRENT_MAP` for each setup service. Map14 selects default unless
F458 selects `ms_map14_flag458`; these have their own entity/zone event pointers in
`pointertable.asm` and `pointertable_458.asm`, with shared area-description/item pointers.
This identifies the next owners, not their natural invocation or a new Map14 init call.
Player actions with the plank, subsequent warps and Battle08 admission remain outside this document.

## Coverage and gap disposition

Fixture paths below are under `tests/fixtures/h2/`; narrow command names have prefix
`uv run sf2 h2`. Coverage is reused evidence, not a new execution of every rail.

| Claim / existing owner | Reused surface | Missing relation / disposition |
| --- | --- | --- |
| Map73 request, substitution and no encounter | `map-event-request-consumption-static-v1.json`, `common-maps-static-v1.json`, `battle-global-data-static-v1.json`; commands `map-event-request-consumption`, `common-maps`, `battle-global-data`; [request consumption](./map-event-request-consumption.md), [battle global data](./battle-global-data.md). | Separate consumer/table coverage does not prove request survival or natural arrival. Reuse these owners if a caller composition is later required. |
| Setup selector and init return | `map-setup-static-v1.json`, `map-init-static-v1.json`, primary body 79 and its two script calls; commands `map-setup`, `map-init`; [map inventory](./map-data-inventory.md). Retained init comparison passes. | F700 set is a distinct no-first-scene path; F508/F705 select a later owner. F700 is written before conditional control. No new selector fixture is needed. |
| Scene commands, state reset, branches and entity rebuilds | `map-script-engine-static-v1.json`, retained `cs_503A6` 0-168; `map-entities-static-v1.json`; commands `map-script-engine`, `map-entities`; [common scripting](./common-scripting.md). Retained script comparison and direct source/text/inline-list review pass. | Separate command/handler facts do not establish correct follower mappings, final status, completed waits or visible/audio output for this caller. F6 clear/set and pre-existing ally follower mappings are controls. |
| Map14 load and event owners | `map-content-static-v1.json`, `map-setup-static-v1.json`, `map-events/routing-setup.json`; commands `map-content`, `map-setup`, `map-events`; [map content](./map-content.md). | In-script load is not ordinary entry/init. F458 changes subsequent event lookup without changing the just-loaded scripted population. Later interactions are deferred. |
| Map73 init → Map14 population → caller F700 → pending/control read | **Reuse/extend existing owners** `map-init`, `map-script-engine`, `map-entities` and `map-event-request-consumption` only if executable composition becomes necessary. | Concrete missing relation: actual caller/entity/pending state at this first read. Minimum public-safe result would bind caller, input F700/F6/follower flags/player type, script branch, current map, entity mappings, F700 write and pending/control outcome. Controls: F700 set, F6 absent, follower conflicts, nonzero pending event, alternate player type. No standalone scene fixture or comparison platform is warranted here. |
| Natural arrival, motion, presentation, services and player plank progression | **Deferred runtime Unknown**, preserving predecessor reachability and pending/service limitations. | Any later observation needs its own bounded question and entry/consumer. Neither these Unknowns nor the available later script constitute an automatic emulator or acquisition queue. |

## Reproduction and review

Bounded source/fixture reads and in-memory selections reuse two JSON payloads totaling
18,285,979 bytes (about 18.3 MB), without rewriting them. Comparisons are linear in their retained
inputs, using one process with an estimated under-512-MiB peak and one-minute comparison budget.
Outputs are this prose and small ignored review/planner receipts. No new bulk collection,
layout decode, installation, transcript/roster export or model/API processing batch is required.
Replan if investigation must pass the first control/event read or require new acquisition.

Load `. ./local/private-inputs.ps1` in the owning checkout and run the exact
[retained-comparison command](./post-tower-return-through-earthquake-entry.md#reproduction-and-review).
It compares all map-init expected data and invokes the existing map-script-engine fixture verifier;
both pass here. Check the selected read-only upstream with `git -C $upstream rev-parse HEAD`
and `git -C $upstream status --short`; `$upstream` is a local selection, never a published
machine path. Read the relative paths and labels above. Select `primarySourceBodies` entry
`sourceOwnerSymbol=ms_map73_InitFunction` and `programCorpus.programs[id=cs_503A6]`; compare
all command source lines, macros and operands with the pinned source, then read the inline
bodies and `ce_506DE/ce_5072E/ce_5079E` lists. Resolve both internal branch targets, the four
subroutines and dialogue cursor resets against keys `08A5..08CE` in `gamescript.txt`.

Record direct prose/reference/link/private checks, `git diff --check`, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD`, actual final-head CI and any completed
failures in the Issue/PR handoff. Documentation-only scope selects no local normal/full/engine/
H3/H4/Godot suite. Preserve existing ignored inputs, results and failures; leave a clean pushed
Draft PR and stop the writer. Main-gate owns acceptance/merge; cleanup and the next Issue are
not part of this slice.
