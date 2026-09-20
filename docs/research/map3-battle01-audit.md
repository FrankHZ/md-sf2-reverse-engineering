# Map 3 to Battle 01 Research Gap Audit

- Status: **OPEN** — Research evidence mapping; not readiness approval or H4 PASS.
- Evidence review base: accepted `main` `51947b99271bb48bac25400b9057c0d8932eafe2`, tree
  `82ec07aa55b241ad5b70cfbf80d0300873c5ac6f` (2026-09-19).
- ROM: USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
- Source: `ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`.
- Product owner: [ADR 0010](../decisions/0010-map3-battle01-product-acceptance.md), selected
  `1A + 2A + 3A + 4A + 5B + 6A + 7C + 8D + 9A + 10A`.

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

This reuse is **not currently executable as an admitted observation**. The observer's controlled
bootstrap ROM/RAM/register writes and state-aware joypad policy are not an ADR 0015 passive replay.
Removing the bridge alone does not make it one. A passive method would also need a concrete admitted
start artifact, frozen non-adaptive input transport and removal of continuing shims/writes; neither
is supplied by the existing command. Actual text consumption cannot reuse `DisplayText`-RTS.
A narrower controlled-H3 method must be explicitly classified and accepted with its limitations; it
cannot silently inherit permission from historical R2a. The accepted evidence dossier grants no
execution permission. Issue #456's [preparation owner](map3-messenger-acceptance.md#unshimmed-map3-candidate-preparation-only)
now defines an opt-in candidate in the existing messenger Python/Lua rail: service/scratch restoration
at the R1 wait, explicit frozen diagnostic input, missing status/NPC readbacks and bounded gate/Map19
callbacks. It has no CLI wiring or runtime result. Its materialization status is
`CANDIDATE-PREPARED-NOT-ADMITTED`, not `PRELAUNCH-PASS`; ordinary R2a/R2d fixtures remain unchanged.
The timed diagnostic input is not an accepted original recording and may fail. Independent method,
exact material, code, verification and old-lineage/budget review still precede any launch.

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

### Lineage decision before implementation or launch

This proposal targets stateful gate movement/F604/warp semantics and reuses the earlier batched
observer, rather than the disabled replay capability's fixed movie/capture backend. That technical
difference does **not** establish a new budget: it overlaps the failed R2b route and inherits its
provenance. The later accepted static graph and the explicit bridge's success do not erase the failed
natural work or prove a corrected natural runner. Main-gate must independently decide the method
class, whether accepted new static evidence or a separately corrected capability changes admission,
and exactly how prior R2b/replay consumption constrains it. Until then the old prohibition remains.

The [capability failure owner](original-reference-replay-capability.md) and
[scenario boundary](original-reference-replay-scenario-api.md) retain actual failure details and
missing receipts. Ordinal 2 cannot be presumed unused; its reported FAIL is not the same-candidate
PASS required by the disabled path for ordinal 3. No nominal reset, fourth launch, renamed runner
or task creates allowance. Recovery of that disabled path is not this proposal's milestone
prerequisite; an explicit lineage/budget disposition is. No new observer/fixture/launch is admitted
by this dossier. The next boundary is independent main-gate review of these fields and that concrete
method/reuse/admission decision, not automatic runtime execution.

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
