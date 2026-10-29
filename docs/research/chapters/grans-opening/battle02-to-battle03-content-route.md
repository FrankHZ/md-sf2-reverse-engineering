# Battle 02 outcome to source Battle 03 admission

Evidence date: 2026-10-12. **Confirmed:** the pinned source connects a conditional Battle02
victory return on Map66 to a Map66 zone-event caller that can unlock source Battle03 and return
to `MainLoop` for admission. The unlock is not written by Battle02's after-program or by the
Yeel/Kazin conversation. **Inferred:** Yeel and its offer of guidance belong to the intended
journey toward Hawel. **Unknown:** the naturally executed walking route, whether that visit precedes
the encounter, and the actual Battle02 return or Battle03 initialized state. Numerical order is
not used as evidence of chronology.

## Agreement and provenance

This is the Standard-rigor, bounded cross-owner research agreement for
[Issue #696](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/696), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
The sole tracked write is this document. It consumes accepted main
`9e295ffa11fc40d63261f022675588f8f3c206b9` and the
[Battle01-to-Battle02 route](./battle01-to-battle02-content-route.md), accepted in
[PR #693](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/693).
That predecessor establishes conditional Battle02 admission, not its natural caller state,
initialized roster or outcome. No unmerged synthesis or remake behavior supplies original evidence.

The deliverable is a transition/content handoff and evidence-gap disposition. It stops at source
Battle03 admission and its selected entry dependencies; Battle03 resolution, Hawel-house events,
and later encounters are outside scope. No new fixture, parser, engine behavior, campaign extractor,
emulator run or runtime-closure queue is authorized. Acceptance is direct source/fixture/verifier
review, explicit predicates and Unknowns, document/link/private-boundary checks, committed planner
selection and actual CI, followed by a clean pushed Draft PR for independent main-gate review.

All source paths in this document are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM` at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The inspected checkout is clean at that pin. The registered US retail ROM passed identity verification:
SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Source symbols and addresses below identify static consumers, not newly observed execution.
`Fnnn` denotes a flag. `BATTLE_TO_YEEL = 2`, `BATTLE_TO_HAWEL_HOUSE = 3`,
`MAP_YEEL = 5`, and `MAP_OVERWORLD_GRANSEAL_KINGDOM = 66` are pinned `sf2enums.asm` identities.

## Conditional transition and content table

**Confirmed** here means the named static operand, branch or consumer relation. Mandatory means
required on that selected branch, not an assertion that a player naturally reached it.

| Stage | Predicate, effect and safe next endpoint | Evidence and boundary |
| --- | --- | --- |
| Starting Battle02 contract | Map66 with F402 set can select battle row 2, area `(16,19,22,18)`, wildcard trigger X/Y. The ordinary new-battle path requires F88 clear. | **Confirmed:** predecessor route and `code/common/maps/getbattle.asm`, `CheckBattle` (`0x799C`). F502 clear/F452 clear describe first entry, not all callers. This investigation does not invent a winning input sequence. |
| Outcome selection | After action death processing, and again after after-turn effects/death processing, `CountRemainingCombatants` counts placed living units. Zero allies takes defeat before zero enemies can take victory. Bowie at zero HP forces the ally result to zero. Battle02's enemy-leader flag is zero, so the defeated wrapper does not use its leader-present tail to kill the remaining enemies. | **Confirmed:** `code/gameflow/battle/battleloop_1.asm`, `battleloop/countremainingcombatants.asm` (`0x23C58`), `cutscenes/battleendcutscenesend.asm`, and `data/battles/global/enemyleaderpresence.asm` row 2. This is the all-remaining-counted-enemies outcome rule, not simply killing enemy slot 128. |
| Victory return preparation | `BattleLoop_Victory` heals living/immortal allies and saves Map66, the first battle-party member's combatant X/Y plus battle-area origin, and that member's entity facing into the map-event parameters; parameter 1 is zero. | **Confirmed:** `code/gameflow/battle/battleloop_2.asm` (`0x23CBA`). It calls `UpdateForceAndGetFirstBattlePartyMemberIndex` before reading the coordinates. This is a recipe from final caller state, not a fixed return coordinate or evidence of a settled field endpoint. |
| After-program and join | With F502 clear, row 2 of `rpt_AfterBattleCutscenes` dispatches `ms_Empty`; with F502 set, it skips the script. Both paths reach `EndAfterBattleCutscene`, read join row 2 (zero), and call `JoinForce(0)`. | **Confirmed:** `code/gameflow/battle/cutscenes/afterbattlecutscenesstart.asm` / `afterbattlecutscenesend.asm`; `data/battles/cutscenes/afterbattlecutscenes.asm` slot `0x47CF8`, `afterbattlejoins.asm` slot `0x47D6C`. Zero is the Bowie index consumed by `JoinForce`, not a no-call sentinel. No additional story recruit is named by that row. |
| Victory flags and field handoff | After the wrapper returns, victory clears F402, sets F502, loads the saved return parameters into `d0`–`d3`, and returns `d4=1`. `MainLoop` resumes at `alt_MainLoopEntry`, runs `SwitchMap`, then calls `ExplorationLoop` without rechecking battles first. | **Confirmed:** `battleloop_2.asm`; `code/gameflow/mainloop.asm` (`0x75C4`, return site `0x75E0`). No Map66 override exists in `data/maps/global/flagswitchedmaps.asm`. F403 is not written by this victory sequence. |
| Map66 field state | `ms_map66` is its only setup; its authored ordinary entity list is empty and init is RTS. Exploration with a supplied map clears battle mode and initializes map entities/followers, then loads the map and invokes init. | **Confirmed:** Map66 `mapsetups/pointertable.asm`, `s1_entities.asm`, `s6_initfunction.asm`; `data/maps/mapsetups.asm`; `code/gameflow/exploration/explorationfunctions_2.asm`. An empty ordinary list does not imply no leader/followers, and is not the battle roster. |
| Zone dispatch | Controlled movement must inspect a target with masked zone marker `$1400`; the event consumer passes the requested X/Y to `RunMapSetupZoneEvent`. Map66 checks wildcard-X rows at Y=9, 10, then 28 before its default. The first two select `Map66_ZoneEvent0` (F537 set/F538 clear writes F438); the third selects `Map66_ZoneEvent2` (F538 set/F539 clear writes F439). Each successful branch requests a reload. They do not fall through to the default when their flag gates return immediately. | **Confirmed:** `code/common/scripting/entity/entityscriptengine_2.asm`, `loc_5294`; `ProcessMapEventType6_ZoneEvent`; `code/common/scripting/map/mapsetupsfunctions_1.asm`, `RunMapSetupZoneEvent` (`0x4751A`); Map66 `s3_zoneevents.asm`. Matching an arbitrary ordinary tile to the default table row is insufficient. |
| First Battle03 unlock | At a default-selected zone event, `Map66_DefaultZoneEvent` loads `d0=3` and calls `CheckRandomBattle`. F503 clear selects the request path without testing `STEP_COUNTER` or drawing RNG: set F403, write longword `$000100FF` at `MAP_EVENT_TYPE`, set `STEP_COUNTER=30000`, wait for scrolling, request boost sound and flash. | **Confirmed:** caller `0x4FAD4`/call `0x4FAD8`, helper `0x47856..0x478C6`; reproduced `sf2-map-event-random-battle-state-static-v1`, caller `map66-default-battle3`. Neither F502, F72 nor a Yeel visit is read by this helper. Presentation and natural entry remain **Unknown**. |
| Request to admission | After the zone handler returns, the pending type-1 request is polled and processed. Parameter 1 is zero and parameter 2 is `MAP_CURRENT`, so the warp consumer exits exploration to `MainLoop`. With current Map66 and F402 clear, `CheckBattle` selects unlocked row 3, writes area `(0,0,22,30)`, and returns index 3 to `BattleLoop`. | **Confirmed conditional consumer composition:** `sf2const.asm` map-event byte layout; `WaitForEvent`, `ProcessMapEvent`, `ProcessMapEventType1_Warp` in `explorationfunctions_2.asm`; `mainloop.asm`; `getbattle.asm`; battle-map row 3 at `0x7A4B`. This is the primary stopping boundary. Remaining parameter values are not a new coordinate snapshot. |

The first-entry composition assumes F503 clear and the successful return of the selected services;
F453/F88 clear additionally selects the new-battle intro dependencies below. It inherits F402 clear
from the investigated victory. `CheckBattle` scans in table order: if an alternate caller has F402
set, row 2 wins on Map66 even when F502 is set. It clears F402 for the completed row but still
returns index 2 on that call. Likewise F403/F503 both set clears F403 but still returns index 3
when that row is selected. Area bounds are outputs, not admission predicates; both rows use
wildcard triggers. A F403-clear call can continue to other unlocked Map66 rows, not necessarily
return no battle. This note does not assume later rows are unreachable in arbitrary flag states.

The long request write covers the type word and parameter bytes 1/2 only (`0xFFA84A..0xFFA84D`).
The consumer still uses retained parameter 3/4 bytes as signed coordinate overrides and parameter 5
as facing. Thus the static same-map/battle selection is clear, but a claim about preserved exact
position or facing would require the caller's parameter state. `CheckBattle` resolves `MAP_CURRENT`
from `CURRENT_MAP`; its wildcard row makes that coordinate uncertainty immaterial to row 3 selection.

The empty-program claim is consumer-backed: `code/common/scripting/map/ms_empty.asm` contains the
`$FFFF` terminator; `ExecuteMapScript` in `mapscriptengine_2.asm` reads it and branches directly to
its termination tail (`loc_47234`). That tail can wait for scrolling and clears scrolling speed;
empty does not mean the interpreter has no effects. It supplies no story-command warp, text, flag
or join. The shared join tail instead calls `JoinForce` in `code/common/stats/battleparty.asm`
(`0x9956`), which sets the joined flag derived from the ally index, updates the force and conditionally
calls `JoinBattleParty` when below capacity. It has no zero-index skip. The built join table is
`data/battles/cutscenes/afterbattlejoins.asm`, not the unused same-named file under `global/`.
These are **Confirmed** source relations, not evidence of a new character joining after Battle02.

Battle02 defeat and escape are separate from the investigated victory return. `BattleLoop_Defeat` calls
`UpdateBattleUnlockedFlag`, restores Bowie's HP, halves gold, calls `GetEgressPositionForBattle`
and returns `d4=-1`; Battle02 is not the hardcoded losable-battle exception. Egress/Angel Wing uses
the same unlock helper and returns `d4=0`. In `code/gameflow/battle/battlefunctions/battlefunctions_1.asm`,
that helper clears the current unlock only if its completion flag was already set. It does not
mark an ordinary failed Battle02 complete. These are **Confirmed** alternate static returns;
actual destination/state and onward walking are **Unknown** here.

## Yeel branch: content without an invented prerequisite

Map66's warp records provide candidate visits, but not legal paths between them. The table includes
`(19,23)` to Yeel `(21,42)`, Up, and `(18,22)` to Yeel `(7,1)`, Down. Yeel's wildcard-X exits at Y=43
and Y=0 return to Map66 `(19,24)` and `(18,21)`, both Down. Map66 also has a separate `(6,5)`
warp to source Map43/Hawel's house `(6,11)`, Up. These are **Confirmed** records in Maps66/05
`6-warp-events.asm`; they do not prove that Hawel's house can be reached before or after Battle03.
The house's script consumers and Battle03 resolution are deliberately not entered.

| Selected content | Actor/text/caller relation | Role and limit |
| --- | --- | --- |
| Early Yeel setup | `ms_map5` declares `ALLY_KAZIN` at `(7,6)`, Down, alongside village residents, animals and a priest. Its init and default zone program return directly. | **Confirmed:** Map05 `pointertable.asm`, `s1_entities.asm`, `s6_initfunction.asm`, `s3_zoneevents.asm`. Later selector flags 650, 530, 28 override the default in that order (last set wins). Assume them clear for this early branch; neither Map5 nor Map66 has a flag-switched-map override. |
| Offer of guidance | The default entity table binds `ALLY_KAZIN`, facing condition Down, to `Map5_EntityEvent0`. Fixed text IDs 784–787 offer to guide the party to his master Hawel and place the house outside the village. The program calls `cs_51B14`, then sets F72. | **Confirmed:** Map05 `s2_entityevents.asm`; actual program in `s2_entityevents_650.asm`, `Map5_EntityEvent0` (`0x51AC2`), script (`0x51B14`). Filename suffix does not mean this program requires F650: the default table calls across that file boundary. Text 786 is the intervening narrative message; do not assign all four messages to a speaker solely from a sprite. |
| Following, not recruitment | `cs_51B14` executes `followEntity ALLY_KAZIN,ALLY_BOWIE,2` and ends. Its caller then sets F72; it does not call `JoinForce` or set F4. | **Confirmed:** exact program/caller order and `ALLY_KAZIN=4`. `data/scripting/entity/followers.asm` binds F72 to Kazin; `InitializeFollowerEntities` in `followersfunctions_1.asm` consumes it. `mapfunctions.asm` initializes followers before setup entries and skips an already indexed ally entry, so the default Kazin placement is not proof of a second live Kazin after reentry. Natural following/persistence remains **Unknown**. |
| Optional local information | Entity event 5 uses IDs 767–769 to describe Hawel and give a northwest direction; event 10 uses 775–777 to report Galam soldiers asking about him. Event 13 reads F72 and chooses 782–783 or 788–789. Event 14 calls the church service. | **Confirmed:** default table bindings and program bodies in `s2_entityevents_650.asm`. These provide grounded local context, not an F403 writer. IDs here are fixed `txt` operands, not a `textCursor` sequence. Service results and rendered speech are **Unknown**. |

F650's alternate pointer table reuses the default init, zone, descriptions and item handler, but
selects a different entity list/table. It omits the explicit Kazin placement and the default Kazin
event binding; a follower can still come from follower state. F530/F28 use later shared handlers and
are outside this early content branch. Map05's persistent F940 church block copy and four door-step
records, and Map66's F508/F611 block copies, further prevent treating warp tuples as a complete
walkability proof. These records are owned by the existing map-content rail; no layout is published.

**Inferred:** the guide conversation and northern exit explain a plausible story-facing route.
**Unknown:** which Map66 marker is naturally reached next after the caller-dependent victory return,
whether the player visits Yeel or talks to Kazin before it, and whether every listed seam can be
connected with legal movement. This investigation therefore confirms the conditional same-map
Battle03 connection while rejecting the stronger claim that Battle02 automatically launches it,
or that Kazin's F72 is an admission requirement. The smallest safe endpoint without a walking proof
is the default-zone request/row-3 admission contract, not a narrated continuous walkthrough.

## Source Battle03 entry dependencies

**Confirmed:** `data/battles/global/battlemapcoords.asm` row 3 is
`[66,0,0,22,30,255,255]`. Slot 3 of `data/battles/spritesets/entries.asm` selects
`BattleSpriteset03` (`0x1B33F4`) from `spritesets/spriteset03.asm`; its header declares 12 ally
placements, 6 enemies, 1 AI region and 0 AI points. These counts are capacity/placement data, not
12 joined or active allies. No full roster or terrain grid is reproduced. `terrainentries.asm`
slot 3 selects `BattleTerrain03`; the later alias of that payload does not establish story order.
The [spriteset](../../battle/battle-spriteset-data.md), [global-data](../../battle/battle-global-data.md) and
[terrain/routing](../../battle/battle-routing-data.md) owners retain the inventory and payload boundaries.

**Confirmed:** before-battle row 3 (`0x47A8E`) selects `bbcs_03` (`0x4980E`) in
`data/battles/entries/battle03/cs_beforebattle.asm`. It executes `csub_49816`, which writes
`EGRESS_MAP=MAP_YEEL`, then ends. There is no dialogue cursor, recruitment or F403 write in this
program. This is an entry-side return-destination assignment, not evidence that Yeel was visited.
`ExecuteBeforeBattleCutscene` tests F453 but does not set it. The start wrapper tests and sets F453
before dispatching row 3 (`0x47B32`) to `ms_Empty`. With F453 already set both dispatches are skipped;
the F88 suspended-battle path also bypasses this normal new-battle preparation. Do not assume every
Battle03 caller receives the EGRESS_MAP write.

`BattleLoop`'s clear-F88 path installs current map/battle, runs the before wrapper, clears the region
flags, heals/initializes combatants, loads the battle and then runs the start wrapper. Existing
[admission](./map3-battle01-admission.md) and [battle-loop](../../battle/battle-loop.md) owners describe this
shared order; their Battle01 bindings do not prove initialized Battle03. Actual party, activation,
first turn, service presentation and natural entry remain **Unknown**. Nothing here accepts a remake
implementation or expands the original runtime frontier.

## Evidence gaps and fixture disposition

Fixture names below are under `tests/fixtures/h2/`, with IDs prefixed `sf2-`. The assessment inspected
the fixture payloads and owning `src/sf2tool/h2/` verifiers as well as the source. It does not treat
an aggregate inventory as execution proof.

Research owners are the [random-battle caller contract](../../world/map-event-random-battle-state.md), [request consumer](../../world/map-event-request-consumption.md), [map content](../../world/map-content.md), [common scripting](../../world/common-scripting.md), [cutscene routing](../../battle/battle-cutscenes.md) and [cutscene data](../../battle/battle-cutscene-data.md).
Map05/Map66 filenames above resolve under `data/maps/entries/map05/` and `map66/`, with setup files under `mapsetups/`; shared battle paths resolve under `code/gameflow/battle/`.

| Claim or gap | Disposition and owner | Exact boundary or discriminating failure |
| --- | --- | --- |
| Counting/outcome order and ordinary return | **Already covered** at shared static scope: `battle-loop-static-v1.json` (`remainingCombatants`), `battle-control-static-v1.json`; commands `h2 battle-loop`, `h2 battle-control`. Battle02 row 2 in `enemyleaderpresence.asm` selects the non-leader rule. | Preserve defeat-before-victory when both counts are zero, placed-X/positive-HP tests and Bowie-dead override. Inventory/representative symbols do not substitute for the selected return-parameter recipe. |
| Battle02 empty after-program, join and return recipe | **Reuse/extend existing owners** if an executable Battle02 specialization is needed: `battle-cutscenes-static-v1.json`, `battle-routing-data-static-v1.json`, `map-script-engine-static-v1.json`, and the shared code provenance in `map3-battle01-victory-return-static-v1.json`. Owning narrow commands are `h2 battle-control` for the return recipe and `h2 battle-cutscenes` / `battle-routing-data` for the selected dispatch/join; reuse `map-script-engine` for termination and keep the historical Battle01 fixture's selected claim unchanged. | Missing composed witness: current battle 2, F502 clear/set, row 2 empty target, both join tails and saved map/position/facing → F402 clear/F502 set → MainLoop field entry. Smallest public result is addresses, selectors, derived flags and ordered edges, not combat or positions from an invented caller. Counterexamples: treating zero join as skip, routing to a Battle01 after-program, or replacing final combatant coordinates with starting placements. |
| Map66 default caller and random helper | **Already covered:** `map-event-random-battle-state-static-v1.json`, `facts.callerSites` entry `map66-default-battle3`, `completionFlagGate`, `stepCounterGate`, `randomCadence`, `requestWriteSequence`; command `h2 map-event-random-battle-state`, reproduced in this slice. | This is the exact caller plus body, not just a reference count. F503 clear bypasses the step/RNG gates. F503 set and nonzero step makes no request; with zero step, first RNG result zero requests, nonzero takes a second draw and stores that result +2 instead. It preserves the caller's d1/d6/d7 on return: internal d1 selection is not a returned success flag. |
| Pending request → Battle03 row selection | **Reuse/extend the existing conditional-admission candidate**, not one new fixture per encounter: predecessor's `CheckBattle` candidate, the random-helper rail, `map-event-request-consumption-static-v1.json`, and `map-setup-static-v1.json`. Owning narrow commands remain `h2 common-maps` for CheckBattle and `h2 map-event-request-consumption` for the consumer join, retaining `h2 map-event-random-battle-state` and `map-setup`; no new command is proposed. | Existing request rail stops at polling/dispatch; the helper rail does not enter the downstream consumer. Smallest extension: helper's long write joined through zero parameter-1 type-1 return, MAP_CURRENT resolution and ordered row-3 selection. Controls: row 2 wins with F402 set; F403/F503 set still returns 3 while clearing F403; coordinates outside area still select 3; non-zone markers or Y=9/10/28 do not invoke this default caller. |
| Map/setup/entity/event/warp content and Kazin | **Already covered** as structure: `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-init-static-v1.json`, `map-content-static-v1.json`; `map-events/zone-programs.json`, `entity-programs.json`, `script-invocation.json`; `map-script-engine-static-v1.json`. Reuse `h2 map-setup`, `map-entities`, `map-init`, `map-content`, `map-events`, `map-script-engine`. | Exact program keys include `Map66_DefaultZoneEvent:326356` and `Map5_EntityEvent0:334530`. Preserve default-to-suffixed-file ownership, script return before F72, follower versus joined state, and last-set selector priority. Do not add per-NPC fixtures or use flag-route topology to prove temporal order. |
| Event-marker/walkability joins | **Reuse/extend map-content and bounded route mechanisms** only if a later content claim requires it. The owning narrow command would be `h2 map-content`, reusing its canonical-map import for working-layout inputs; `h2 map3-battle01-admission` is a route-mechanism precedent, not a Battle02/03 proof. | Needed source/caller/state: a specified returned Map66 position, working layout with relevant F508/F611 copies, selected entities/followers, movement/zone/warp marker consumers and terminal. Minimal public result: reachable terminal predicates/route existence, not a map dump. Counterexamples: a matching warp tuple with no marker, or an earlier specific zone row suppressing the default. No static route run was made here. |
| Battle03 initialized party, entry effects and natural chronology | **Deferred runtime Unknown:** relevant battle-loop, source-script and map consumers above. | Direct source does not give a naturally attained entry flag/party/position snapshot or service completion. No immediate H3 request: any later observation needs separate admission rationale. A static extension cannot close natural walking, presentation or save/load persistence. |

No new standalone H2 fixture is justified for the guide dialogue, individual map, or battle inventory.
The two composition needs above have existing owners; proposals are not implementation authority.
Their minimal expected results and counterexamples define future acceptance only if an actual consumer
needs the claim. Merely retaining Unknowns does not require acquisition.

## Reproduction and completion boundary

Resource use is bounded source/fixture text inspection plus one existing static command. The selected
helper fixture is small; no campaign traversal, extracted map dump or runtime trace is generated.
Existing inventory outputs are retained and were inspected only for selected rows; their historical
runs are not reported as fresh results. Original source, H1 listing, ROM and generated JSON remain
private. No inputs/environments were copied and no installation or emulator was launched.

In the owning worktree, load `local/private-inputs.ps1` in the launching process. `$upstream` denotes
the explicitly selected read-only pinned checkout, not a published machine-specific location.

```powershell
. ./local/private-inputs.ps1
uv run sf2 rom verify
uv run sf2 h2 map-event-random-battle-state --upstream-path $upstream
```

**Confirmed reproduced:** both commands passed on the evidence date. The latter produced ignored
`local/derived/map-event-random-battle-state-static.json`, contract
`sf2-map-event-random-battle-state-static-v1`, SHA-256
`F633C17B998F92CF575E6D235AB76FB49DEC2CD27AEE019385F32F53DD3ABD4D`.
Other owner commands are reproduction routes, not claims of additional runs in this slice.

Before freeze, review this complete document against the exact source/fixture owners, resolve local
links, check the single tracked path/private boundary and `git diff --check`, then run committed
`uv run sf2 verify plan --base origin/main --head HEAD`. The exact Git identity, planner result,
actual CI and stopped-writer handoff belong on Issue/PR. No normal/full/engine/H3/H4 suite is required
for this document-only outcome. Main-gate retains independent acceptance and merge authority.
