# Battle 03 outcome through Hawel's house to source Battle 04 admission

Evidence date: 2026-10-12. **Confirmed:** the pinned source supports a conditional chain from
Battle03 victory back to Map66, a warp into Map43/Hawel's house, its default initialization program,
and an exit warp back to Map66 that can admit source Battle04. The house program recruits Kazin and
writes F404; Battle03's empty after-program does neither. **Inferred:** these connected contents
express the intended next story segment. **Unknown:** a naturally executed walking route, the actual
Battle03 return state, the house caller's entity state, and the initialized Battle04 party. The
source does not establish that Battle03 completion is a mandatory predicate of the house scene.

## Agreement and provenance

This is the Standard-rigor bounded cross-owner research agreement for
[Issue #700](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/700), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
The sole tracked write is this document. The accepted starting base is
`7807b5f3fe880e5bc68de44f4056111cd530c84f`; the
[Battle02-to-Battle03 route](./battle02-to-battle03-content-route.md), accepted in
[PR #698](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/698), supplies conditional
Battle03 admission, not a natural entry snapshot or winning route. Concurrent synthesis and
remake code are not dependencies or evidence about the original.

The result supports the next content handoff. Scope is Battle03 outcome/after/return, directly
connected Map43 content, and Battle04 admission with selected before/start dependencies. It stops
there: Battle04 resolution, imprisonment, later destinations and Battle05 are not investigated.
No new parser, fixture, generic campaign tool, emulator run, engine change or Chinese translation
is part of this agreement. Acceptance is direct source/fixture/verifier review, conditional content
and gap tables, full prose/link/private-boundary checks, committed planner selection and actual CI,
then a clean pushed Draft PR for independent main-gate review.

All original source paths below are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM` at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout was verified clean at that pin. The registered USA retail ROM passed
identity verification: SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Addresses and program identities are static provenance, not observations of natural execution.
`Fnnn` denotes a flag. Pinned `sf2enums.asm` defines `BATTLE_TO_HAWEL_HOUSE=3`,
`BATTLE_AMBUSHED_BY_GALAM_SOLDIERS=4`, `MAP_HAWEL_HOUSE=43`,
`MAP_OVERWORLD_GRANSEAL_KINGDOM=66`, and `ALLY_KAZIN=4`.

## Conditional transition table

**Confirmed** in this table means an operand, branch or consumer relationship in the named source.
Conditions apply to the selected branch; they do not assert a natural caller state.
Map43 and Map66 filenames resolve under `data/maps/entries/map43/` and `map66/` respectively;
setup files are in their `mapsetups/` directories.
Short battle-code paths resolve under `code/gameflow/battle/`; `mapscriptengine_1.asm` and
`mapscriptengine_2.asm` resolve under `code/common/scripting/map/`.

| Stage | Predicate and effect | Source and safe boundary |
| --- | --- | --- |
| Starting Battle03 branch | The predecessor's Map66 default zone caller can set F403 and return a same-map request. With earlier row 2 not unlocked, `CheckBattle` can select row 3. | Accepted predecessor; `code/common/maps/getbattle.asm`, `CheckBattle` (`0x799C`). Battle numbering is not the connection. F88 clear selects ordinary new-battle preparation. |
| Outcome | Remaining counts are checked after action death processing and again after after-turn effects/death processing. Zero allies selects defeat before zero enemies selects victory; Bowie at zero HP forces the ally count to zero. Battle03's enemy-leader-presence row is zero. | `code/gameflow/battle/battleloop_1.asm`, `battleloop/countremainingcombatants.asm` (`0x23C58`), `cutscenes/battleendcutscenesend.asm`, `data/battles/global/enemyleaderpresence.asm` row 3. Counted units have nonnegative X and nonzero HP. Killing slot 128 alone is not the outcome rule. |
| Victory return preparation | Heal living/immortal allies; save current map, the first battle-party member's final combatant X/Y plus battle-area origin, and that member's entity facing in map-event parameters 2–5; write parameter 1 zero. | `code/gameflow/battle/battleloop_2.asm`, `BattleLoop_Victory` (`0x23CBA`), including `UpdateForceAndGetFirstBattlePartyMemberIndex`. This is a final-state recipe, not the initial placement or a fixed return coordinate. |
| After-program and join tail | F503 clear dispatches after row 3 to `ms_Empty`; F503 set skips the script. Both paths read join row 3 (zero) and call `JoinForce(0)`. | `code/gameflow/battle/cutscenes/afterbattlecutscenesstart.asm` / `afterbattlecutscenesend.asm`; `data/battles/cutscenes/afterbattlecutscenes.asm` slot `0x47CFA`, `afterbattlejoins.asm` slot `0x47D6D`. Zero is Bowie, not a no-call sentinel; this tail is not Kazin's recruitment. |
| Return to exploration | Clear F403, set F503, return saved map/position/facing in d0–d3 and d4=1. `MainLoop` enters at `alt_MainLoopEntry`, applies `SwitchMap`, then calls `ExplorationLoop` without first rechecking battles. | `battleloop_2.asm`; `code/gameflow/mainloop.asm` (`0x75C4`, battle return site `0x75E0`). The investigated Map66 caller returns to Map66; no Map66 flag-switched-map override exists. F404 is not written here. |
| Candidate house entry | Map66's warp at `(6,5)` requests Map43 `(6,11)`, Up, no-scroll. A controlled target must actually supply the warp marker and reach `WarpIfSetAtPoint`. | Map66 `6-warp-events.asm`; `code/common/scripting/entity/entityscriptengine_2.asm`, `loc_528A`; `code/gameflow/exploration/exploration.asm`, `WarpIfSetAtPoint`. The tuple does not establish a legal path from the caller-dependent battle return. |
| House setup and scene | With Map43 selected and F612 clear, entity setup is `ms_map43_Entities`; init calls `ms_map43_InitFunction` → `cs_540C0`. With F612 set, the alternate setup has an empty authored entity list and RTS init. | `data/maps/mapsetups.asm`; Map43 `pointertable.asm`, `pointertable_612.asm`, `s1_entities*.asm`, `s6_initfunction.asm`; `RunMapSetupInitFunction` (`0x474FC`). No F503 or F72 test gates this selection or default init call. |
| Recruitment and unlock | Script tail: sad `join` Kazin, final cursor dialogue, `followEntity` Kazin/Bowie, `clearF 72`, `setStoryFlag 4`, termination. After script return, init sets F612 and F650, clears F72 again, writes `EGRESS_MAP=43`, and returns. | Map43 `s6_initfunction.asm`, `cs_540C0` (`0x540C0`) and `ms_map43_InitFunction` (`0x540A6`). Consumers derive F4 for recruitment and F404 for unlock; details below. The program does not issue an exit warp. |
| House exit request | Map43's only warp matches wildcard X, Y=14 and requests Map66 `(6,6)`, Down, no-scroll. A matching producer invocation fills parameters 1–5 and sets event type 1. | Map43 `6-warp-events.asm` (`0xB1FF0`); `WarpIfSetAtPoint`. These coordinates belong to the warp, not Battle03 victory. Walking to its marker after the scene remains Unknown. |
| Battle04 admission | The zero-parameter-1 type-1 consumer exits exploration to `MainLoop`. For Map66, F402/F403 clear and F404 set, `CheckBattle` selects row 4 and supplies area `(0,0,22,30)`; its X/Y triggers are wildcards. | `code/gameflow/exploration/explorationfunctions_2.asm`, `WaitForEvent`, `ProcessMapEvent`, `ProcessMapEventType1_Warp`; `mainloop.asm`; `getbattle.asm`; `data/battles/global/battlemapcoords.asm` row 4 (`0x7A52`). This endpoint is conditional on earlier flags and the returned request. |

Battle03's after-program conclusion comes from the table and interpreter, not an absent
`cs_afterbattle.asm` filename. `code/common/scripting/map/ms_empty.asm` is `$FFFF` at `0x47A4E`;
`ExecuteMapScript` in `mapscriptengine_2.asm` consumes it and reaches `loc_47234`. Termination can
wait for scrolling and clears scrolling speed. It contributes no story text, warp, flag or join
command, but is not an effect-free interpreter call. The shared after wrapper then calls
`JoinForce` in `code/common/stats/battleparty.asm` (`0x9956`), whose zero-index path is ordinary:
set the ally-derived joined flag, update force membership, and call `JoinBattleParty` if below
capacity. The consumed table is under `data/battles/cutscenes/`, not the unused namesake in `global/`.

The separate defeated-enemy wrapper also has an empty Battle03 target: row 3 of
`data/battles/cutscenes/battleendcutscenes.asm` is `ms_Empty`. Its caller in
`code/gameflow/battle/cutscenes/battleendcutscenesstart.asm` requires Bowie alive and enemy slot 128
dead, and skips the script when F503 is set. The zero leader-presence row then skips queuing the
remaining living enemies. This explains why that earlier wrapper supplies neither a house warp
nor a substitute victory rule.

Battle03 defeat and escape are separate alternatives. **Confirmed:** `BattleLoop_Defeat` invokes
`UpdateBattleUnlockedFlag`, restores Bowie, halves gold, obtains the egress position and returns
d4=-1; Battle03 is not the hardcoded losable-battle exception. Egress/Angel Wing uses the same
unlock helper and returns d4=0. In
`code/gameflow/battle/battlefunctions/battlefunctions_1.asm`, the helper clears F403 only when F503
was already set. Neither ordinary failed first Battle03 nor escape marks it complete. Its selected
before-program can set `EGRESS_MAP=MAP_YEEL`, as established by the predecessor, but only on its
unsuspended, unplayed-intro branch. Actual failed/escaped destinations and onward walking remain
**Unknown** here; they are not substituted for the victory branch.

## Hawel-house content and caller state

**Confirmed:** `GetCurrentMapSetup` in `code/common/scripting/map/mapsetupsfunctions_1.asm` selects
the default, then replaces it for each set selector in source order. Map43 has only selector F612.
`RunMapSetupInitFunction` loads the selected init pointer at offset 20 and calls it. On a supplied-map
entry, `ExplorationLoop` initializes entities/followers before loading the map and invoking init.
Neither Map43 nor Map66 has an entry in `data/maps/global/flagswitchedmaps.asm`.

Default Map43 declares Hawel (logical entity 128), then two Galam soldiers (129/130); Hawel starts
at `(63,63)` and the script positions him at `(7,4)`. The entity stream falls through into the
alternate list's terminator. F612's alternate list is empty; this does not imply no leader/followers.
Both setups share default entity/zone handlers that simply return, and share descriptions/item
handling. Local books/well descriptions provide context, not F404 writes (`s2_entityevents.asm`,
`s3_zoneevents.asm`, `s4_descriptions.asm`, `s5_itemevents.asm`).

The default list does **not** declare Kazin. In
`code/common/scripting/map/mapfunctions.asm`, `InitializeMapEntities` calls
`InitializeFollowerEntities` before reading setup entities. The follower table in
`data/scripting/entity/followers.asm` binds F72 to Kazin; its consumer in
`code/common/scripting/map/followersfunctions_1.asm` tests that flag and declares the follower.
The predecessor establishes a Yeel conversation that writes F72 after its follow script, without
recruitment. Thus F72 set supplies a source-backed Kazin presence path. **Unknown:** the naturally
attained F72/entity state on house entry. Absence of an F72 gate in the caller does not prove a visit
without that follower has correct choreography, nor that the Yeel conversation was naturally
mandatory. Being joined is not by itself the F72 follower declaration.

`cs_540C0` is embedded in Map43 `s6_initfunction.asm`, not the separate standalone `map-scripts`
corpus. The summaries below are grounded in actual command operands and matching IDs in
`data/scripting/text/gamescript.txt`, not solely ASM comments. They describe content; rendered speech,
service completion, action timing and natural execution are not newly observed.

| Scene segment | Dialogue and actor binding | Ordered content/effect |
| --- | --- | --- |
| Arrival and intrusion | `textCursor 811`; IDs 811–816 (`0x32B..0x330`), Kazin except soldier 129 at 815. | Establish the house and lack of response, enter, discover Hawel and confront the soldiers. The program repositions Bowie/Kazin, uses `reloadMap 0,4`, then explicit roof/step events. This is init content, not a player entity-interaction trigger. |
| Soldiers depart; Hawel explains | IDs 817–826 (`0x331..0x33A`): Hawel 128, Kazin, and Bowie's 822 whisper. | Scripted soldier movements/hides precede Hawel's account that Galam soldiers demanded his Ground Seal notes. Kazin relays the opening of the seal and Granseal's disaster. Dialogue claims are not independent proof of off-screen events. |
| Warning and death | IDs 827–831 (`0x33B..0x33F`), Hawel/Kazin. | Hawel describes the sealed danger and jewels and gives a final request; the program shivers/stops him and moves him to `(63,63)` with `setPosFlash`. Kazin mourns. It does not write a combatant-death result for Hawel. |
| Shared purpose and recruitment | IDs 832–834 (`0x340..0x342`), Kazin. `join ALLY_KAZIN\|ALLY_SADJOIN` occurs between 833 and 834. | Kazin asks to accompany the party to fulfill Hawel's wish, then points toward the Ancient Shrine. The join consumer separately displays fixed message 446; it does not consume the scene cursor. Following and F72/F404 operations follow 834. |

For ordinary text execution, `csc04_setTextIndex` installs 811, and each selected `nextText` or
`nextSingleText` consumer reads then increments `CUTSCENE_DIALOG_INDEX`: 24 advances cover 811–834.
The two non-closing `nextText` calls are 829 and 832. The scene has no yes/no branch for Kazin's
request. `ExecuteMapScript` initially clears `SKIP_CUTSCENE_TEXT`; its debug/P2-Start skip path can
suppress ordinary text handlers and their cursor increments. These ranges describe the normal text
branch, not every debug execution.

The material command semantics are **Confirmed** by actual consumers:

- `csc08_joinForce` in `mapscriptengine_2.asm` reads `0x8004`, clears bit 15, selects sad join music,
  then calls `JoinForce(4)`. The joined-flag base is zero, so F4 is set; active-party insertion is
  conditional on capacity (12), not guaranteed by the scene entity list. Fixed join message/wait
  behavior is distinct from persistent membership.
- `csc2C_followEntity` in `mapscriptengine_1.asm` resolves entity addresses and calls `AddFollower`.
  It is not `JoinForce` and does not replace F4. Clearing F72 removes the flag used for later
  follower initialization; it is not a force removal. The scene first joins, then follows, then
  clears F72. Naturally displayed following/reentry behavior remains Unknown.
- `csc13_setStoryFlag` in `mapscriptengine_2.asm` reads operand 4, adds
  `BATTLE_UNLOCKED_FLAGS_START=400`, and calls `SetFlag`: F404, not F4 or F504. It neither clears
  earlier unlocked battles nor requests a transition.
- `reloadMap` encodes opcode `$46`; `csc46_reloadMap` in `mapscriptengine_1.asm` calls `LoadMap`
  for the current map with camera operands `(0,4)`. It does not run setup init recursively or
  enqueue the house exit. Later F612/F650/EGRESS_MAP writes belong to the ASM caller after script
  return, not the script's earlier reload.

The tail explains two persistent selectors: F612 selects the revisit house setup; F650 selects
Yeel's alternate setup, subject to that map's later selector priorities in the predecessor. Neither
is a `CheckBattle` requirement for row 4. An externally supplied F612-set, F404-clear caller selects
the RTS house init and does not recreate the missing unlock. No scene-completion, save/load
persistence or reentry observation is claimed.

## Returned request and Battle04 entry dependencies

The warp relation differs from the predecessor's random helper's partial longword request.
**Confirmed:** `warpNoScroll` in `sf2mapmacros.asm` encodes parameter 1 zero. For a matching table
row, `WarpIfSetAtPoint` writes `MAP_EVENT_WARP` to the type word, copies the four type/map/X/Y bytes
to parameters 1–4, copies facing (and padding) at parameter 5, and selects warp sound. The house exit
therefore supplies map 66, X=6, Y=6, Down explicitly. `WaitForEvent` polls the type; `ProcessMapEvent`
clears it before dispatch. `ProcessMapEventType1_Warp`'s zero-parameter-1 branch removes its
exploration return frame and returns map/coordinates to `MainLoop`; nonnegative X/Y override the
current player position. Its nonzero-parameter-1 branch is a scrolling transition that stays in
exploration. The house record uses the former. Table matching still depends on reaching the caller:
controlled movement checks masked marker `$1000` before calling `WarpIfSetAtPoint`.

`CheckBattle` scans in table order. For Map66, unlocked row 2 or row 3 wins before row 4 even if its
completion flag is set: it clears that row's unlock but still returns its index on the same call.
Similarly F404/F504 both set still returns 4 while clearing F404. Area `(0,0,22,30)` is output,
not an in-bounds admission test; row 4's triggers are both 255. Without F404 it can continue to later
unlocked Map66 rows. These counterexamples prevent treating the scene, battle number or completion
flags as a complete chronological rule. Map66's default zone helper can also rearm completed
Battle03 under its step/RNG conditions; the predecessor owns that alternate source, not a new
Battle04 unlock.

Map43's flag-copy table is empty, but step/roof records and scripted block changes still matter to
walking. Map66 has its F508/F611 block-copy dependencies. This slice does not decode a route from
the final battle coordinate to `(6,5)` or from the settled house scene to its Y=14 exit. The safe
result is conditional consumer composition, not a continuous walkthrough or proof that Battle03
victory is required before the house. There is no missing unlock writer in the static chain; natural
caller/route evidence remains **Unknown**.

For row 4 selected on the ordinary F88-clear, F454-clear branch:

| Entry dependency | Confirmed source relation and limit |
| --- | --- |
| Before dispatch | `data/battles/cutscenes/beforebattlecutscenes.asm` slot 4 (`0x47A90`) selects `bbcs_04` (`0x4981E`) in `data/battles/entries/battle04/cs_beforebattle.asm`. The before wrapper tests F454 but does not set it. |
| Before content | `bbcs_04` starts cursor 2311, loads Map66 with camera `(1,5)`, and loads separate scene entity table `ce_4989E`. It includes Bowie/Kazin, named companions, Lemon and soldier sprites. These are staged entities, not the active combat party. IDs 2311–2318 (`0x907..0x90E`) have Kazin confront the soldiers/Lemon about Hawel and Lemon order capture. No join or F404 write occurs here. Requested future action in dialogue is not evidence of a Battle04 outcome. |
| Normal preparation | `BattleLoop` installs map/battle for the before call, restores its supplied map/battle afterward, clears region flags 90–105, heals, initializes allies/enemies, clears AI memory, loads battle, then invokes the start wrapper. Actual party and completed service state are Unknown. |
| Encounter data | `data/battles/spritesets/entries.asm` slot 4 selects `BattleSpriteset04` (`0x1B34DC`) in `spritesets/spriteset04.asm`: 5 ally placements, 8 enemy entries, 3 AI regions, 1 AI point. Placement capacity is not membership. `data/battles/terrainentries.asm` slot 4 aliases `BattleTerrain03`; shared terrain does not make the encounter or story identical. |
| Start and alternatives | `data/battles/cutscenes/battlestartcutscenes.asm` slot 4 (`0x47B34`) is `ms_Empty`. `ExecuteBattleStartCutscene` sets F454 before dispatch; F454 already set skips both before/start programs. F88's suspended path bypasses ordinary initialization and wrappers. Neither alternative necessarily replays the introduction. |

The [battle-loop](./battle-loop.md), [cutscene code](./battle-cutscenes.md),
[cutscene data](./battle-cutscene-data.md), [spriteset](./battle-spriteset-data.md),
[global data](./battle-global-data.md) and [routing/terrain](./battle-routing-data.md) owners retain
shared boundaries. Battle04 combat, resolution and subsequent destinations are outside this document.

## Evidence gaps and fixture disposition

Fixture names are under `tests/fixtures/h2/`; IDs use the `sf2-` prefix. Selected payloads and owning
`src/sf2tool/h2/` verifier functions were reviewed with the source. An inventory hash or representative
symbol is not silently upgraded to a composed consumer proof.

| Claim or gap | Disposition and existing owner | Missing claim or discriminating control |
| --- | --- | --- |
| Battle03 outcome, after and return | **Covered** at shared static scope by `battle-loop-static-v1.json` (`remainingCombatants`), `battle-control-static-v1.json` (`controlFacts`), `battle-cutscenes-static-v1.json` (`cutsceneFacts`). Selected rows/final-state recipe are direct source review here. **Reuse/extend existing owners** for a composed executable witness. Commands: `h2 battle-loop`, `battle-control`, `battle-cutscenes`, `battle-routing-data`. | Missing specialization: battle 3, F503 clear/set, empty target, both join tails, final combatant/entity state → F403 clear/F503 set → field return. Minimum public result: selectors, addresses, ordered state/return relations. Controls: both counts zero means defeat; zero join still calls Bowie; initial placements cannot replace final coordinates. Preserve the existing Battle01 victory fixture's narrower claim. |
| Map43 selector/init/script | **Covered:** `map-init-static-v1.json`, `expected.routeJoins` source orders 93/94; `scriptCallSites` entry `ms_map43_InitFunction:0:cs_540C0`; `primarySourceBodies`, `embeddedPrograms[id=cs_540C0]`. Reproduced `h2 map-init`; its verifier compares the complete semantic result. `map-setup-static-v1.json` / `map-entities-static-v1.json` supply selection/stream structure. | F612 clear calls the scene; F612 set chooses RTS at `0x540BE`. Preserve caller writes after script return, entity-list fallthrough and lack of a Kazin setup entry. This does not prove natural follower presence or script service completion. |
| Dialogue/join/follow/F404 | **Covered:** `map-script-engine-static-v1.json`, `dialogueCommandFacts`, `forceStateCommandFacts.sourceSites`, `storyStateCommandFacts.sourceSites` and handlers; reproduced `h2 map-script-engine`. `cs_540C0` join is command 109, follow 111, clearF 112, setStoryFlag 113. [Common scripting](./common-scripting.md) owns these contracts. | Controls: the high bit selects music, not ally identity; F4/F72/F404 differ; setStoryFlag neither warps nor clears earlier unlocks; skipped dialogue does not advance the normal cursor. Static sites do not establish natural presentation. No per-NPC fixture is needed. |
| Map content/warp producer | **Covered** as structure/consumer policy by `map-content-static-v1.json`, `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-events/zone-programs.json` / `entity-programs.json`. Reuse `h2 map-content`, `map-setup`, `map-entities`, `map-events` if extensions are needed. [Map content](./map-content.md) owns marker/working-layout rules. | A no-scroll warp fills destination parameters; a tuple without the caller's warp marker is not traversal. Empty entities do not erase follower/leader state. Default event stubs do not trigger the init scene on interaction. |
| Unlock → exit → Battle04 | **Reuse/extend the existing conditional-admission candidate** in the predecessor, not a new fixture per battle. Owners: `map-script-engine-static-v1.json`, `map-event-request-consumption-static-v1.json`, `common-maps-static-v1.json`; commands `h2 map-script-engine`, `map-event-request-consumption`, `common-maps`. The [request rail](./map-event-request-consumption.md) stops at local dispatch; common-maps inventory alone does not guard every `CheckBattle` branch. | Missing executable join: Map43 script/caller return with F404 set, matching no-scroll exit → supplied Map66 request → row 4 with F402/F403 clear. Minimum public result: producer/consumer addresses, parameter fields, predicates and row identity. Controls: nonzero parameter 1 stays in exploration; F403 selects row 3 first; F404/F504 selects row 4 while clearing F404; F612 set/F404 clear does not rerun init. No new command/implementation proposed. |
| Walking/natural caller | **Reuse/extend map-content/route mechanisms** only for a needed static reach claim; otherwise **deferred runtime Unknown**. Static command: `h2 map-content`; Battle01 admission is a mechanism precedent, not evidence for these maps. | Need specified Battle03 return state, working layouts, F508/F611 and house roof/step state, entities/F72, movement consumers and terminal. Minimal result: conditional route existence, not layouts. Counterexample: a matching warp on an unreachable/unmarked target. No route run or automatic H3 queue is authorized. |
| Battle04 entry | **Covered** as structure by `battle-cutscenes-static-v1.json`, `battle-routing-data-static-v1.json`, `battle-cutscene-data-static-v1.json`, `battle-spriteset-data-static-v1.json`, `battle-global-data-static-v1.json`, and map-script engine `bbcs_04` sites. **Deferred runtime Unknown:** natural entry, roster, service completion, first turn. | Routing inventory has `routeTargetsParsed=false`; selected target relations here are direct table/source review, not invented fixture fields. Controls: F454 skips intro, F88 bypasses normal preparation, scene entities/placement counts do not prove active party. Stop before resolution. |

No standalone new H2 fixture is justified. Composition proposals extend existing owners; they are
not implementation authority or prerequisites for this content handoff. Natural walking, presentation
and persistent state remain Unknown without blocking the conditional static result or creating an
automatic runtime acquisition request.

## Reproduction and completion boundary

This investigation uses bounded source/fixture text inspection and two existing static commands
chosen for house caller and script-consumer claims. Their ignored outputs total approximately
18.3 MB; no campaign traversal, extracted layouts, runtime trace or new tooling was produced.
Existing small inventory outputs were retained; their historical runs are not fresh verification.
ROM, upstream, H1 material and generated JSON remain private and untracked. No input/environment
copies, installation, emulator or Godot process were needed.

In the owning worktree, load `local/private-inputs.ps1` in the same process. `$upstream` denotes
the selected read-only pinned checkout, not a published machine-specific location.

```powershell
. ./local/private-inputs.ps1
uv run sf2 rom verify
uv run sf2 h2 map-init --upstream-path $upstream
uv run sf2 h2 map-script-engine --upstream-path $upstream
```

**Confirmed reproduced:** all three passed on the evidence date. Generated contracts are
`sf2-map-init-static-v1` at `local/derived/map-init-static.json`, SHA-256
`492C60FAF687F5B6510B505FC3EF7F0482F37B89620A6E9B3AAD67288100A768`, and
`sf2-map-script-engine-static-v1` at `local/derived/map-script-engine-static.json`, SHA-256
`F1531D901EBA300FD3EAD721250BE057B310218224B98223EDBF0EF946BE134F`.
Other commands above identify reproduction/extension owners, not additional runs in this slice.

Before freeze, review full prose, selected source/fixture references, links, exact tracked path and
private boundary; run `git diff --check` and committed
`uv run sf2 verify plan --base origin/main --head HEAD`. Exact Git identity, plan, actual CI and
stopped-writer handoff belong on Issue/PR. No blanket normal/full/engine/H3/H4 suite is required for
this document-only result. Main-gate retains independent acceptance and merge authority.
