# Map 3 to Battle 01 Research Gap Audit

- Status: **OPEN** — Research evidence mapping; not readiness approval or H4 PASS.
- Evidence review base: accepted `main` `51947b99271bb48bac25400b9057c0d8932eafe2`, tree
  `82ec07aa55b241ad5b70cfbf80d0300873c5ac6f` (2026-09-19).
- ROM: USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
- Source: `ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`.
- Product owner: [ADR 0010](../decisions/0010-map3-battle01-product-acceptance.md), selected
  `1A + 2A + 3A + 4A + 5B + 6A + 7C + 8D + 9A + 10A`.

The current additional observation is [Issue #475's bounded Map19 acquisition](#interactive-acquisition-reached-map19-admission-consumed),
independently accepted by main-gate in [PR #476](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/476).

## Current milestone applicability

[ADR 0010's current amendment](../decisions/0010-map3-battle01-product-acceptance.md#current-acceptance-amendment)
requires original gameplay/state/results, natural route/admission, victory/after-program flow and the
first stable controllable 5B endpoint. Reached scene, dialogue, animation and audio identities,
semantic order, acknowledgement, blocking, completion and input readiness remain required under 8D.
Exact pixels/palettes, original frame cadence/durations, waveform/chip equivalence, cycle clocks and
VInt/DMA/CRAM/VDP chronology are outside this milestone. Their historical Unknowns remain research
facts; they do not require capture APIs, backend work or individual deviation waivers here.

The [readiness ledger](../design/synthesis/map3-battle01-readiness.md) owns Design/H4 composition.
This audit maps evidence to assertions, not an engine roadmap or a requirement to observe every
Unknown. A controlled helper is not a natural route; a static graph is not observed chronology;
a fixture-local comparison is not the continuous H4 run. Remake output cannot establish original
facts. Only accepted `main` evidence may be consumed by a later contract. Map3 aggregate index records remain unassociated with a future scenario contract; this audit adds no index associations. The [map-data owner](map-data-inventory.md) retains the broader source inventory.

The old replay path is disabled. Recovery, capability validation and scenario transport are not
milestone prerequisites. Preserve its ordinal-2 timeout **FAIL**, cleanup failure, missing genuine
ledger/receipt bytes and launch prohibition in the
[capability owner](original-reference-replay-capability.md#current-lineage-hard-stop).
The failed R2b work and #431/#434 completed failures are not erased or rerun by this review.
[ADR 0015](../decisions/0015-original-reference-replay-and-h4-boundary.md) retains stop-loss and
passive-replay constraints. No launch, recovery, budget reset or H4 acceptance is authorized here.

## Accepted evidence and exact field mapping

Pointers below are JSON Pointers relative to the linked fixture. For compactness, **O** means
`/expectedObservation/records/0` in an H3 fixture; **S** means `/static`. Array element fields apply
to each retained element, not a newly invented record. Commands identify the existing evidence
reproducer, not commands run or authorized by this documentation review. All fixtures retain the
ROM/source identity above; their parent hashes guard dependencies, not continuous runtime lineage.

| Rail and existing reproducer | Exact accepted fields / source boundary | Sufficient assertion and retained limit |
| --- | --- | --- |
| R1 [fixture](../../tests/fixtures/h3/map3-admitted-start-v1.json), [owner](map3-admitted-start.md); `uv run sf2 h3 map3-admitted-start --timeout-seconds 180` | `O/handoff` (current/egress map and D0–D4), `O/selectedSetup`, `O/chronology`, `O/programRequest`; `O/scenarioState/playerEntity`, `gold`, `difficultyFlags`, `joinedFlags`, `activeFlags`, `allies`, `rngSeed`, `vintTime`; `S/defaultGuardFlags`, `S/sessionPatches`, `S/harness` | **Confirmed** controlled 1A projection and default setup/init traversal through `WaitForEvent` (`0x2591C`). Ally fields are id/class/level, HP/MP current/max, attack/defense/agility/move, items and spells. Raw entity coordinates are not route tile coordinates. `vintTime/normalization` explicitly says counters were zeroed after the boundary. It is not a natural New/load flow or raw natural time. |
| R2 [fixture](../../tests/fixtures/h3/map3-battle01-natural-route-v1.json), [owner](map3-battle01-natural-route.md); `uv run sf2 h3 map3-battle01-natural-route --timeout-seconds 300` | `O/logicalInputTrace`, `O/mapTransitions`, `O/scriptTrace`, `O/chronology`, `O/openingMap3`, `O/fieldMenu`; `S/route` and `S/functions` | **Confirmed** original control through house/school/Sarah/entity142/zone callbacks to `ExecuteMapScript` (`0x4712C`), A0=`cs_5149A` (`0x5149A`), before its body. The exact input/map/x/y/waypoint rows are retained. Field menu is NotReached on this prefix. No Castle/battle continuation or rendered dialogue is supplied. |
| R2a [fixture](../../tests/fixtures/h3/map3-messenger-acceptance-v1.json), [owner](map3-messenger-acceptance.md); `uv run sf2 h3 map3-messenger-acceptance --timeout-seconds 300` | `O/textIds`, `O/speakerOperands`, `O/promptReturn`, `O/promptFlag89`, `O/joinSelector`, `O/joined`, `O/followers`, `O/guards`, `O/flags`, `O/endpoint`, `O/terminal`; `S/stream`, `S/text`, `S/functions` | **Confirmed** command/text-ID/speaker sequence, original default-zero prompt return, Sarah/Chester joins, follower links, F600/F66/F603 and Map 3 `(43,10)`, Down, follower-ready wait. F603 commit `0x50EE4` and handler return `0x50EE8` precede the wait. Speaker operands remain packed source words (including `0xC001`), not normalized character IDs. The 17 text commands and join text 447 do not prove actual prose/audio consumption. |
| R2b [fixture](../../tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json), [owner](map3-castle-battle-unlock.md); `uv run sf2 h2 map3-castle-battle-unlock` | `S/routeGraph/segments`, `S/zones/zoneAdmissionOrder`, `S/zones/selectedTables`, `S/occupancy`, `S/warps`, `S/retainedWarpJoins`, `S/programs`, `S/flags`, `/sourceContext` | **Confirmed static** legal Maps 3/19/20/21 graph, source programs `cs_51652`, `cs_53104`, `cs_53996`, `cs_52F0C`, `cs_52F40`, `cs_53EF4`, and F401/F256 semantics. The graph's inputs are navigation/interaction topology, not a complete timed recording or observed program completion. Warp destination-facing annotations are static operands, not natural outgoing orientation. |
| R2c [fixture](../../tests/fixtures/h2/map3-battle01-admission-static-v1.json), [owner](map3-battle01-admission.md); `uv run sf2 h2 map3-battle01-admission` | `S/extensionRoute/segments`, `S/warps`, `S/admission/checkBattle`, `mainLoop`, `newBattle`, `firstRound`; `S/cutscenes/beforeBattle`, `battleStart`, `introFlag`, `regionCutscenes`, `spriteset`; `S/loadAndTurnOrder` | **Confirmed static** Map 21→40→57 links, `CheckBattle` F401/F501 selection, `BattleLoop`, `bbcs_01`, `ms_Empty`, `LoadBattle`, and generation before first actor read. Structural program/resource identities and source ordering are sufficient as static comparison inputs. They do not fix natural caller state or a reached actor. |
| R2d [fixture](../../tests/fixtures/h3/map3-battle01-player-ready-v1.json), [owner](map3-battle01-admission.md); `uv run sf2 h3 map3-battle01-player-ready --timeout-seconds 600` | `O/continuity`, `O/admission`, `O/scenario/activeParty`, `O/scenario/combatants`, `O/turnState`, `O/deterministicState`, `O/readiness`, `O/chronology`; `S/bridge`, `S/inputPlan`, `S/functions`, `S/ram` | **Confirmed** post-bridge original traversal and player-ready state at `ControlBattleEntity` `0x22E70`, after `WaitForVInt`, before input read. Combatants include statusEffects/activationBitfield and x/y as well as stats/items/spells. `O/continuity/naturalR2bContinuity` is false: bridge writes flags, guard position and RNG/time. Actor 1 and this seeded order cannot be imposed on natural admission. Returned before/start programs and no pending modal/transfer are bounded readiness facts, not proof that every cue completed. |
| R3a [fixture](../../tests/fixtures/h2/map3-battle01-turn-control-static-v1.json), [owner](map3-battle01-turn-control.md); `uv run sf2 h2 map3-battle01-turn-control` | `/turnOrderConsumer`, `/controlDispatch`, `/playerConstructionHandoff`, `/aiConstructionHandoff`, `/commonActionConstruction`, `/preResolutionHandoff`, `/battle01ControlInputs` | **Confirmed static** actor dispatch, player/AI control and action-construction convergence. Actual actor branch, movement, target, action and RNG results remain **Unknown** for the natural battle. |
| R3b [fixture](../../tests/fixtures/h2/map3-battle01-action-effect-static-v1.json), [owner](map3-battle01-action-effect.md); `uv run sf2 h2 map3-battle01-action-effect` | `/actionEffectSpine/dispatch`, `callerContexts`, `rewardConvergence`, `functionAddresses`; `/retainedBattleActions` | **Confirmed static** `ApplyActionEffect` selectors and reward convergence through `DropEnemyItem`; existing combat/economy contracts supply local rules. No reached hit/damage/status/death/EXP/gold/drop outcome is supplied by this static spine. |
| R3c [fixture](../../tests/fixtures/h2/map3-battle01-action-completion-static-v1.json), [owner](map3-battle01-action-completion.md); `uv run sf2 h2 map3-battle01-action-completion` | `/actionCompletionSpine/primaryTargetLoop`, `followupBranches`, `explosionBackedge`, `endSequence`, `executeIndividualTurnHandoff` | **Confirmed static** primary/follow-up/explosion and action-completion edges. No double/counter/item-break branch is required merely because it exists; close only branches reached by the admitted comparison. |
| R3d [fixture](../../tests/fixtures/h2/map3-battle01-turn-finalization-static-v1.json), [owner](map3-battle01-turn-finalization.md); `uv run sf2 h2 map3-battle01-turn-finalization` | `/turnFinalizationSpine/replayContinuation`, `outerLoop`, `outcomeBoundaries`, `functionAddresses` | **Confirmed static** `InitializeBattlescene`→`ExecuteBattlesceneScript`→`EndBattlescene`, battlefield reload, after-turn outcome gates and `0x23BB2`→`0x23B40` backedge. Replay/completion and later rounds are not observed. |
| R4a [fixture](../../tests/fixtures/h2/map3-battle01-victory-return-static-v1.json), [owner](map3-battle01-victory-return.md); `uv run sf2 h2 map3-battle01-victory-return` | `/victoryReturnSpine/victoryBody`, `afterBattleRouting`, `battle01AfterBattleProgram/operations`, `afterBattleJoin`, `mainLoopReturn`; `/sourceContext/h1RomAnchors` | **Confirmed static** `BattleLoop_Victory` `0x23CBA`, selected `abcs_battle01` `0x496DC`, F401 clear then F501 set, D4=1, MainLoop resume/SwitchMap and unentered `ExplorationLoop` call at `0x75E4`. The 80 source operations are a structural corpus, not 80 observed completions or a 5B state. |

### What the admitted projection does not contain

R1's public `scenarioState/allies` is not a full raw combatant dump: status is omitted,
and its four consecutive item bytes are not four complete two-byte item slots.
`S/defaultGuardFlags` names only six checked guards (1/602/603/506/543/609).
The [R1 source completion](map3-admitted-start.md#static-completion-of-the-admission-projection)
establishes the controlled NewGame→SaveGame→MainLoop→Map3-init→first-wait boundary:

- **Confirmed (static):** status is a word at ally offset 44. For the 30 living
  allies and original start equipment, status at the wait equals status at controlled NewGame
  entry masked with `3` (STUN/POISON). NewGame does not clear status; exploration healing clears
  other conditions and the stat updater recalculates CURSE. No starting equipped item is cursed.
  Existing MOV observations equal class bases, with no starting equipment MOV effect; STUN
  would subtract 1, so STUN is also excluded. **Unknown:** only inherited POISON per ally remains.
  Reset-based zero is a conditional/inference, not a restored-span boolean or a runtime observation.
- **Confirmed (static):** F604/F605/F607/F608/F401/F256 and additional castle selectors
  F501/F507/F982 are clear at R1: the 128-byte settings clear, intervening writers and
  F256–F383 map-local clear are traced in the owner. This does not assign their values at
  R2a or a later castle wait.
- **Confirmed (static):** event type is a word and remains 0 on this initialized idle-player,
  no-script prefix; event parameters are inactive. Map-script A6 is call-local. Window entries
  and dialogue/portrait/timer indices reset before init; empty-window guards exclude
  stale animation storage. The first wait entry precedes installation of the player-controlled
  actscript, so its idle cursor need not be fixed. Active NPC scripts remain live.

The smallest missing R1 readback is 30 POISON bits (mask 2 of each status low byte;
30 byte reads, or 60 bytes through a word getter), in a future independently admitted batch. Exact active-NPC continuation, when
needed, requires its relevant state/phase as well as RNG; this bounded projection is not a
complete save state. Preserve raw entity coordinates, separately observed four-byte RNG, and
explicitly normalized time. Visible New/naming/load remains excluded by controlled 1A.
The DisplayText/name/menu seams below and natural-route/presentation Unknowns remain.
No separate observation or replay recovery is authorized by these deductions.

### Presentation sufficiency under 8D

The [dialogue contract](../design/contracts/dialogue-system.md) supplies command widths, text cursor,
packed speaker operands and DisplayText/portrait/clear call order. The
[standalone program contract](../design/contracts/standalone-map-script-program-data.md) and R2b/R2c/R4a
supply source operation/resource identities. The
[battle scene contract](../design/contracts/battle-scene-presentation.md) supplies scene dispatch and
asset/animation identities; it explicitly excludes treating the HP/EXP replay fixture as rendered
scene evidence. The [audio contract](../design/contracts/audio-system.md) supplies command namespaces
and bounded playback-state evidence, not complete scene-specific playback/loop/fade/resume semantics.
These static identities and accepted bounded rules remain sufficient without new hardware captures.

**Confirmed inspection limit:** R1 `S/sessionPatches` includes `display-text-rts` at decimal 25184
(`0x6260`). R2a and R2d use
[`map3_messenger_acceptance_observer.lua`](../../tools/bizhawk/map3_messenger_acceptance_observer.lua):
`patch_cart` applies the R1 patches during bootstrap; `restore_scope` restores them at finalization.
The messenger text callbacks record command entry, cursor and speaker; they do not restore the text
service or observe its real content consumption. The original YesNoPrompt return is separately
observed and must not be relabelled as injected. R2d's long-cutscene callbacks only refresh liveness.
Consequently the accepted text IDs, program returns and readiness are valid at their declared seams,
but insufficient for unshimmed dialogue acknowledgement or all actual visual/audio completions.

For 8D, a cue assertion needs its source program/operation and resource ID, reached order, dispatch
and consumer completion/acknowledgement boundary, associated state effect, and whether input is
blocked or available before/after it. Persistent music needs observed start/replacement/stop semantics
where reached, not a fictitious “track ended” event. Static waits/returns may establish semantic
ordering, but a service bypass or entry-only callback cannot prove actual delivery. Exact duration,
waveform and hardware register history are unnecessary. 7C private content identity/provenance remains
required; no prose, asset or capture is made public by this mapping. Actual remake host consumption
and continuous start-to-5B state/input observations remain H4 work, separate from original evidence.

The accepted [exploration presentation owner](../../remake/docs/exploration-programs.md) explicitly describes MUSIC_JOIN/MUSIC_SAD_JOIN as project-authored C-major/C-minor chord loops. This is evidence of the product implementation boundary only. An original command ID plus an actual `AudioStreamPlayer` start/fade/stop does not establish a 7C original audio asset. Require the private asset identity/provenance and its binding to the consumed cue separately; generated modern chords do not satisfy that asset assertion. This audit neither inventories other private repositories nor admits an alternative audio source.

## Research Gap Register

This table replaces the earlier generic RA closure demands. “Missing” identifies an assertion not
proved by the mapped evidence; it is not permission to run a scenario.

| RA | Admitted sufficiency | Minimum missing fields/checkpoints and why they matter |
| --- | --- | --- |
| RA-01 | R1 captured 1A fields; source-proved route/active-state conditions above; no visible New/load requirement | Remaining R1 dependency: inherited POISON per ally. Read mask 2 from 30 status low bytes at the first admitted `WaitForEvent` in a future accepted batch, retaining initializer/write provenance. Add active NPC state only if exact continuation is required. No alternate seed, raw flag dump or natural-New campaign is needed. |
| RA-02 | R1 default selector/init call and no guarded program request | Along the selected route: actual setup identity, entry/return of each real init/event program, changed route flags and pending script/event state. Static selector rows cannot fix later caller flags. Observe only the selected path, not every 506/543/609 alternative. |
| RA-03 | R2/R2a prefix; R2b legal graph; R2d post-bridge traversal | R2a→R2b natural control: ordered logical inputs and accepted input reads, map/position/facing, zone/warp identity, program call/return and F604/F605/F607/F608/F401/F256 changes with their callers. These stateful waits/interactions determine whether the route actually reaches admission; static topology and bridge writes cannot prove them. First proposal is bounded below. |
| RA-04 | R2c structure; R2d post-bridge CheckBattle/BattleLoop/before/start/load order | At naturally carried Map57 admission: F401/F501/F88, map/battle/area, before/start program entry/return and F451, transfer/modal state. No new algorithm is needed; the missing assertion is continuity of caller state from the omitted story segment. Reuse R2d's observation fields after that seam is settled. |
| RA-05 | R2d exact controlled combatants/order/readiness | At natural load/generation/first input: party, all participating combatant fields in `O/scenario`, region flags 90–105, actor/order/turn offset, RNG and RNG copy/time, and `O/readiness` fields. Natural values remain **Unknown**; bridge-seeded actor 1/order is not a natural golden. |
| RA-06 | R3a–d plus existing combat/action/after-turn local rules | For reached turns: round/order/actor, input and cancel/commit, movement/target/action selector, AI choice, RNG before/after each relevant draw/effect, per-target HP/MP/status/death, EXP/level/spell/gold/drop/item effects, follow-ups only if reached, action/replay return, after-turn mutations and outcome gate. Local controlled rules do not select a natural winning trace or account for its carried state. Reuse those rules; do not rerun unrelated branch matrices or predetermine a winner by bridge state. |
| RA-07 | R4a selected victory/after-program/return spine | Observe natural victory entry, selected after-program and reached operation completions/effects, join result, F401-clear→F501-set, D4 return, MainLoop/SwitchMap result and ExplorationLoop reentry. Merely entering `abcs_battle01` or reaching R4a's unentered target does not establish completion. |
| RA-08 | FieldMenu static caller graph; R2/R2a NotReached | Record whether selected later route actually calls FieldMenu. If absent, no menu-page matrix is required by 2A; if present, capture that call's logical choice, return and effect. Do not turn optional menu coverage into a milestone gate. |
| RA-09 | R2a text IDs/speaker/prompt chronology and static program/text corpus | On reached programs only: text ID/packed speaker/portrait identity, selected prompt return, cursor advance, actual text consumer/acknowledgement/close and program return order. Original prose stays private. Service shims and static ID lists leave actual consumption **Unknown**. |
| RA-10 | 6A excludes save/load/checkpoint/suspend | Restart-to-admitted-state is a product check. No new persistence observation; cross-process durability remains a separate **Unknown**, and harness reset is not a save feature. |
| RA-11 | Source-backed identities, local semantics, bounded request/return evidence | Reached scene/dialogue/entity-animation/audio cue identities and semantic consumption/completion/blocking/input-ready relations above, with private content provenance. Capture only necessary consumer boundaries not proved statically or by retained observations. Hardware precision is excluded; no complete 8C capture inventory or backend is required. |
| RA-12 | 5B defines endpoint shape only | First stable control after RA-07: map/egress, player position/facing, party and combatant status/stats/items/spells, gold, route/battle flags, RNG/time relevant to continuation; no pending battle/transfer/script/modal; next ordinary logical input is accepted and its state effect observed. Final numeric values and map are **Unknown** here. An ExplorationLoop call alone does not prove controllability. |

RA-07/12 reuse the [victory-return owner](map3-battle01-victory-return.md) and
[battle cutscene routing contract](../design/contracts/battle-cutscene-routing.md); RA-04 uses
[map-entry routing](../design/contracts/map-entry-routing-state.md); RA-06 uses
[combat resolution](../design/contracts/combat-resolution.md) and the R3 retained-owner bindings.
No original observation is required for 9A's modern mappings/accessibility implementation itself;
9A/10A need separately reported H4 state-equivalence, acknowledgement and deviations. Existing
accepted implementation evidence is not original-game evidence.

The conditional Map21 alias135 and Map3 post-F603 cleanup findings remain at
[their source owner](map3-castle-battle-unlock.md): default unassigned index39 is zero only absent
intervening writes; sprite-callee return and natural F401/F256 continuity are not established by
that calculation. Removed142's `0xFF` is different. Its four stores affect inactive combined-window
storage under the documented initialization/consumer guards, not a proved natural return state or a
general missing-entity no-op. These conditional static answers do not independently justify runtime
work. Broader setup-flag meanings, optional interactions, natural secondary-region/later-round state,
and unrelated hardware Unknowns remain with their subsystem owners, not extra mandatory RA rows.

## First necessary original-observation dossier

**Selected question:** from the accepted R2a follower-ready state, does ordinary input reach
`Map3_ZoneEvent4`, complete `cs_51652`, commit F604, and traverse the north warp to a stable first
Map19 wait with the expected route state? This is the earliest missing **natural-route** boundary.
Earlier real text/cue consumption is also missing as explained above; this proposal does not claim
those earlier presentation assertions closed or open a separate capture launch for them.

**Why it is necessary and bounded:** the answer determines the first unsupported 2A transition.
R2b proves the legal path and guard/program shape; R2a stops before it; R2d substitutes a Map21
terminal. None establishes the caller's event admission, asynchronous guard movement completion or
actual F604-before-warp order. A full battle or hardware observation would not be the smallest
question. Do not extend this candidate into royal/tower programs, Map21 unlock, battle or 5B merely
because the later rows remain open.

### Start, input candidate and checkpoints

Use the accepted ROM/source above and R1→R2→R2a dependency projections. R2a's endpoint is a public
state projection, **not an existing portable save-state artifact**. The only implemented way this
rail reaches it rebuilds the controlled prefix in process. No actual full-state file/hash or frozen
movie from this endpoint is supplied here. **Proposed method for review: extend the existing
controlled-H3 continuation**, preserving the admitted prefix and observing the original gate/warp
without its R2d bridge. Main-gate must explicitly decide whether that method is admissible under the
current rules and lineage. If it is rejected, do not silently switch to passive replay: that would
require its own independently admitted start/input artifact and concrete implementation decision.
Neither method follows automatically from 8D.

The candidate navigation is exactly R2b `S/routeGraph/segments/0/inputs` from `(43,10)` to `(31,5)`,
then segment 1's gate event, segment 2's inputs to `(28,1)`, then segment 3's Map3→Map19 warp to
`(26,30)`, Up (static destination operand). Segment 0 has 31 inputs and segment 2 has 7; these are
logical navigation edges, **not** a frozen frame/input schedule. Hold/release frames, acknowledgement
inputs, startup state identity and any timing-dependent carried RNG values are **Unknown**. Source
`textCursor 537` in this gate program sets a cursor; it is not itself a displayed text line.

| Checkpoint | Required observation and terminal condition |
| --- | --- |
| R2a `WaitForEvent` `0x2591C`, after F603 commit/return | Match `O/endpoint`, `O/flags`, `O/joined`, `O/followers`, `O/guards`. Read F604 and the setup-selection guards, current event/script/input state, RNG/time and any admission fields still unresolved statically. F604 must be read, not assigned false from expectation. Freeze the declared controlled setup boundary before continuing. |
| `Map3_ZoneEvent4` `0x50DF8` | Retain map/player coordinates, event identity and F604 at entry. R2b's `S/zones/zoneAdmissionOrder/0` selects `(31,5)`; its source guard orders `chkFlg 604`, `script cs_51652`, `setFlg 604`, return. Fail on a skipped or different branch; no synthetic event publication. |
| `ExecuteMapScript` A0=`cs_51652` `0x51652`, then return to ZoneEvent4 | Record the reached command order, logical aliases 138/139 resolved through current entity mappings, movement start/destination/completion and awaited entity139 completion. Preserve the distinction between non-waited entity138 and waited entity139; script return alone must not manufacture entity138 completion. Confirm the caller F604 write occurs after script return. Exact return/write callback addresses must be derived from the pinned source/H1/ROM before implementation is admitted; this document supplies no invented PCs. |
| Original north warp and first Map19 setup/init return to `WaitForEvent` | Record the actual event/warp operands, map/player/facing, selected setup/init (`ms_map19_InitFunction` `0x530EA`, selected `cs_53104` `0x53104`) and program entry/return, F604 and retained route state, pending transfer/script/modal and input mode. Observe a bounded ordinary input-read/acceptance at that wait. Stop there; Map19 `(26,30)`, Up is a static landing expectation, not a new observed endpoint. |

Source joins are R2b `/sourceContext`, `S/programs/cs_51652`, `S/zones`, `S/retainedWarpJoins` and
`S/routeGraph/segments/0` through `/3`; `Map3_ZoneEvent4` belongs to Map3 setup zone events and
`cs_51652` to Map3 `scripts_1.asm`. Map loading/setup control follows the existing R1/R2 callbacks
and the map-entry contract. Resolve source paths/anchors from those owners at the pinned commit,
not a floating upstream branch. The source program requires no new gameplay algorithm in Lua.

### Rail reuse and actual executability

**Confirmed inspection:** the accepted implementation is
[`map3_messenger_acceptance.py`](../../src/sf2tool/h3/map3_messenger_acceptance.py) and its shared
[observer](../../tools/bizhawk/map3_messenger_acceptance_observer.lua).
[`map3_battle01_player_ready.py`](../../src/sf2tool/h3/map3_battle01_player_ready.py) consumes that same
observer. At follower-ready it either sets `finish_pending`, or requests the extension bridge;
`seed_extension_terminal` writes R2b flags/guard/RNG/time, and the extension injects MAP_EVENT fields.
The source-graph H2 has no runtime observer. **No accepted unchanged rail executes this proposal.**

The smallest reuse candidate keeps the existing R1/R2/R2a guards, one physical-PC callback dispatch,
source-backed entity lookup/readback, typed callback failure/finalization, and
[`bizhawk.py`](../../src/sf2tool/h3/bizhawk.py) process/timeout launch mechanism. It needs a reviewed
continuation after follower-ready in place of stop/bridge, this bounded source/input/checkpoint
configuration, and actual terminal/consumer readbacks. Preserve old fixture cases and projections;
any changed output/schema/case ownership must be granted separately before editing those shared
surfaces. Do not create a new fixture when an extension of this batched rail can answer the question:
ADR 0014 parts 1/2 identify a real missing acceptance fact, but part 3 does **not** justify a separate
one-case fixture here.

At this dossier's initial review, this reuse was **not executable as an admitted observation**.
The observer's controlled bootstrap ROM/RAM/register writes and state-aware joypad policy are not an ADR 0015 passive replay.
Removing the bridge alone does not make it one. A passive method would also need a concrete admitted
start artifact, frozen non-adaptive input transport and removal of continuing shims/writes; neither
is supplied by the existing command. Actual text consumption cannot reuse `DisplayText`-RTS.
A narrower controlled-H3 method must be explicitly classified and accepted with its limitations; it
cannot silently inherit permission from historical R2a. The accepted evidence dossier grants no
execution permission. Issue #456's [preparation owner](map3-messenger-acceptance.md#unshimmed-map3-candidate-preparation-only)
now defines an opt-in candidate in the existing messenger Python/Lua rail: service/scratch restoration
at the R1 wait, explicit frozen diagnostic input, missing status/NPC readbacks and bounded gate/Map19
callbacks. It has no CLI wiring. Its materialization status is
`CANDIDATE-PREPARED-NOT-ADMITTED`, not `PRELAUNCH-PASS`; ordinary R2a/R2d fixtures remain unchanged.
The timed diagnostic input is not an accepted original recording and may fail. Issue #460 classifies
the proposed method as controlled-start H3: declared R1 bootstrap mutations, restoration of original
services/scratch at the first wait, and only original gameplay execution plus frozen input thereafter.
This method classification grants no execution budget. The candidate now retains optional typed
native-process diagnostics, immediate PID and pre-termination timeout recording, actual local-copy
identity, preserved Lua artifacts and independent host cleanup outcomes. Existing ordinary helper
callers retain their semantics. Synthetic no-process error-path checks are not original evidence.

The [preparation owner](map3-messenger-acceptance.md#reproduce-preparation-without-execution) records a
source-visible input-construction defect: candidate-06 inserts C pulses before the trace's entity142
facing/interaction sequence. A separate fixed-input proposal relocates those blocks after interaction,
retaining the same counts and proposed budgets; candidate-06 is preserved. Its reach/timing and menu
risk remain **Unknown**. The initial claim that the gate had no dialogue was an analysis error from
using H2's selected operation subset; the complete pinned `cs_51652` contains six `nextSingleText`
commands. That claim is withdrawn and the gate confirmation block remains unchanged.

Exact material/code review, actual CI and old-lineage/budget disposition still precede any launch.
The implementation-stage stopping condition was a frozen Draft PR with runtime gates explicitly
**NOT RUN**. The separately admitted single diagnostic result is recorded below; all other selected
runtime gates and the old replay path remain unexecuted.

Required private inputs/toolchain are the canonical ROM, pinned upstream/H1 anchors, retained
fixtures, BizHawk 2.11.1 / Genesis Plus GX and the manifest-owned executable/Lua identities. Resolve
immutable inputs using [local-private-inputs](../operations/local-private-inputs.md); keep writable
session/config/state/output/TEMP under the existing owning worktree. A public fixture projection
cannot stand in for missing core state. Before any later launch, verify input identity and exact
start/input/configuration/checkpoint binding, declared time/frame limits, callback/return coverage,
error exit and contained output/cleanup. Exact new candidate limits and runtime availability are
**Unknown**, not inherited as an allowance from an older timeout value.

Failure conditions are input/start/fixture/source drift; unexpected selector/program/warp; missing
or out-of-order entry/return/F604/movement completion; stalled input or program; terminal with pending
work; callback/parser/status failure; timeout/nonzero exit; surviving process; failed unregister,
restoration or cleanup. Return a typed failure with the last valid checkpoint and preserve diagnostic
facts. No forced flags, positions, movement completion, PC advance, longer-budget retry or automatic
continuation may repair a divergence. A successful bounded route result would still leave real cue
consumption outside its instrumented boundaries, the rest of R2b, natural battle results and 5B open.

### Executed diagnostic and remaining boundary

**Confirmed failure, 2026-09-19 project date:** after PR #461 was accepted, main-gate issued the
[exact single controlled-start diagnostic disposition](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/460#issuecomment-5746916798)
under ADR 0015's further-work condition. The accepted execution commit was
`4386c0148362e44d8d9256cf183671832f7c10e2`, tree `3f0aeda35c7715f6e7002baf3f3021b4e78fe7c3`.
The frozen 23,234-frame proposal ran once with its 600-second timeout and 28,634-frame watchdog;
no warmup, old H3 command, repair or retry occurred. This was neither old replay ordinal 3 nor a
new two-plus-one allowance. The specific permission is exhausted.

The diagnostic stopped before the intended gate question: `candidate:warp` rejected the first
opening warp's raw `MAP_CURRENT=255` at `ProcessMapEventType1_Warp` `0x25978`, observer frame 499 /
input frame 145. The source represents that same-map house stair warp with the sentinel; the
candidate prefix check allowed only literal map 3. This is a confirmed observer/source mismatch,
not an observed original route failure. Handler completion, messenger/gate/F604/north warp and
Map19 control were not observed. The
[result owner](map3-messenger-acceptance.md#single-admitted-controlled-start-diagnostic-result)
retains exact code/material identities, source attribution, reproduction route and limitations.

The native process exited 1 without timeout; Lua reported zero remaining callbacks and restored
scope/session state, the host recorded deleted session ROM and unchanged canonical ROM, and an
independent process check found no owned PID/runtime-copy survivor. The diagnostic is **FAIL** and
has no terminal observation, even though its failure and cleanup diagnostics were retained.
The runtime/status/checkpoints and earlier failures remain private and preserved. No extra launch
follows this result. The separately scoped Issue #463
[zero-launch correction](map3-messenger-acceptance.md#map_current-correction-and-bounded-warp-admission)
now distinguishes raw/effective map only for source-bound no-scroll warps, reuses R2 source/target
coordinates for house and both school stair transitions, and retains gate/F604/north/Map19 checks.
It verifies pinned source/equate/ROM constraints and read-only admission; it does not observe warp
completion. The fixed 23,234-frame input and attempted material remain unchanged. The original
caller-dependent gate transition,
later fixed-input reach and natural continuity remain **Unknown**; no public golden, full 8D or H4
is promoted. Further action returns to independent main-gate disposition, with no remaining launch
inferred from this failed diagnostic.

### Corrected candidate diagnostic and exhausted permission

**Confirmed failure, 2026-09-19 project date:**
[Issue #465](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/465) admitted exactly one
further diagnostic after independent acceptance of PR #464's MAP_CURRENT correction. Execution used
accepted commit `07dce82ac1f157e626521549aa30b62c0a064165`, tree
`dff51195c746619ba4953c73a2dc71a255367399`, the retained corrected candidate and unchanged
23,234-frame input / 28,634-frame watchdog / 600-second hard timeout. All retained material identities
were checked before the sole API call. This was controlled-method actual start **2**, distinct from
the disabled replay's ordinal 1/2; its exact permission is now exhausted and grants no third start.
The old PID 42464 / frame 499 / input 145 / exit 1 failure and cleanup remain intact.

This run passed the first raw-255 house warp admission and recorded original controller readback at
Map 3 `(3,3)`, Right, frame 643 / input 289, then opening `cs_5145C` return at frame 1167 / input 813.
It failed at original `FieldMenu` entry `0x2127E`, frame **1749 / input 1395**, after the frozen
house-exit block's C at inputs 1393–1394. The last checkpoint is `field-menu:reached`, Map 3 `(4,4)`,
F601 set and F603/F604 clear; there are zero tracked pending returns. This confirms that this
particular trace reaches the menu, not that the intended original route is impossible. No menu
selection/effect, school warp, messenger/R2a boundary, gate/F604/north warp or Map19 control was
observed. RA-08's accepted R2/R2a NotReached projection remains limited to its own prefix;
this separate failed diagnostic is not a replacement golden or a reason to require optional menu
coverage. Source-only school/north destinations remain expectations, not reached endpoints.

PID **26448** exited **1**, without timeout or tree-kill. Lua reports zero callbacks and all recorded
restoration flags true; host session deletion/canonical-identity checks succeeded. Independent OS
inspection found no owned PID/runtime-copy survivor, so no extra cleanup was required. The
[corrected result owner](map3-messenger-acceptance.md#corrected-candidate-single-run-result) retains
exact material identities, bounded checkpoint/exit/finalization details and read-only reproduction.
All old/new private artifacts remain preserved. No repair, rescheduling, re-materialization,
warmup or second call followed. The intended 2A gate question, natural continuity, full 8D and H4
remain **Unknown**; #437 is not complete. Only result-document/scope/planner/public-CI acceptance
follows this failure, with no automatic further original-runtime action.

### Lineage decision before implementation or launch

This proposal targets stateful gate movement/F604/warp semantics and reuses the earlier batched
observer, rather than the disabled replay capability's fixed movie/capture backend. That technical
difference does **not** establish a new budget: it overlaps the failed R2b route and inherits its
provenance. The later accepted static graph and the explicit bridge's success do not erase the failed
natural work or prove a corrected natural runner. Any further execution requires main-gate to
independently decide whether additional accepted static evidence or a separately corrected capability
changes admission, and exactly how prior R2b/replay and the completed diagnostic consumption constrain
it. The exhausted dispositions above and the separately admitted #475 result below do not reopen the
disabled replay path or establish any further allowance.

The [capability failure owner](original-reference-replay-capability.md) and
[scenario boundary](original-reference-replay-scenario-api.md) retain actual failure details and
missing receipts. Ordinal 2 cannot be presumed unused; its reported FAIL is not the same-candidate
PASS required by the disabled path for ordinal 3. No nominal reset, fourth launch, renamed runner
or task creates allowance. Recovery of that disabled path is not this proposal's milestone
prerequisite; an explicit lineage/budget disposition is. No additional observer/fixture/launch is admitted
by this dossier. Main-gate independently accepted the bounded #475 observation below in
[PR #476](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/476); no automatic runtime execution follows.

### Interactive acquisition reached Map19; admission consumed

**Confirmed bounded original observation, independently accepted by main-gate in
[PR #476](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/476):** after PR #474 was independently accepted,
[Issue #475](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/475) explicitly admitted
one `run_map3_observation_candidate(..., interactive=True)` call and at most one native start from
accepted `9cb14a55c2777b515a18450e98fc37ca90350292`, tree `63659d8c0d16f662279727dc1301f641ef0a3d4c`.
Only retained #473 prepared-02 was used, after full identity/ROM/process checks and a pre-call note;
no reprepare, old frozen-input execution, smoke or retry occurred. This was interactive controlled-R1
acquisition with ordinary controller inputs after service/scratch restoration. It was not passive
reset/New/load replay. That sole admission is exhausted: historical controlled total **3**, while
old replay ordinal 1/2 restrictions and all #460/#465/#471 failures/preparations remain intact.

The [result owner](map3-messenger-acceptance.md#single-admitted-interactive-acquisition-result) retains
exact material/log identities, source/PC/frame/order joins, analysis failures and read-only reproduction.
The immutable acquisition-runner result remains `OBSERVATION-COMPLETE-UNREVIEWED`, describing the
collector output before independent review. Main-gate acceptance now applies to the bounded original
observation at first source-bound Map19 movement acceptance; the label is not a replay/H4 PASS:

| Dossier question | Reached boundary and remaining scope |
| --- | --- |
| R1 inherited state | All 30 status words were 0 in this attempt; live entities/index, RNG and unnormalized raw time were retained. This does not establish universal/reset-start POISON absence. |
| Unshimmed opening and messenger | House/Sarah/Astral consumers returned; messenger `cs_5149A` ran through real text consumers and YesNoPrompt D0=0. Follower-ready frame 8515 observed F600/F66/F603 true, F604 false at `(43,10)`, Down, with no pending consumers. No FieldMenu callback occurred. |
| Caller-dependent gate | `cs_51652` entry 8903; its six texts completed, script return and original F604 false→true trap followed in strict order at 10032. Guard readbacks are retained without inventing additional movement-completion observations. |
| North warp / first Map19 control | Original north handler 10197 selected Map19; init and `cs_53104` entry/return 10259, first wait 10285, original Up movement acceptance `0x52E8` at 10329 / emulator 10328. Map19 `(26,30)`, event word 0, F604 true, no pending script/text/prompt/init/close consumers. The frame-end position did not yet move to the next tile. |
| Evidence and cleanup | 10329 complete frames including 355 bootstrap, 200 explicit batches, 582 checkpoints; all ordered command/result/input records agree with the host receipt. PID 39748 exit 0, elapsed 1778.575/1800 seconds, no timeout/forced termination, callbacks and declared scope restored, session ROM deleted and canonical unchanged; independent OS found no survivors. |

The 2A gate/F604/warp question now has one bounded original observation independently accepted
by main-gate in [PR #476](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/476). It does not retroactively repair the failed frozen diagnostic or update accepted fixture
projections. Source mapping rows elsewhere in this dossier retain their original evidence scopes;
this result is a separate observation, with no index/schema/golden/counter changes. The final log's
outer emulator clock is restored to 214 after cleanup while its acquired frame-end snapshot remains
10329; preserve both, as the result owner explains. Native EOF/abort and partial-batch termination
were not exercised. Full downstream castle/tower/battle/5B continuity, complete 8D and H4 remain
**Unknown** and #437 remains incomplete. No second invocation, fourth start, new replay, engine/code
repair, merge or retained-environment cleanup follows this result.

## Map19 continuation admission proposal (Issue #479; not authorized)

This **static proposal**, reviewed against accepted `origin/main`
`1a37c2825746db0f04f6adccd1d125dc5b792cb5`, tree
`d8c511f37427fac2ce2d3cd1e3278b567c3c2ddb` on 2026-09-20, recommends one future
interactive controlled-R1 traversal to the first natural Battle01 player-ready seam. It does not
admit implementation, preparation or execution. [Issue #479](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/479)
is related to #437; completing this proposal does not close the milestone. The
[accepted readiness boundary](../design/synthesis/map3-battle01-readiness.md#conditional-runtime-questions)
and ADRs [0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md),
[0015](../decisions/0015-original-reference-replay-and-h4-boundary.md) and
[0016](../decisions/0016-remake-start-evidence-deferral.md) govern any later admission.

[Issue #483](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/483) subsequently authorizes
implementation and offline preparation of this selection only. The
[capability owner](map3-messenger-acceptance.md#natural-battle01-continuation-capability-offline-only)
records the implementation, direct checks and remaining native Unknowns. Its prepared limits are
reviewable configuration, **not runtime admission**. Actual controlled starts remain three; no
natural continuation, first actor/readiness, winning trace or H4 result is supplied by that work.
The recommendation below continues to own a later runtime decision.

### Actual start and retained-artifact limit

**Confirmed by bounded read-only inspection:** `local/issue473/prepared-02/runtime/` retains
`actual-inputs.jsonl`, `checkpoints.jsonl`, `observer.observed.json`, `observer.status.txt`,
`host-status.json`, `bridge/receipt.json`, the runtime copy and its SaveRAM. Its `Genesis/State`
directory is empty; no saved emulator state was found in that runtime or `local/issue475/` or
`local/root476-review/`. The latter two contain analysis/handoff material and the independent result
check, not a continuation state. This is an inventory of these named roots, not a machine-wide search.
No private payload or input was modified. The retained host result says session ROM deleted,
canonical unchanged, PID 39748 exited 0, no timeout/forced termination; the observation reports all
declared restoration fields true. Current process inspection found no EmuHawk or Research collector.

The [observer](../../tools/bizhawk/map3_messenger_acceptance_observer.lua) keeps `saved_state` from
`memorysavestate.savecorestate` only in memory at bootstrap; `restore_scope` loads it on finalization.
It does not serialize the acquired Map19 core. The acquired callback/frame-end projection and
completed input trace cannot reconstruct CPU/core/NPC state. SaveRAM is persistent save data, not
that execution state, and has not been loaded or admitted as a start. The immutable collector label
remains `OBSERVATION-COMPLETE-UNREVIEWED`; PR #476 independently accepted only the bounded facts.

| Candidate start | Availability and consequence |
| --- | --- |
| Continue the #475 process at Map19 | Unavailable: process exited and core restored. The last accepted Up input did not establish next-tile displacement. |
| Load a genuine retained Map19 state | None found in the named evidence roots. A future state would need actual acquisition, identity, lineage and a separately reviewed save/load boundary; there is none to authorize or use here. |
| Fresh interactive controlled-R1 traversal | The existing bootstrap is reusable after a separately accepted capability extension. Restore services/scratch at the first `WaitForEvent`; thereafter only explicit ordinary controller inputs advance the original. Reacquire the prefix within the same attempt before the missing continuation. This is controlled 1A, not natural New/load. |
| Replay #475's frozen prefix, or synthesize Map19 from its projection/R2d bridge | Frozen replay is a separate method/determinism/admission question under ADR 0015, not an optimization automatically granted by the recording. Synthesis/bridge writes cannot establish the missing natural continuity. Neither is recommended for this slice. |

### Selected route and necessary reached assertions

Reuse [R2b](map3-castle-battle-unlock.md) and its
[static fixture](../../tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json)
`static/routeGraph/segments`, `zones/zoneAdmissionOrder`, `programs`, `flags` and `warps`, then
[R2c/R2d](map3-battle01-admission.md) and the
[admission fixture](../../tests/fixtures/h2/map3-battle01-admission-static-v1.json)
`static/extensionRoute`, `admission`, `cutscenes` and `loadAndTurnOrder`.
The pinned source/ROM identities at the top of this audit apply. The following are **Confirmed
static** relationships; their natural reached caller state and completion are **Unknown**.
Navigation rows are legal topology, not timed inputs or guaranteed NPC clearance.

| Segment / existing source owner | Proposed observation and acceptance reason |
| --- | --- |
| Reacquired R1→first Map19 control | Reuse #475's real consumer, prompt, F604, north-warp and init-return checkpoints. Retain this attempt's POISON, entity/index, party, gold, RNG/copy and raw-time readback; do not copy #475's values. Keep its acquired Map19 checkpoint as an intermediate event, then observe actual movement beyond `(26,30)` (selected next tile `(26,29)`). This supplies the missing displacement and a single continuous lineage for 2A. |
| Map19 royal route→Map20 palace | R2b segment `map19-entry-to-royal-warp` crosses `(29,15)` and `(25,13)` through `Map19_DefaultZoneEvent` direct returns, then warp `(23,3)`→Map20 `(23,37)`. Record the actual zone/warp operands, selected setup and init entry/return. `ms_map20_InitFunction` gates the royal entry on raw entity position `0x22803780` and F605 clear; `cs_53996` returns before its caller sets F605. Observe its text/actscript/entity waits and returned caller; source post-program player `(23,39)`, Down is a comparison, not an injected terminal. Also read F507 to exclude the later init branch. |
| Royal return→Astral | Map20 `(23,37)`→Map19 `(23,3)`; LEFT is the static warp operand, not a substitute for observed facing. `ms_map19_InitFunction` hides alias140 via `cs_53104` when F605 is clear or F608 is set; with F605 set/F608 clear the return makes Astral available. Record setup, both flag reads and the actual init path, then selected `(16,6)`/Up interaction with alias140. |
| `Map19_EntityEvent12` at `0x52EF2` | With F607 clear, `cs_52F0C` (`0x52F0C`) prompts and branches to `cs_52F40` (`0x52F40`) only on F89 set. Observe actual prompt input/return/F89; do not transfer the messenger's D0=0 expectation. Acceptance executes waited alias140 movement, hides it and sets F608; the caller sets F607 after the script returns. F607 alone is not agreement. The selected minimal route accepts on this first interaction; decline/repeat (`cs_52F24`) is an explicit off-route stop, not an automatic retry or permission to force F89. |
| West tower→Map21 guard | Cross Map19 `(16,5)` default-zone return; warp `(6,2)`→Map20 `(6,37)`, then `(3,36)`→Map21 `(3,16)`. Observe each setup/init/warp return and map-local F256 at the guard caller. At `(4,16)` facing Right, alias128's `Map21_EntityEvent0` needs F608 set/F256 clear for `cs_53EF4` (`0x53EF4`). Record its awaited guard `(5,16)`→`(6,16)` action, actual alias135 lookup/facing target, script F401 write, script return, then caller F256 write. [The unchecked lookup owner](map3-castle-battle-unlock.md#map-21-entity-135-unchecked-lookup-and-conditional-continuation) already explains the conditional slot0 effect; observe index byte 39 and the reached target/return, not a new missing-entity algorithm. The selected Map21 `(5,15)`/Down wait is a checkpoint, not a state to seed. |
| Map21→40→57 | Reuse R2c's 46 logical-input topology: Map21 `(9,1)`→Map40 `(4,30)`, then selected event cell `(14,12)`→Map57 `(8,18)`. Wildcard `y=12` does not make all that row valid warp events. At `CheckBattle` (`0x799C`), use incoming D0 map, because `CURRENT_MAP` may still hold the source; record F401/F501, selected area `(0,0,16,20)`, returned battle index and `BattleLoop` arguments. |
| Natural admission/load/first dispatch | Record F88 and the new-battle branch, `bbcs_01` (`0x494BC`) entry/return, `LoadBattle` (`0x25610`) entry/return, `ms_Empty` start selection/F451 and return, F90–105 reset, activation/region/spawn/generation order and actual first actor dispatch. The before program's `loadMapFadeIn MAP_ANCIENT_TOWER_ENTRANCE,2,10`, `ce_49694` entity population and white fades are script-owned scene changes, not extra player warps. Preserve their waits/returns and the later Map57 reload. R2d supplies structural/readback fields; its seeded RNG/order/actor are never natural goldens. |

For each stateful checkpoint retain source PC/program/operation, callback order, actual completed
input frames, map/position/facing, relevant flag before/after and caller return. Reuse R1/R2d readers
for party/joined/active flags, gold, ally class/level, HP/MP, stats, status, full item/spell slots,
and RNG/RNG-copy/raw time at R1, Map19, post-royal, guard return, pre-load, post-load/generation and
ready. These joins distinguish carried accounting from battle initialization/healing and random AGI
draws. Preserve observed changes and source causes, not an assumed unchanged inventory or fixed seed.
Record participating combatants' position/activation/status and turn order/offset at load and ready.
No optional dialogue, shop, chest, church or alternate branch campaign is necessary for this 2A path.

For reached 8D assertions, extend the existing real `DisplayText`, `CloseDialogueWindow`, prompt,
acknowledgement and stack-matched program returns. Bind each cue to its program operation/resource,
blocked/available input relation and source completion predicate: waited entity/actscript idle,
camera/fade completion or returned blocking consumer as applicable. Non-waited actions may remain
live; a whole script return cannot assert every animation finished. R2d's long-scene liveness hooks
are not completion observations. Track only reached relevant cues, using accepted dialogue, program,
entity/camera/screen and [audio](../design/contracts/audio-system.md) contracts; no hardware clock,
frame/audio capture platform or screenshot is needed. For persistent music, observe selected command
and source consumer dispatch, replacement/fade/stop when actually reached (including map/battle
music selection); do not wait for or invent a track-end event. If no stop is reached, retain active
music at terminal. Mailbox consumption alone does not prove audible output or all fade/resume
semantics. **7C original audio asset identity/provenance remains independently open**, even if these
command observations succeed; modern authored chords do not close it.

### Minimum capability extension before any runtime decision

The implementation inspection is read-only. Reuse
[`prepare_map3_observation_candidate` / `run_map3_observation_candidate`](../../src/sf2tool/h3/map3_messenger_acceptance.py),
the shared observer's `install_candidate`, `returned`, `sample`, `add_callback`, frame logging and
`finalize_success`/failure restoration, and
[`DebugBridge`](../../src/sf2tool/bizhawk_debug_bridge.py)'s serial connection, identity, explicit
`step/state/ping/abort`, process receipt and containment. The
[bridge composition owner](../operations/bizhawk-debug-bridge.md#map3-interactive-composition-offline-only)
already owns transport. No new transport, automation service, input-policy engine or state authority
is justified.

Concrete extension points are necessary because the present candidate rejects programs beyond its
Map3/gate/`cs_53104` set, rejects warps after first Map19, and sets `finish_pending` at first Map19
movement acceptance. Its init-return booleans are specific to first entry. Replace only the selected
continuation's terminal/phase predicates with the source-bound table above, preserving Map19 as an
intermediate checkpoint and distinguishing repeated init calls by stack and selected setup/flags.
Bind the additional callbacks to pinned source/H1/ROM through existing preparation checks. Extend
pending-consumer tracking for reached waited actions/camera/fades and flag-caller returns; do not
convert liveness into completion or silently allow arbitrary programs.

Reuse the R2d reader shapes in
[`map3_battle01_player_ready.py`](../../src/sf2tool/h3/map3_battle01_player_ready.py)'s
`_static_contract`/`_observer_config` and Lua `extension_combatant`/`capture_extension_result`, but
separate their read-only fields from the bridge requirements. Enabling `config.extension` wholesale
is wrong: `seed_extension_terminal` writes flags, map/event/guard state, RNG and time, while existing
`extension-check-battle` requires `extension_bridge_seeded`; result continuity is explicitly false
and deterministic state contains a seeded block. Candidate callback registration also filters out
ordinary `extension-*` roles. Integrate the source-bound reads under candidate dispatch, preserving
shared-PC dispatch/error handling and recording acquired values instead of satisfying those bridge
guards. Read the full word `MAP_EVENT_TYPE`, not only R2d's low-byte readiness projection. Derive
modal/transfer/consumer readiness from actual state; do not copy its constant `cutsceneOrMenuModal=false`.

The runner hardcodes 1800 seconds in preparation, run validation and `DebugBridge.start`, as well as
28634 frames/2048 batches/120 frames per batch. Any new bounds require an explicit reviewed candidate
configuration and consistent host/Lua enforcement. Existing candidate watchdog coverage is not a
new-route progress proof. Add the bounded idle/progress accounting below to this same mechanism;
no helper-test project, fixture duplication, aggregate gate, input generation or native dry run is
part of #479. Future capability ownership must declare its exact source/observer/bridge paths and
any required documentation before editing; this proposal owns only this audit.

### Recommended endpoint, failure and preserved scope

Proposed success is the **first natural player-ready callback at `0x22E70`**, within
`ControlBattleEntity` (`0x22E1A`), after its `WaitForVInt` and before its input read. Require original
before/start/load/generation returns, Map57/Battle1/area, F401 set/F501 clear/F451 set, actual region
flags, ally actor matching the current turn/moving/view target, neutral current input, no targeting
or committed battle action, event **word** zero, no pending script/text/prompt/transfer/modal and no
unfulfilled blocking presentation consumer. Capture callback-time state and the completed frame
separately. This bridge stops at frame end, not by suspending a CPU instruction: retain any later
callbacks/state changes in that same neutral frame, skip the rest of the requested batch, and do not
call the frame-end snapshot a before-input sample.

Do not seed or demand actor1, order or RNG `0x1234`. To keep this admission-only proposal bounded,
stop at a non-player first `ExecuteIndividualTurn` dispatch or any action-resolution entry before
player-ready with an explicit **out-of-scope-before-player-ready** result. That is a coverage stop,
not evidence that the original's legal AI turn is wrong. Record actual order/actor/state; no forced
ally, hidden combat execution or retry. Whether the natural first dispatch permits the proposed
success remains **Unknown**. Extending through intervening actions would require separate R3a–d
observation coverage and review, not a runtime fallback.

Other first stops are callback/source-identity/order/readback failure, unexpected setup/program/
warp/FieldMenu or selected prompt decline, premature battle/defeat/victory, exhausted wall/frame/
batch/idle/progress limit, malformed input, disconnect or abort. Preserve distinct typed reasons,
last source checkpoint and actual delivered frames; a limit is an incomplete observation, not proof
that a game wait cannot complete. Any restoration/callback/process/session-deletion failure defeats
an otherwise reached endpoint. Keep `OBSERVATION-COMPLETE-UNREVIEWED` on a successful collector
result until independent Research review; never emit replay/H4 PASS.

A reviewed success could close the selected 2A castle/tower caller continuity and natural admission/
first-player state portion of RA-02–RA-05, plus specifically observed 8D consumers. It cannot close
natural battle actions/results/RNG draws (RA-06), victory/after-program/5B (RA-07/RA-12), remaining
8D or H4. [R3a–d](map3-battle01-turn-control.md) and
[R4a](map3-battle01-victory-return.md) remain reusable static evidence for a later winning trace:
`BattleLoop_Victory` `0x23CBA`, `abcs_battle01` `0x496DC`, F401 clear→F501 set, D4=1,
MainLoop/SwitchMap and the unentered `ExplorationLoop` call at `0x75E4` do not prove 5B. Reaching a
stable controllable post-after-program state requires its own declared fields/consumer closure.
The proposed player-ready run exits/restores too; absent a separately admitted retention mechanism,
a later continuous winning observation must budget a fresh traversal again. This cost is explicit;
it does not justify adding savestate/export or silently continuing into combat now.

### Proposed ceilings and decision sequence

The table and approval sequence below record the original proposal. For current Issue #485 work,
the user's [stabilization authorization](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/485#issuecomment-5752405430)
supersedes its cumulative stopping/one-attempt restrictions. The [current acquisition owner](map3-messenger-acceptance.md#savestate-linked-segments-issue-485)
retains real costs and failures, adds early checkpoints, source-consumer/facing checks and original
FieldMenu cancel recovery, and requires actual save → verified load → forward save acceptance.
Completed failed children may retry from the last compatible complete parent with unique retained
attempts; successful children become the next parent. Historical costs are never refunded.
Single-step/exchange/disconnection and identity/core/I/O containment remain; offline implementation
does not close native continuity or any later Battle01/H4 acceptance boundary. Subsequently the
[actual house → native load → Sarah forward-save chain](map3-messenger-acceptance.md#accepted-early-native-save-and-resume)
was independently accepted as **Confirmed**: native core/observer continuity before input, inherited
epochs, 1050 forward child frames and a later closed checkpoint. The latest complete Sarah parent is
the continuation point. Later messenger/Map19/Battle01 and H4 remain **Unknown**, with all failed costs retained.

The current [Issue #485 method amendment](map3-messenger-acceptance.md#savestate-linked-segments-issue-485)
supersedes this proposal's single-process/no-savestate choice and request for fresh user confirmation
of that choice. The user has authorized savestate-based segmentation. First deliver offline
implementation/material in a Draft PR with zero new native starts; main-gate retains independent
implementation acceptance and concrete lineage admission. The previous proposal and failures below
are retained as their original scope, not instructions to reject the new method.

The four selected completed-frame ends are settled Map19 `(26,29)` field control; returned royal
Map20 `(23,39)`/Down control after F605; existing neutral Map21 `(5,15)`/Down guard wait; and the
completed frame after the first player-ready callback. The first three add zero pending blocking
consumer/return/program/audio operation, neutral input, settled player/camera, no modal/transfer and
recent source field-control poll predicates. These are **Inferred** safe resumable points from the
source and observer structure, with **Confirmed** offline predicate rejection checks; their actual
reachability/native save/load remains **Unknown**. The final frame is evidence only, not resumable.
Callback-time player-ready and later saved frame remain separate. The implementation must not repeat
R1 bootstrap, reconstruct original state manually or silently abandon observer continuity on load.

The planning envelope becomes one lineage of at most four normal starts, no automatic retries or
branching. Historical starts were **3** before it and are now **5**, including the registration
failure and the [completed camera-readiness failure](map3-messenger-acceptance.md#segment-1-camera-readiness-failure).
Neither produced a state/pair. Reviewed cumulative consumption is 1556.4798537000315 active seconds,
10740 delivered frames and 174 advancing batches, leaving 5643.5201462999685 seconds, 25260 frames and
426 batches. Initial preparation requires all reviewed values; children inherit only their sealed
parent's consumption and actual start receipt. Actual observer/emulator frames and R1 epochs remain
independent of resource consumption. The completed real-library clock proof is preserved without
rerun. No retry or second segment is admitted.

State/step and save now share a completed-frame readiness predicate exposing raw camera A/B targets
and original scrolling bits alongside closure/input fields. Ordinary not-ready saves return typed
nonterminal results, advance zero frames, write/restore nothing and renew no budget. Fatal
malformed/off-route/identity/I/O/callback/budget failures retain their stops. The last failed save
passed player/closure checks but rejected camera equality; its raw camera values and extra settling
time are **Unknown**. Original source masks are now ROM-bound and added without removing that guard.
The 36000 frames/600 batches and 7200
active-session seconds remain cumulative. First Map19 by **3600** and returned guard by 5700 use
cumulative active time, so loading cannot renew either stage allowance.
[Main-gate decision 5752243620](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/485#issuecomment-5752243620)
adopts only that first-stage change: it leaves 2043.5201462999685 seconds before Map19, 491.3300810999354
above the last 1552.1900652000331-second reacquisition. This allowance is not a completion prediction.
Other caps and reviewed consumption stay unchanged. Fresh prepare-only material replaces no old
artifact and grants no native admission; final independent review/merge and concrete admission are
still required. The 3600-frame progress clock carries
forward; offline gaps are separately recorded. Original first-AI/action/decline/failure stopping
rules remain, with no forced order or intervening combat. Native compatibility belongs to admitted
segments, not an additional smoke process. Existing #475 raw, #483 preparation and prior failures
remain preserved; none contains a continuation state to substitute for this lineage's first segment.

All numbers below describe the **earlier proposal**, not a remaining allowance. #475 consumed **1778.575/1800
seconds for 10329 frames and 200 batches**, including 355 bootstrap frames and 9974 operator frames.
Only 21.425 seconds of its wall limit remained. Roughly 172 seconds of nominal 60-Hz emulated time
is not 1778 seconds of emulation work: wall time includes host, native execution, log/transport and
paused operator decisions. The retained result does not isolate those components, so no CPU speed
or exact operator-idle measurement is claimed. The observed aggregate is about 5.8 completed
frames/wall-second and 8.9 wall-seconds/batch, not a throughput guarantee.

| Proposed ceiling | Rationale and required stop |
| --- | --- |
| One run API invocation, at most one owned native process start; no smoke/retry/reconnect | No automatic fourth-start right. A future explicit decision must identify this as an additional historical controlled start (total would become 4 if it starts), with the existing three and both controlled FAIL results intact. ADR 0015's present prohibition must be explicitly resolved by accepted capability/static review and a fresh user/main-gate decision; a new Issue does not reset it. Preflight failure also ends this proposed invocation. |
| 7200 seconds from native launch, including startup and all paused time | Four times the exhausted prior ceiling, explicitly proposed for reacquisition plus two long scenes and downstream route. Check first Map19 by 2400 seconds and returned Map21 guard by 5700; missing either stops as stage-budget exhaustion, leaving 1500 seconds for R2c/before/start/readiness. These are stop-loss estimates, not predicted completion times. |
| 36000 total emulator frames including bootstrap; 600 input batches; 1–120 frames each | R2b has 72 remaining navigation inputs after Map19 plus R2c's 46, alongside unshimmed palace/prompt/before-battle waits. Roughly 3.5 times prior completed frames at the observed aggregate would cost about 6200 wall seconds; 600 batches at the prior mean cost about 5336. Neither extrapolation proves feasibility, but both leave measurable headroom under 7200, unlike reusing 1800. Wall, frame and batch ceilings are independent; first reached wins. No frozen timing table is inferred from topology. |
| 120 seconds paused without an accepted advancing batch; state/ping do not renew this deadline | Bounds a missing operator without consuming emulated time. Keep the existing 60-second startup/individual exchange limit. Enforce operator idle on the host too so a stalled native receive cannot evade it; prompt waiting is intentional only within these explicit limits. |
| 3600 advanced frames without source-bound progress | Reset on a new program operation/return, actual relevant entity/camera/fade progress or accepted input/position change; repeated identical polling/liveness does not reset it. Paused host time consumes the idle/wall limits instead. Explicit source waits in the selected palace/before program are at most 60 each, but total entity/text-consumer duration is unmeasured; exceeding this 60 nominal-second window stops for review, never skips the wait or raises the cap. |
| Existing teardown grace: 3 seconds wait, then owned-handle termination and 3 seconds wait | Zero extra gameplay frames/starts. Preserve primary failure and separate cleanup result. Forced containment proves process termination only; native EOF/abort restoration remains unproved by the successful #475 path. |

Before any proposed run, a later capability slice must implement and independently review the exact
checkpoint/readback/limit/typed-stop changes, including same-frame terminal and unsupported-first-AI
handling. Only then may main-gate present the concrete method, estimates and stop-loss exception to
the user for fresh explicit runtime approval. If these budgets or the bounded first-actor restriction
are unacceptable, revise the static plan; do not launch to discover a better budget. Offline
preparation/materialization is also separately scoped, and must not overwrite #473/#475/root-review
material. No such API has been called for this proposal.

Research would own the one process/controller and fresh ignored attempt output in the existing
Research environment. Reuse installed immutable tools; do not attach to a stale process or share
writable emulator state. Require launch identity/PID, actual command/frame/checkpoint reconciliation,
Lua status and errors, all restoration domains, zero candidate callbacks, neutral input, process
exit/timeout/forced-containment status, independent owned-survivor inspection, session-ROM deletion
and canonical identity unchanged. Keep those results private and publish only the reviewed bounded
projection. Stop/cleanup here means the runner's declared restoration and process teardown, not
permission to delete retained evidence or remove worktrees/refs.

The two controlled failures, actual three starts, disabled replay ordinal 1/2 timeout/cleanup failure
and missing genuine receipts remain with their existing owners. Exact post-447 wait entry remains
**Inferred**; #475's terminal still establishes no next-tile displacement. Remaining continuity,
7C audio, remaining 8D, H4 and 5B retain their existing **Unknown** boundaries. This proposal supplies
no new runtime evidence.

Reproduce this proposal's checks by reading the named fixture JSON pointers and pinned source
sections, the runner/observer functions above, and enumerating only the three named private evidence
roots. Parse the small terminal/status/receipt projections read-only; do not load SaveRAM or execute
retained analysis scripts. Check Markdown targets/anchors, exact one-file scope, public/private
boundary and `git diff --check`; record exact Git identity and actual public CI in the Draft PR.
For #479 specifically, the Issue restricts verification to these direct checks: the general planner
command below is **NOT RUN**, as are all runtime APIs, H1/H2/H3, normal/full suites and helper tests.

## Documentation verification and handoff boundary

This review reads accepted fixture fields and maintained source/contract owners without replaying
prior evidence. Use direct Markdown link/pointer/scope checks and `git diff --check`. On a clean
committed candidate, load the existing ignored private-input configuration in the same shell and run:

```powershell
uv run sf2 verify plan --base origin/main --head HEAD
```

Interpret its selection under the documentation-only policy; no normal/full/Python aggregate,
.NET/Godot/H3 suite is required solely by this audit. Record exact head/tree, planner result and
actual scoped CI on the Draft PR/Issue. The underlying R1–R4a commands above remain their owners'
evidence reproducers, not this slice's acceptance commands. Keep private inputs/outputs and prior
completed failures intact. Independent review/merge, later Design field composition, original
observation admission and continuous H4 acceptance remain separate boundaries; this audit does not
close the milestone or #437.
