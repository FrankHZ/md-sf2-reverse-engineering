# Damaged-ship plank through Battle08 admission

Evidence date: 2026-10-12. **Confirmed:** the default Map14 `(45,5)` zone offers a plank
choice. Accepting requests explicit Map14 `(13,11)`, Down. With F458 and F703 clear, its init
runs the above-deck script, whose second choice repeats until accepted; it then sets F408 and
requests Map72 `(1,1)`, Down. That map's first eligible encounter is Battle08.
**Inferred:** object staging depicts carrying and handing over a plank, then following a
worker to rescue his friend at North Cliff. **Unknown:** natural traversal, actual actor/follower
state, pending-event survival and service/presentation completion. This is conditional source
order, not a continuous runtime journey or remake capability.

## Agreement and provenance

This is the Standard-rigor agreement for
[Issue #732](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/732), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Cross-map requests, setup replacement and caller-return ordering require explicit boundaries.
The sole tracked owner is this document, with no shared-path writes or stacked dependency.
Accepted starting base is `d4eeb21f5a28e45742114c62ee9f01b4b8d31536`, consuming
[voyage through damaged-ship control](./voyage-through-damaged-ship-control.md).
Concurrent design work consumes accepted evidence independently.

Start with that predecessor's scripted Map14 population and conditional control read, not an
ordinary Map14 init. Follow the plank event, above-deck handoff, outward request and Battle08
before/start dispatch. Stop at the battle-start dispatcher's return, before combat, results,
escape or town construction. Same-map stairs and direct entry are counterexamples, not a
pathfinding claim. Other interiors, exhaustive NPC dialogue, later stories, new fixtures,
parsers, platforms, extraction, emulator/engine/UI work and Chinese translation are excluded.
Acceptance is pinned source/text/fixture review, retained comparisons, full document/reference/
link/private/diff checks, a clean committed planner and actual final-head CI, then a frozen
Draft PR for independent main-gate acceptance.

Original paths below are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM` at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The reused read-only checkout is clean at that pin. Retained `map-init` and `map-script-engine`
payloads bind this revision and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`; their existing fixture
comparisons pass. No new ROM verification, extraction or runtime observation is claimed.
Fnnn denotes flag nnn. Short `map14/...` paths have prefix `data/maps/entries/`; bare
map-script/setup filenames have prefix `code/common/scripting/map/`. Command indices are
zero-based retained `programCorpus.programs[id=...]` records, not frames or inline action indices.

## Starting population and plank event

**Confirmed:** the predecessor's `cs_503A6` ends with Map14 current and the `ce_5079E`
population, followed by its Map73 caller setting F700. Neither in-script Map14 load called
Map14 init. The authored main placement is `(40,8)`, Left. NPC references 131/132/133 are
three `MAPSPRITE_OBJECT2` records at `(44,6)` Right / `(45,6)` Down / `(46,6)` Left; final
priority writes concern Bowie and middle object 132. Jaha has been hidden. These are source
placements and operations, not observed arrival state. F66/F72/F73 follower mappings, F6,
player type and the pending value at the first control read retain the predecessor's limits.

The required event producer is `code/common/scripting/entity/entityscriptengine_2.asm`,
`loc_5294`: when the destination working-map block's masked event bits (`& $3C00`) equal
`$1400`, it writes `MAP_EVENT_ZONE_EVENT` (6), then word X/Y parameters derived from entity
destination plus proposed movement divided by `MAP_TILE_SIZE`. It does not consult F257.
This is a movement/event condition, not evidence of legal player access to `(45,5)`.
Collision, occupancy, altered blocks and controlled-entity updates remain inputs. No fresh
layout decode or route search is performed here.

`code/gameflow/exploration/explorationfunctions_2.asm:ProcessMapEvent` clears the pending word
before dispatch. Type 6 applies main-entity init, reads the X/Y words and calls
`mapsetupsfunctions_1.asm:RunMapSetupZoneEvent`. That routine calls `GetCurrentMapSetup`,
selects the first matching coordinates (255 is a wildcard), or the default. After the handler
returns it closes portrait/text windows, waits a VInt and waits for entity 0 to stop.
This tail is a service dependency, not a completed natural observation.

`data/maps/mapsetups.asm` selects Map14 default unless F458 selects `ms_map14_flag458`.
The default `map14/mapsetups/s3_zoneevents.asm` table (`0x58E48`) names exactly `(45,5)`
for `Map14_ZoneEvent0` (`0x58E50`); its default returns. The handler calls `cs_58E5C`, then
sets F257 and returns. Neither it, the script nor the required producer/selector checks F257:
its write is **not** a one-time guard. F458 instead selects `s3_zoneevents_458.asm`, whose
only nondefault row is `(255,15)` calling another script. It does not select the plank choice
at `(45,5)`. Setup selection uses current flags at the service call, not population history.

### Choice and pending request

**Confirmed:** `cs_58E5C` is commands 0-15, source lines 22-47 in the default zone file.
Command 0 selects text 2261 (`08D5`); 1 asks about bringing up the plank; 2 invokes `yesNo`.
`mapscriptengine_2.asm:csc11_promptYesNoForStoryFlow` calls
`code/common/menus/yesnoprompt.asm:YesNoPrompt`, sets F89 for return 0 and clears it for
nonzero, then sleeps 10. The menu starts on Yes; Left selects Yes (0), Right selects No (-1),
A/C confirm the current choice, and B returns -1. Cancel and No therefore share the script
branch. The handler overwrites any supplied F89 instead of reusing an old answer.

| Path / commands | Ordered effect |
| --- | --- |
| No or B cancel: 3 → 15 | F89 clear jumps to `cs_58EB8` and ends. No scripted movement or warp occurs. Caller sets F257 and the zone tail closes text/portrait and waits. A zero subsequent pending read can reinstall control; actual readiness is Unknown. |
| Yes: 4-9 | Hide text; await Bowie Down 1; wait 40; await `eas_DeactivateAutoFacing` separately for 131, 132, 133. The action disables auto-facing and jumps to idle; unreachable instructions after that jump do not add acceleration effects. |
| Yes: 10-15 | Start Bowie, 131 and 132 Down 3 without joins; await 133 Down 3. Write `warp MAP_SHIP_DAMAGED,13,11,DOWN`; end. Waiting for 133 does not independently prove the other three actors completed their motion. |

`csc07_warp` (`mapscriptengine_2.asm`) writes one shared pending slot: word type 1,
parameter 1 zero, then byte map/X/Y/facing. It does not load the map immediately. The script
ends; its caller sets F257; the zone tail returns; a later `WaitForEvent`/`ProcessMapEvent`
must consume the surviving request. The no-scroll type-1 consumer unwinds exploration and
returns the tuple to `code/gameflow/mainloop.asm:MainLoop/@Start` (`0x75C4`). Another writer
can overwrite this slot during intervening services. No queue or guaranteed natural delivery
is inferred from the warp command.

## Explicit Map14 entry versus stairs and repeat entry

**Confirmed:** neither Map14 nor Map72 appears in `data/maps/global/flagswitchedmaps.asm`.
No encounter row names Map14. If the accepted plank request survives, MainLoop enters
`ExplorationLoop` with explicit map 14 and `(13,11)`, Down. Its explicit-map branch sets
current map and not-in-battle, gets the selected setup entities, rebuilds entities, loads sprites,
clears setup flags 256-383 (including just-set F257), and sets F80. The default fixed list is
empty, but follower initialization still occurs. It then loads the map, installs base VInts and
calls the selected init. No player-control handoff intervenes between rebuilding and init.

`map14/6-warp-events.asm` also has these alternative no-scroll records:

| Source coordinate | Request |
| --- | --- |
| `(12,11)`, `(13,11)`, `(14,11)` | `MAP_CURRENT` to `(44,8)`, `(45,8)`, `(46,8)`, Up, respectively. |
| `(44,9)`, `(45,9)`, `(46,9)` | `MAP_CURRENT` to `(12,12)`, `(13,12)`, `(14,12)`, Down, respectively. |
| `(255,20)` | Map72 `(1,1)`, Up; a separate outward route, without a story-flag write in this record. |

For the same-map sentinel, `CheckBattle` resolves current Map14 for its scan, then preserves
the sentinel on the no-battle return. `ExplorationLoop` takes `@MapIndexNotProvided`: update
main-entity properties and declare any raft, instead of rebuilding the setup population or
clearing temporary flags. **Both branches still call `RunMapSetupInitFunction`.** The scripted
request is explicit Map14, not `MAP_CURRENT`; a spawn coordinate matching a warp-table
coordinate is not itself a second automatic warp invocation.

The init predicates, from `pointertable.asm`, `pointertable_458.asm` and their init files:

| Selected state | Init result |
| --- | --- |
| F458 clear, F703 clear | `ms_map14_InitFunction` (`0x58F12`) runs `cs_58FA4`; after return, sets F703, then writes `EGRESS_MAP=14`. It does not test F257, a carried item or entry coordinates. |
| F458 clear, F703 set | Return before the script, F703 write and EGRESS_MAP write. Explicit entry leaves the default empty fixed list plus admitted followers; it does not restore plank actors. |
| F458 set, either F703 value | `ms_map14_flag458_InitFunction` (`0x58F9A`) only returns. Its explicit-entry population is priest 128 at `(45,4)` Down and worker 129 at `(13,16)` Up, plus followers; its alternate zone script is outside this first-handoff scene. |

A same-map stair or direct Map14 entry with F458/F703 clear can select the above-deck scene
without a plank acceptance check. F703 or F458 can suppress/replace that content. These are
conditional counterexamples to mandatory plank completion, not proof of a legal walking
shortcut or naturally attainable arbitrary flags. F703's source comment also mentions a Ribble
scene; the flag is not unique proof that this plank handoff occurred.

## Above-deck script and actor boundary

**Confirmed:** `cs_58FA4` is in `map14/mapsetups/scripts_2.asm`, source lines 4-216,
commands 0-129. There is no generic `scripts.asm` owner for this program. Its `ce_59270`
list declares main `(13,11)`, Down; Jaha `(12,20)`, Slade `(14,20)`, Sarah `(14,21)`,
Kiwi `(14,22)`, Chester `(14,23)`, Kazin `(14,24)`, all Up; and:

| Logical reference | Declaration |
| --- | --- |
| 128 | Merchant `(13,13)`, Up. |
| 129 | Young man `(9,13)`, Left. |
| 130 | Man `(16,13)`, Right. |
| 131 | Worker `(13,20)`, Up. |
| 132 / 133 / 134 | `MAPSPRITE_OBJECT2` at `(12,11)` Right / `(13,11)` Down / `(14,11)` Left. |

`csc42_loadMapEntities` calls `mapfunctions.asm:InitializeMapEntities`: clear entities,
initialize followers, admit fixed declarations and main entity 0. An already mapped ally is
skipped in the fixed list. NPC references resolve through `ENTITY_INDEX_LIST`, not physical
allocation order. Objects are now 132-134, not the previous 131-133. Follower flags/player type
can change ally admission; fixed placements alone do not guarantee the live cohort. This
script has no F6 check: it declares and directs Kiwi even when F6 is clear. Actor commands
do not join Kiwi or assert his combat eligibility.

| Commands | Complete ordered phase |
| --- | --- |
| 0-9 | Cursor 2262; reload current map at view `(8,7)`; load `ce_59270`; await Bowie init; wait 1; priority Bowie=0, man 130=0, middle object 133=nonzero; fade in. Start 129's custom action without a join: wait 1200, relative `(1,0)`, wait destination, relative `(0,1)`, wait destination, wait 600, relative `(0,-7)`, wait destination, idle. |
| 10-27 | Merchant asks someone to take the plank; face him Right/Left/Right with 30 waits. Face man 130 Left; reply; await Left 2; start merchant Left 2 unjoined; await man Up 1 then Left 1. Face merchant Right/man Up; thanks; face man Down; wait 30. |
| 28-43 | Start objects 132/133 Down 1 and await 134 Down 1; handover narration; wait 30; man describes needing timber. Start man and 132/133 Down 7, await 134 Down 7; hide man and all three objects. Await merchant Right 2 then Up 1. |
| 44-58 | Merchant laments Granseal; wait 60; worker 131 calls for help. Await custom speed `(48,48)` then idle; await worker Up 6; await worker init; merchant faces Down; worker nods twice and stops animation; await merchant Down 1; question; wait 30; restart worker animation. |
| 59-68 | Worker describes the missing friend at North Cliff; merchant reacts; face merchant Left/Right/Up with 30 waits; address Bowie. |
| 69-75 | `cs_59122` resets cursor 2275, asks Bowie to investigate and prompts Yes/No. F89 set jumps to `cs_59142` (76). No/cancel shows two replies, then jumps back to 69. There is no refusal return/control handoff. Every retry resets the text cursor and overwrites F89 through the menu handler. |
| 76-94 | Cursor 2278; merchant praises Bowie and assigns the guide; await merchant Left 1, face Right. Cursor 2280; await worker Up 2; thanks and guide dialogue; wait 30. Start worker and Bowie Down 4; await Jaha Up 5; face Bowie Left/Jaha Right; camera destination `(8,10)`; Jaha calls out; wait 30; Sarah comments on his wet appearance. |
| 95-112 | Place Kazin/Chester/Slade/Sarah/Kiwi at X13, Y10/9/8/7/6, Down. Start Kazin Down 4/Right 1/Down 1, Chester Down 5/Right 1, Slade Down 6/Left 1, Sarah Down 7; await Kiwi Down 7. Face Slade/Sarah/Kiwi/Chester Down, Kazin Left, Bowie and worker Up, merchant Down. |
| 113-129 | Sarah/Kazin/worker dialogue about accompanying Bowie; wait 60; neutral camera. Start worker, Bowie, Sarah, Chester, Jaha, Kazin and Slade Down 7, then await Kiwi Down 7. `setStoryFlag 8`; `warp MAP_OVERWORLD_BEDOE_KINGDOM,1,1,DOWN`; end. |

Text operands match `gamescript.txt` keys `08D6..08EF` (2262-2287), including the cursor reset
on each refusal loop. Story interpretation is **Inferred**; operation order and speaker operands
are **Confirmed**. No item-give/remove, force-join/leave or HP assignment implements the
narrated handover or Jaha's wet appearance. `hide` removes scene entities/mappings, not
inventory or combatants. Priority's `csc53` stores 0 for zero and 1 for nonzero in the resolved
entity's priority byte; `$FFFF` is not a signed height. Exact overlap is unobserved.
`stopEntity`/`startEntity` control animation, not health.

There are no `setBlocks` commands in `cs_58FA4`: its reload changes view through `LoadMap`,
not an item container or new setup init. The explicit incoming entry already reloads Map14
layout; earlier below-deck saved/copied blocks are not an inventory representation. Named
action waits join the named entity reaching idle; independent actors, unjoined 129's custom
script, scrolling, text and fading need their own completion evidence. Final Kiwi wait is not
a global motion barrier. `eas_Init` restores speed 32 and action properties after the worker's
speed-48 move; speeds/waits are source operands, not measured elapsed times.

`csc13_setStoryFlag` adds `BATTLE_UNLOCKED_FLAGS_START=400` to 8 and sets **F408**. It
does not set F8, F458 or F508. Next, the warp writes pending Map72; then `csc_end` returns
to Map14 init, which sets **F703 before EGRESS_MAP=14**. Caller writes precede the pending
read and outward consumption, not request production. If the request survives, the nonzero
read skips control installation and type 1 returns it to MainLoop. If overwritten/cleared,
delivery is not established. EGRESS_MAP is a stored destination input, not immediate escape
or an accepted future battle outcome.

## Map72 admission and Battle08 before/start

**Confirmed:** `code/common/maps/getbattle.asm:CheckBattle` (`0x799C`) scans ascending battle
indices after substitution. `data/battles/global/battlemapcoords.asm` row 8 (`0x7A6E`) is Map72,
area `(1,32,32,30)`, trigger `(255,255)`. Rows 0-7 name Maps63,57,66,66,66,16,66,58; none
competes for Map72 regardless of F400-F407. Later Map72 rows 24 and 26 use F424/F426;
neither precedes eligible row 8. With F408 set, `(1,1)` selects Battle08 even outside its
rectangle: this is output geometry, not a containment predicate. Facing is not an admission test.

F508 set makes this selected call clear F408, but it **still returns battle 8**. F408 clear
skips row 8; a later unlocked matching row may win, otherwise there is no battle. F703, F257
and F458 are not tests in CheckBattle. The independent gangplank warp can also admit Battle08
if F408 is already set. This does not prove walking reach or natural history for arbitrary flags.

MainLoop passes d1=8 to `code/gameflow/battle/battleloop_1.asm:BattleLoop` (`0x23A84`), which
clears PLAYER_TYPE and sets F399. With **F88 clear**, it clears seconds, saves incoming
map/battle, writes current Map72/Battle08, installs base VInts and calls the before dispatcher.
`code/gameflow/battle/cutscenes/beforebattlecutscenesstart.asm` checks
`BATTLE_INTRO_CUTSCENE_FLAGS_START + 8 = F458`. Set skips; clear selects before-table row 8
(`0x47A98`) in `data/battles/cutscenes/beforebattlecutscenes.asm`, namely `bbcs_08`.
This before dispatcher does **not** set F458 on return.

### Before-scene program

**Confirmed:** `data/battles/entries/battle08/cs_beforebattle.asm:bbcs_08` (`0x4A952`)
contains commands 0-34, source lines 4-47. `ce_4AA14` declares main `(4,50)`, Right;
Sarah `(4,49)`, Jaha `(4,51)`, Kazin `(3,50)`, Chester `(2,50)`, Kiwi `(1,50)`, all Right.
NPCs are worker 128 `(5,50)` Right, worker 129 `(14,34)` Down, witch 130 and
goblin/green ooze/hunter goblin 131/132/133 initially `(63,63)` Down. Slade has no fixed
record here; a supplied follower mapping is a separate input. Loading the list reinitializes
followers and may skip already mapped ally declarations.

| Commands | Ordered scene |
| --- | --- |
| 0-9 | Cursor 2461; load Map72 with view `(1,46)`; rebuild from `ce_4AA14`; await Bowie init; stop worker 129 animation; await custom motion off, Left orientation, sprite update, idle. F6 set jumps to 8; clear executes `setPos ALLY_KIWI,63,63,DOWN` at 7. Both request Main Theme and fade in. |
| 10-17 | Guide 128 points out the friend; await Right 2, face Up, await Up 1; target camera at 129; dialogue about the prone friend; request Enemy Attack music; camera destination `(7,32)`. |
| 18-30 | Place 131 `(12,35)` Down, wait 20; 132 `(11,35)` Down, wait 20; 133 `(13,35)` Down, wait 30; 130 `(12,37)` Down. Dialogue operands 25-28 are all `$C0,130`, including the question about hurting the worker; retain operands rather than assigning a speaker from narrative intuition. Face 130 Up; attack order at 30. |
| 31-34 | Install `eas_Jump` on 131 and 132 unjoined; await it for 133; end. The action disables auto-facing, requests relative Y -1 / wait 5 / Y +1 / wait 5 / zero displacement, restores auto-facing and jumps to idle. |

Text operands match `gamescript.txt` keys `099D..09A3` (2461-2467). **Inferred:** staging
portrays a prone worker and hostile creatures. Animation/motion suppression does not assign
worker HP or prove death; these NPC sprites are not a combat roster. F6-clear relocation is
not `hide`, does not undo a join and does not prove Kiwi's live mapping. Loading Map72 inside
the before scene does not re-enter MainLoop admission or invoke Map72 setup init.

### Actual start handoff and reentry distinctions

**Confirmed:** after the before dispatcher returns, F88-clear BattleLoop restores its saved
map/battle, writes them again, clears region flags 90-105, heals living/immortal allies,
initializes ally/enemy battle positions, clears AI memory and calls `LoadBattle` with d0=0.
`code/gameflow/battle/battleloop/initializecombatants.asm` uses the updated battle party and
Battle08 spriteset, including HP checks for ally placement; it does not turn the before-scene
NPC list into participants. `code/gameflow/battle/battlefunctions/loadBattle.asm` (`0x25610`)
positions battle entities, loads map/sprites and terrain, installs base VInts, requests map
music and fades in. These are actual calls with unobserved runtime completion.

It then calls `ExecuteBattleStartCutscene` (`0x47AEE`). If F458 is clear, this dispatcher sets
it **before** executing start-table row 8 (`0x47B3C`), whose target is `ms_Empty`. If already
set, it skips that target. First entry therefore reaches an empty start script and sets the
same flag that selects Map14's later setup. Non-suspended reentry with F458 set skips the
before scene but still initializes/loads battle. Neither dispatch recruits anyone. The stopping
boundary is the start dispatcher's return, immediately before `BattleLoop/@Start` calls
`ActivateEnemies`; turns and outcomes are excluded.

F88 set is a separate suspended-battle path: restore saved seconds, clear F88, clear AI memory,
reset camera target, call `LoadBattle`, and jump directly to the individual-turn loop. It bypasses
both before/start dispatchers and ordinary position initialization. This counters a claim that
every BattleLoop entry plays the intro or sets F458 here; validity of a supplied suspended
state is not established. No combat is executed or inferred in this investigation.

## Coverage and gap disposition

Fixture paths below have prefix `tests/fixtures/h2/`; narrow commands have prefix
`uv run sf2 h2`. Inventory describes existing coverage, not fresh full-rail execution.

| Claim / existing owner | Reused surface | Missing relation / disposition |
| --- | --- | --- |
| Plank producer, selector and caller F257 | `map-events/zone-programs.json`, `Map14_ZoneEvent0:364112`; `map-setup-static-v1.json`; `common-scripting-static-v1.json`; commands `map-events`, `map-setup`, `common-scripting`; [common scripting](./common-scripting.md). | **Covered** source operations do not prove a reachable event tile or repeated natural invocation. F257 already set, F458 replacement and No/cancel discriminate paths. |
| Explicit/current-map warp and pending consumer | `map-content-static-v1.json`, `common-maps-static-v1.json`, `map-event-request-consumption-static-v1.json`; [map content](./map-content.md), [request consumption](./map-event-request-consumption.md); commands `map-content`, `common-maps`, `map-event-request-consumption`. | **Covered** separate writers/consumers; actual pending survival and traversal remain **Unknown**. Explicit 14 versus sentinel, nonzero/overwritten pending and independent exit are controls. |
| Map14 init, scene commands and actors | `map-init-static-v1.json` primary bodies 19/20; `map-script-engine-static-v1.json` programs `cs_58E5C`, `cs_58FA4`, `bbcs_08`; `map-entities-static-v1.json`; [map inventory](./map-data-inventory.md); commands `map-init`, `map-script-engine`, `map-entities`. | **Covered** at body/command/handler scope; retained comparisons pass. Inline actions/lists and text were reviewed directly. F703/F458, F6 and follower conflicts distinguish paths; declarations do not prove joined force or completed waits. |
| Battle admission, before/start and roster boundary | `battle-global-data-static-v1.json`, `battle-loop-static-v1.json`, `battle-cutscenes-static-v1.json`, `battle-cutscene-data-static-v1.json`, `battle-routing-data-static-v1.json`, `battle-spriteset-data-static-v1.json`; [battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md), [routing](./battle-routing-data.md), [spritesets](./battle-spriteset-data.md); commands with those basenames. | **Covered** shared dispatcher facts (source comparison passes); exact row targets above are direct source reads, not the routing summary's `routeTargetsParsed=false`. F408-clear, F508-set, later Map72 unlocks, F458-set and F88 resume are controls. No roster or natural combat completion is asserted. |
| Plank request → init choice → F408/request → caller F703/EGRESS_MAP → Battle08 start | **Reuse/extend existing owners** `map-events`, `map-init`, `map-script-engine`, `map-event-request-consumption` and battle dispatch only if executable composition is required. | Missing relation: caller/entity/pending state across returns. Minimum public-safe composition binds initial population/flags/player type, choice results, setup, request writes/consumption, caller-write order, battle and intro/start result. Counterexamples above prevent a fixed happy-path claim. No standalone scene fixture or platform is warranted. |
| Walking, asynchronous service completion and presentation | **Deferred runtime Unknown**, preserving predecessor limits. | Future observation must name entry, actor mappings, completed services and consumer. This gap does not authorize H3, escape replay, broad capture or remake work. |

## Reproduction and review

Bounded source/fixture reads and in-memory selections reuse two JSON payloads totaling
18,285,979 bytes (about 18.3 MB), without rewriting them. Comparisons are linear in retained
input size; one process, estimated under-512-MiB peak and one-minute comparison budget suffice.
Outputs are prose and small ignored review/planner receipts. No bulk collection, layout decode,
transcript/roster publication, installation or model/API batch is needed. Replan if dependencies
exceed the above-deck/Battle08-start handoff or require acquisition.

Load `. ./local/private-inputs.ps1` in the owning checkout. Run the existing
[retained-payload and cutscene-source comparison](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
with locally selected `$upstream`; all three comparisons pass. Verify the checkout using
`git -C $upstream rev-parse HEAD` and `git -C $upstream status --short`. Machine paths remain
private. In the retained script payload select the three IDs above and compare each command's
`sourceLine`, `macro` and `arguments` to pinned source; read intervening inline actions and
both entity lists. Check branch labels `cs_58EB8`, `cs_59122`, `cs_59142`, `cs_4A994`, and
text keys `08D5..08EF` plus `099D..09A3`. Review init bodies 19/20, the named zone program,
coordinate rows and dispatcher tables against source. The selected programs contain
16 + 130 + 35 commands and 34 distinct dialogue IDs; repeated choices add no static text IDs.

Record direct document/source/reference/link/private checks, `git diff --check`, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD`, actual final-head CI and completed
failures in the Issue/PR handoff. Documentation-only scope selects no local normal/full/engine/
H3/H4/Godot suite. Retain ignored inputs/results and freeze the clean pushed Draft PR for
main-gate acceptance; no self-merge, cleanup or next Issue belongs to this task.
