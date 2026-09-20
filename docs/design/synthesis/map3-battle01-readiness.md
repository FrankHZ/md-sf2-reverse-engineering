# Map 3 to Battle 01 Readiness Ledger

- Status: **NOT READY** for eventual continuous-milestone acceptance; not a default blocker for a separately authorized implementation start
- Accepted evidence baseline: `aabec912372c06a89265369c767399c7573154eb`; original findings
  retain the exact provenance in their linked owners and fixtures.
- Milestone owner: [ADR 0009](../../decisions/0009-first-phase4-playable-slice.md)
- Tooling boundary: [ADR 0008](../../decisions/0008-godot-csharp-cli-first-remake-tooling.md)
- Product profile: [ADR 0010](../../decisions/0010-map3-battle01-product-acceptance.md)
- Start-policy amendment: [ADR 0016](../../decisions/0016-remake-start-evidence-deferral.md)
- Scope: Layer B readiness accounting for one continuous playable scenario from an admitted Map 3
  start through observable completion of Battle 01

## Current acceptance scope

[ADR 0010's current amendment](../../decisions/0010-map3-battle01-product-acceptance.md#current-acceptance-amendment)
selects **8D gameplay and presentation semantics**, superseding the old 8C hardware-exact gate.
Pixel equality, original frame cadence/animation durations, waveform/chip and VInt/DMA/CRAM/VDP timing
are outside this milestone's acceptance domain. Timing that changes gameplay results, event order
or input readiness still needs comparison. Private 7C content, 9A accessibility, 10A deviations,
natural continuity and the 5B endpoint remain required.

Historical 8C capture, raw-time and hardware Unknowns retained in the evidence matrices below are
research boundaries, not completion gates or closed facts. The old original replay path is disabled;
its ledger recovery, runtime capability, scenario transport and full hardware APIs are not milestone
prerequisites. Main-gate owns disposition in the milestone Issue, splitting a child only for concrete
removal/replacement implementation. Old failures and launch prohibitions remain. Necessary new
original observation must answer a named gameplay question, reuse evidence first and obtain separate
purpose/method and lineage/budget admission; renaming cannot reset consumption.

## Judgment Boundary

This document is a readiness ledger. It does not own original-game evidence, define a new scenario
contract, select a product experience, authorize Phase 4, or replace the fixtures and contracts it
links. Its purpose is to state what accepted `main` can already support, what remains open, who must
own each closure, and what eventual continuous-milestone acceptance must inspect.

The product-choice slots and battle-functions contract are closed. Accepted runtime evidence now
closes the controlled Map 3 start, older fixture-local prefixes, and a separately accepted interactive
acquisition through first original Up acceptance in Map19 `(26,30)`, before next-tile displacement.
Accepted static owners close the R2b-through-R4a source/H1/ROM topology. R2d adds a **Confirmed**
explicit-bridge H3 continuation through Battle 01 initialization to the first stable player-input seam. The current
judgment is still **NOT READY** for eventual milestone acceptance because those owners do not join
that Map19 boundary through the remaining castle/tower route to natural Battle 01 admission, a complete
playable battle, an executed after-battle program, and one exact observable ending state.

The following distinctions are normative:

- a controlled helper or debug seam is not a natural-story route;
- a static source graph is not an observed chronological playthrough;
- a fixture-local H4 surface is not an end-to-end scenario golden;
- an indexed file is not automatically a future design association;
- a numeric ID or source label is not player-facing meaning;
- a private original asset is not a distributable remake asset;
- a readiness closure is not authorization to create `remake/` or begin Phase 4.
- **NOT READY** in this ledger is not a default implementation-start blocker; ADR 0016 requires a
  separate explicit user authorization and concrete-slice dependency review instead.

No unmerged Research result contributes to this ledger. A future update may consume new evidence only
after it is accepted on `main`.

The retained R2d review reused accepted #303, not a new emulator observation. It
checked the [admission research owner](../../research/map3-battle01-admission.md),
[H3 fixture](../../../tests/fixtures/h3/map3-battle01-player-ready-v1.json),
[fixture schema](../../../schemas/h3/map3-battle01-player-ready-fixture.schema.json), and
[verifier](../../../src/sf2tool/h3/map3_battle01_player_ready.py). The owning reproduction commands
are `uv run sf2 h2 map3-battle01-admission` and `uv run sf2 h3 map3-battle01-player-ready`; neither is
rerun for this documentation-only reconciliation. The fixture explicitly records a
`explicit-controlled-harness-bridge` with `naturalR2bContinuity: false` and stops at
`ControlBattleEntity.after-WaitForVInt-before-input-read`. Thus the bounded ready state is accepted,
but that fixture does not prove natural R2a-to-R2b continuity. The separate observation below closes
only the prefix through first Map19 control.

### Accepted controlled-R1-to-first-Map19 observation

**Confirmed bounded original observation:** this reconciliation consumes the accepted
[Research result and provenance](../../research/map3-messenger-acceptance.md#single-admitted-interactive-acquisition-result)
and [audit disposition](../../research/map3-battle01-audit.md#interactive-acquisition-reached-map19-admission-consumed)
on `aabec912372c06a89265369c767399c7573154eb` (tree
`5fa4cdae544a7a7215cf416f29ba53901d2b7447`), including the
[independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/476#issuecomment-5747831234).
Those owners retain pinned source/H1/ROM/tool identities, actual input/checkpoint/order records,
private artifact identities and read-only reproduction. This is a separate accepted observation,
with no new evidence ID or changed R1/R2/R2a/R2d fixture projection.

After controlled R1 service/scratch restoration, ordinary controller input reached the real
messenger text consumers, original YesNoPrompt return `D0=0`, follower-ready wait, gate
`cs_51652` return, original F604 false-to-true trap, north warp, Map19 init/`cs_53104` returns,
wait and original Up acceptance at `0x52E8`. The terminal is Map19 `(26,30)`, F604 true,
event word 0 and no tracked pending consumers. The completed frame still has that position;
next-tile arrival is **Unknown**. No FieldMenu callback occurred on this acquisition.
All 30 R1 ally status words were 0 in this attempt, resolving its inherited POISON question only;
this does not establish universal reset-start state or update the older R1 fixture.

The collector's immutable `OBSERVATION-COMPLETE-UNREVIEWED` label describes its raw output;
main-gate independently accepted the bounded facts. Neither label nor acceptance is a frozen-replay,
complete 8D or H4 PASS. Exact post-447 wait entry remains **Inferred**; a stale text poll is not
that observation. Unobserved prompt branches and general backend/EOF/abort/partial-batch behavior
remain **Unknown**. Natural New/load, the remaining castle/tower route, Battle01 admission/actions,
victory and 5B are not established. Historical controlled starts total **3**, with no runtime
allowance remaining; the two completed controlled **FAIL** observations and disabled replay lineage
remain preserved. This document review performs no acquisition, replay, preparation or input change.

Current remake behavior is tracked separately by
[ADR 0019](../../decisions/0019-state-and-content-driven-remake-engine.md) and the
[capability ledger](../../../remake/docs/capability-status.md). Implemented common-session admission,
battle outcomes, and exploration return do not establish original natural continuity or complete
8D/H4 acceptance. This review consumes the bounded audit disposition above without rerunning evidence
or closing downstream gaps.

## Readiness Classification

Every dependency is assigned one or more of these exact classifications:

| Classification | Meaning in this ledger |
| --- | --- |
| **Contract-ready** | Accepted implementation-neutral contracts close the named local input, order, state, or output boundary. This does not imply continuous-scenario readiness. |
| **Synthesis-ready** | Accepted owners can support a bounded Layer B explanation without creating new evidence or a scenario-wide claim. |
| **Runtime/natural closure Unknown or Deferred** | Accepted static or bounded runtime owners exist, but the naturally reached caller order, result, persistence, presentation, or continuous behavior required by the milestone is unobserved or conditionally deferred under ADR 0014/0016. |
| **Missing design contract** | Accepted research exists, but no evidence-bound design contract yet owns the implementation-neutral surface required by the milestone. |
| **Explicit product decision** | The answer is a remake scope, experience, asset, accessibility, fidelity, or deviation choice rather than a recoverable original-game fact. |

A row may be contract-ready locally and still contain a scenario-level research or decision gap.
That is expected: this milestone requires composition, not merely the existence of subsystem files.

## ADR Gate

[ADR 0008](../../decisions/0008-godot-csharp-cli-first-remake-tooling.md) accepts Godot 4.7.2 .NET,
C#, a CLI-first toolchain, a plain-C# deterministic domain layer, and a thin Godot adapter. It does
not install Godot, select an MCP adapter, choose distributable assets, create a remake project, or
authorize implementation. The first implementation acceptance profile remains CLI-only.

[ADR 0009](../../decisions/0009-first-phase4-playable-slice.md) accepts exactly one first milestone:
a continuous playable scenario from Map 3 through **completion** of Battle 01. Its eventual acceptance
requires Research and Design gap closure, a main-gate readiness report, and a separate user start
action. The user satisfied that historical implementation-start gate on 2026-08-28, as recorded in
[`remake/README.md`](../../../remake/README.md). Battle entry, initialization, a bounded implementation
slice, or an isolated mechanic cannot satisfy the milestone.

[ADR 0010](../../decisions/0010-map3-battle01-product-acceptance.md) accepts the exact profile
`1A + 2A + 3A + 4A + 5B + 6A + 7C + 8D + 9A + 10A`. It selects a private-local original-asset
profile with no public redistribution and gameplay/presentation-semantic parity. These choices close
the product slots; they do not establish missing Research facts or make the scenario ready.

This ledger therefore remains **NOT READY** until all remaining closure rows below are accepted on
`main` for the eventual continuous milestone. Its status does not itself reject a separately
user-authorized bounded implementation start under ADR 0016.

[ADR 0016](../../decisions/0016-remake-start-evidence-deferral.md) controls that separate start
policy. It preserves this ledger's eventual acceptance target while allowing a user-authorized
implementation slice to require only the accepted owners it concretely needs. Natural continuity,
required original behavioral evidence, required 8D observations, the continuous-scenario contract,
and H4 completion remain **OPEN** acceptance work rather than default pre-start blockers.

## Exact Accepted-Index Audit

### Map 3 aggregate rows

The accepted research index contains exactly 26 records whose `sourcePath` begins with
`data/maps/entries/map03/`. All 26 are currently unassociated and each carries only the aggregate
`sf2-map-data-static-v1` evidence owner.

| Indexed Map 3 source role | Record count |
| --- | ---: |
| setup pointer tables | 4 |
| entity tables | 4 |
| entity-event tables | 4 |
| zone-event tables | 4 |
| area-description tables | 2 |
| item-event sections | 2 |
| setup init functions | 4 |
| script source containers | 2 |
| **Total** | **26** |

This denominator proves accepted file inventory only. It does not establish which rows are selected
from an admitted start, their chronological execution, their natural effects, the complete route to
Battle 01, or a future association set. A later design slice MUST NOT automatically associate all 26
records. It must derive its exact record set from a dedicated accepted evidence owner.

### Battle-functions contract closure

`sf2-battle-functions-static-v1` directly binds exactly 15 research-index records. All 15 are now
associated with [Battle Functions Control Flow](../contracts/battle-functions-control-flow.md):

| Exact record ID | Accepted static surface |
| --- | --- |
| `battle.functions.pulsating-grid` | bounded shared-function inventory |
| `battle.functions.angel-wing` | Angel Wing exit/control path |
| `battle.functions.update-targets` | target-state update boundary |
| `battle.functions.relative-move-table` | relative movement-table identity |
| `battle.functions.execute-turn` | individual-turn control route |
| `battle.functions.load-battle` | ordered battle-load handoffs |
| `battle.functions.move-sfx` | move-command identity selection |
| `battle.functions.control-cursor` | cursor/tile control flow |
| `battle.functions.choose-target` | target-list navigation and result |
| `battle.functions.set-cursor-target` | next-entity cursor target selection |
| `battle.functions.player-input` | player action control state machine |
| `battle.functions.battlefield-menu` | battlefield menu branch surface |
| `battle.functions.ai-target-visual` | AI target-visual handoff |
| `battle.functions.equip-in-battle` | bounded battle equip branch |
| `battle.functions.check-gold-chest` | bounded chest/gold branch |

The [battle-functions research owner](../../research/battle-functions.md) also reports a 16-record
source-path membership join because `map.camera-control.destination-service` shares one source file.
That cross-owner record is not a direct `sf2-battle-functions-static-v1` binding and remains with the
[map-camera update contract](../contracts/map-camera-update-control-flow.md). It is not a candidate
for this battle-functions contract.

### Battle 01 route and outcome rows

`battle.cutscene.data.battle01.beforebattle` and
`battle.cutscene.data.battle01.afterbattle` remain associated with
[Battle Cutscene Routing](../contracts/battle-cutscene-routing.md). That contract closes route-table,
admission, and static program-corpus facts; it explicitly leaves complete MAPSCRIPT effects, natural
reachability, persistence, visible sequencing, and story consequences open.

[Battle Control and Combatant Lifecycle](../contracts/battle-control-lifecycle.md) closes the generic
victory mutation order: heal eligible party, run the after-battle seam, clear the unlocked flag, set
the completed flag, and return `D4=1`. The earlier Battle 01 debug H3 entry uses Debug Battle Test and
skips the before/start cutscenes. The separate R2d explicit-bridge observation below does observe
those programs returning, but neither entry establishes natural Battle 01 admission, after-battle
program effects, or the milestone's observable endpoint.

### Accepted scenario evidence-chain refresh

The accepted scenario-specific owners now form this exact bounded chain. Runtime and static labels are
not interchangeable. Each row's retained boundary describes its own fixture or observation, not a
denial of the separate R2d or controlled-R1-to-first-Map19 result:

| Stage | Accepted owner | Closed surface | Retained boundary |
| --- | --- | --- | --- |
| R1 | `sf2-map3-admitted-start-runtime-v1`; [fixture](../../../tests/fixtures/h3/map3-admitted-start-v1.json); [research owner](../../research/map3-admitted-start.md) | controlled Map 3 state through the first `WaitForEvent` | not a natural New/load route, later Map 3 behavior, raw-time golden, or 8C capture |
| R2 | `sf2-map3-battle01-natural-route-runtime-v1`; [fixture](../../../tests/fixtures/h3/map3-battle01-natural-route-v1.json); [research owner](../../research/map3-battle01-natural-route.md) | natural opening through `cs_5149A` entry-before-body; `FieldMenu` **NotReached** | messenger body, later route, effects, Battle 01 admission, and presentation remain open |
| R2a | `sf2-map3-messenger-acceptance-runtime-v1`; [fixture](../../../tests/fixtures/h3/map3-messenger-acceptance-v1.json); [research owner](../../research/map3-messenger-acceptance.md) | accepted messenger continuation through follower-ready `WaitForEvent`; `FieldMenu` **NotReached** | natural continuation into the static castle/battle route, later effects, and Battle 01 remain open |
| Controlled R1 to first Map19 control (separate observation) | [accepted result/provenance](../../research/map3-messenger-acceptance.md#single-admitted-interactive-acquisition-result); [audit disposition](../../research/map3-battle01-audit.md#interactive-acquisition-reached-map19-admission-consumed) | **Confirmed** real messenger/prompt/gate/F604/north-warp/init-return chain through original Up acceptance at Map19 `(26,30)` | no next-tile displacement, remaining castle/tower route, natural battle admission, winning trace, 5B, frozen replay or complete 8D/H4; no fixture projection changed |
| R2b | `sf2-map3-castle-battle-unlock-static-v1`; [fixture](../../../tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json); [research owner](../../research/map3-castle-battle-unlock.md) | legal source-derived route and unlock topology | natural execution, caller order, endpoint, and R2c continuity are **Unknown** |
| R2c | `sf2-map3-battle01-admission-static-v1`; [fixture](../../../tests/fixtures/h2/map3-battle01-admission-static-v1.json); [research owner](../../research/map3-battle01-admission.md) | legal admission/initialization spine | natural admission, cutscene execution, initialized snapshot, first actor, and player-ready state are **Unknown** |
| R2d | `sf2-map3-battle01-player-ready-runtime-v1`; [fixture](../../../tests/fixtures/h3/map3-battle01-player-ready-v1.json); [research owner](../../research/map3-battle01-admission.md) | **Confirmed** explicit harness bridge from R2a to the retained R2b terminal, then original control through Maps 21/40/57, admission, before/start program returns, initialization, turn generation, actor 1 dispatch, and the first stable input seam | natural R2a-to-R2b continuity, wholly natural snapshot/actor selection, post-seam input/actions/results, presentation, victory, and complete 8C remain **Unknown** |
| R3a | `sf2-map3-battle01-turn-control-static-v1`; [fixture](../../../tests/fixtures/h2/map3-battle01-turn-control-static-v1.json); [research owner](../../research/map3-battle01-turn-control.md) | turn/control source topology | reached player/AI branch, commands, movement, targets, actions, and results are **Unknown** |
| R3b | `sf2-map3-battle01-action-effect-static-v1`; [fixture](../../../tests/fixtures/h2/map3-battle01-action-effect-static-v1.json); [research owner](../../research/map3-battle01-action-effect.md) | action/effect dispatcher and caller topology | actual branch selection, resolution, status, death, EXP, gold, drop, follow-up, and victory are **Unknown** |
| R3c | `sf2-map3-battle01-action-completion-static-v1`; [fixture](../../../tests/fixtures/h2/map3-battle01-action-completion-static-v1.json); [research owner](../../research/map3-battle01-action-completion.md) | action-completion and replay return topology | reached completion, replay, follow-up, after-turn, and next-turn dispatch are **Unknown** |
| R3d | `sf2-map3-battle01-turn-finalization-static-v1`; [fixture](../../../tests/fixtures/h2/map3-battle01-turn-finalization-static-v1.json); [research owner](../../research/map3-battle01-turn-finalization.md) | replay/teardown/reload/after-turn/next-turn control spine | actual outcomes, player readiness, next turn, multi-round play, and victory are **Unknown** |
| R4a | `sf2-map3-battle01-victory-return-static-v1`; [fixture](../../../tests/fixtures/h2/map3-battle01-victory-return-static-v1.json); [research owner](../../research/map3-battle01-victory-return.md) | static Victory-to-after-program-to-return/SwitchMap/Exploration spine | victory/program reach and completion, flags/join outcomes, exploration re-entry, stable endpoint, R4b, and H4 are **Unknown/Deferred** |

The accepted aggregate `sf2-map-data-static-v1` remains the owner of the 26 Map 3 source rows. The
scenario fixtures above do not make all 26 rows reached, required, or candidates for bulk Design
association.

## Continuous-Scenario Dependency Matrix

| Scenario segment | Accepted owner surface | Readiness classification | Required closure |
| --- | --- | --- | --- |
| admitted Map 3 start | [Map 3 Controlled Admission](../contracts/map3-controlled-admission.md), [New-Game State Initialization](../contracts/new-game-state-initialization.md), [Save System](../contracts/save-system.md), [Story Progression](story-progression.md) | **Contract-ready bounded runtime owner** through the first exploration wait; **Accepted product decision** for controlled admission | natural player-visible New/load equivalence and later route remain **Unknown**; raw time and complete 8C remain open |
| Map 3 setup and content | [Map Setup Data](../contracts/map-setup-data.md), [Map and Exploration](../contracts/map-exploration.md), R1/R2/R2a, the separate Map19 observation and R2b owners above | **Contract-ready static/bounded-runtime owners** for selected default setup, reached prefixes, and legal continuation topology; **Missing design contract** for continuous scenario composition | natural continuation after first Map19 Up acceptance, selected later effects, and complete route-required content remain **Unknown/Deferred**; no bulk association of 26 rows |
| exploration loop and input | [Exploration Control Flow](../contracts/exploration-control-flow.md), [Input System](../contracts/input-system.md), [Map Entry Routing State](../contracts/map-entry-routing-state.md), R2/R2a and separate Map19 observation above | **Contract-ready bounded runtime prefix** and local handoffs; **Accepted product decision** for modern logical controls/accessibility | next-tile displacement after Map19 Up acceptance and subsequent input/result chronology remain **Unknown/Deferred** |
| dialogue and interaction | [Dialogue System](../contracts/dialogue-system.md), [Sprite Dialogue Property Data](../contracts/sprite-dialogue-property-data.md), [Text and Font System](../contracts/text-and-font-system.md), [Portrait Window State](../contracts/portrait-window-state.md), R2a and separate Map19 observation above | **Contract-ready static seams and bounded messenger result**, plus accepted real messenger/gate text consumers and returns; **Accepted product decision** for private-local original text | exact post-447 wait entry remains **Inferred**; remaining required dialogue/program and presentation consumption/completion and downstream continuity remain **Unknown/Deferred** |
| field menu and UI | [Exploration Control Flow](../contracts/exploration-control-flow.md), [Window System](../contracts/window-system.md), [UI Layout Data](../contracts/ui-layout-data.md), [UI Graphics Asset Data](../contracts/ui-graphics-asset-data.md) | **Contract-ready** handoff/layout/resource seams; `FieldMenu` is **Confirmed NotReached** in R2/R2a; no FieldMenu callback occurred in the separate Map19 acquisition; **Explicit product decision** for required pages and presentation | no field-menu contract is required by the reached prefix; a later naturally reached route requirement would trigger a bounded conditional owner |
| map resources and camera | [Map Layout Data](../contracts/map-layout-data.md), [Map Palette Data](../contracts/map-palette-data.md), [Map Tileset Data](../contracts/map-tileset-data.md), [Map Sprite Graphics Data](../contracts/map-sprite-graphics-data.md), [Map Entity Data](../contracts/map-entity-data.md), [Map Camera Update](../contracts/map-camera-update-control-flow.md) | **Contract-ready static import/local-control owners**; **Accepted product decision** for private originals and 8D | reached scene/resource/cue identities, order, completion/readiness and private provenance remain **Unknown/Deferred**; hardware exactness is outside scope |
| map-to-battle admission | [Exploration Control Flow](../contracts/exploration-control-flow.md), [Map Entry Routing State](../contracts/map-entry-routing-state.md), [Battle Encounter Definition](../contracts/battle-encounter-definition.md), [Battle Cutscene Routing](../contracts/battle-cutscene-routing.md), R2b/R2c/R2d owners above | **Contract-ready static route/admission spine and bounded explicit-bridge runtime seam**; **Missing design contract** for continuous scenario composition | continuity after first Map19 control through the remaining R2b route and natural admission/caller state remain **Unknown/Deferred**; R2d's program returns and first ready state are controlled, not natural-route closure |
| Battle 01 encounter setup | [Battle Encounter Definition](../contracts/battle-encounter-definition.md), [Battle Control and Combatant Lifecycle](../contracts/battle-control-lifecycle.md), [Battlefield Navigation](../contracts/battlefield-navigation.md), R2c/R2d/R3a owners above | **Contract-ready static encounter/control spine and bounded initialized/player-ready observation** | wholly natural initialized snapshot and first-actor selection, post-seam input, and later-round state remain **Unknown/Deferred** |
| player turn and battle menus | [Battle Functions Control Flow](../contracts/battle-functions-control-flow.md), [Input System](../contracts/input-system.md), R2d/R3a owners above | **Contract-ready static branch/request/local-output owners and bounded actor-1 player-control dispatch**; **Accepted product decision** for manual agency and UI | inputs after the ready seam, AI execution, command, movement, target, action, cancellation, and result remain **Unknown/Deferred** |
| AI and navigation | [Battle AI Decision](../contracts/battle-ai-decision.md), [Battlefield Navigation](../contracts/battlefield-navigation.md), R3a/R3b owners above | **Contract-ready algorithms and static caller topology** | actual AI branch, command, movement, target, result, and multi-round decisions remain **Unknown/Deferred** |
| action construction and resolution | [Battle Action Construction](../contracts/battle-action-construction.md), [Combat Resolution](../contracts/combat-resolution.md), [Spell Resolution](../contracts/spell-resolution.md), [Randomness](../contracts/randomness.md), R3b/R3c owners above | **Contract-ready bounded algorithms and static action/effect/completion topology**; **Accepted product decision** for one deterministic H4 trace | reached seed, actions, resolution/status/death/EXP/gold/drop/follow-up outcomes, replay, and next-turn dispatch remain **Unknown/Deferred** |
| battle presentation | [Battle Scene Presentation](../contracts/battle-scene-presentation.md), dedicated graphics-data contracts, and R3c/R3d owners above | **Contract-ready loader/static asset and replay/finalization topology**; **Accepted product decision** for private originals and 8D | reached scene/animation/audio identities, semantic order, completion/readiness and private provenance remain **Unknown/Deferred**; hardware exactness is outside scope |
| victory and after-battle | [Battle Control and Combatant Lifecycle](../contracts/battle-control-lifecycle.md), [Battle Cutscene Routing](../contracts/battle-cutscene-routing.md), R3d/R4a owners above | **Contract-ready static victory/after-program/return spine**; **Missing design contract** for observable continuous completion | natural victory, program reach/completion, flags/join results, SwitchMap/exploration re-entry, and stable endpoint remain **Unknown/Deferred** |
| save/load scope | [Save System](../contracts/save-system.md), [Global Flag State](../contracts/global-flag-state.md), roster/state contracts | **Contract-ready** bounded service/storage seams; **Accepted product decision** to exclude milestone save/load/checkpoint/suspend | enforce restart-to-admitted-snapshot behavior and keep later save support outside this milestone |
| end-to-end H4 | all named subsystem fixtures and contracts | **Synthesis-ready** for a ledger; accepted static chain and product layers exist; original-reference/continuous runtime remains **Unknown/Deferred** under ADR 0014–0016; **Missing design contract** for scenario composition and executable definitions | add one evidence-bound continuous-scenario contract that consumes, rather than weakens, subsystem fixtures; failed original-reference candidates remain non-evidence |

## Existing Synthesis Boundary

The following Layer B documents can already explain local pieces but do not close this milestone:

- [Gameplay Overview](gameplay-overview.md) connects top-level verbs and subsystem handoffs while
  retaining campaign, UI, timing, and presentation gaps.
- [Story Progression](story-progression.md) connects controlled Map 3 entry, static setup/event/script
  graphs, dialogue/roster/state seams, and save handoffs while explicitly rejecting a reconstructed
  normal campaign route.
- [Map Design Principles](map-design-principles.md) separates map structures from route quality,
  pacing, reachability, and authorial intent.
- [Tactical Battle Loop](tactical-battle-loop.md) composes local battle controller, player/AI,
  navigation, action, resolution, replay, and outcome owners while rejecting complete battle
  simulation and visible timing claims.
- [Progression and Economy](progression-and-economy.md) connects reward/state mutations but does not
  establish the Battle 01 route, balance, or complete persistence.

These documents are **Synthesis-ready** inputs to this ledger. None is the required continuous
scenario contract.

## Accepted Product Choices

[ADR 0010](../../decisions/0010-map3-battle01-product-acceptance.md) closes the product-choice slots
without turning evidence-owned exact values into product choices.

| Decision slot | Accepted state | Remaining closure |
| --- | --- | --- |
| admitted start | **Accepted: 1A controlled admitted snapshot** | R1 owns the exact controlled values/provenance through the first wait; it is not a canonical natural New/load claim |
| route | **Accepted: 2A smallest Research-proven natural route** | The separate acquisition extends the controlled-start runtime prefix to first Map19 Up acceptance; R2b/R2c own static continuation and R2d the explicit bridge to PlayerReady. Next-tile displacement, remaining castle/tower continuity, effects and required backtracking remain open |
| natural battle/cutscene | **Accepted: 3A chronology, with placeholder subclause superseded by 7C/8D** | R2c owns static admission topology; R2d observes before/start returns and first ready state only after the declared bridge; natural admission, wholly natural state, and presentation completion/readiness remain open |
| completion endpoint | **Accepted: 5B first stable controllable post-after-program state** | exact return map/location/state remains Research-owned; `D4=1` alone is insufficient |
| save/load | **Accepted: 6A excluded** | restart returns to the admitted snapshot; later save support is a separate milestone |
| player control and UI | **Accepted: 4A/9A manual agency and modern accessible logical controls** | product 9A is implemented and directly observed; exact original reached actions/input trace and continuous H4 accessibility composition/execution remain open |
| assets | **Accepted: 7C private-local originals only** | ignored private provenance/inventory must close; public distribution remains blocked without rights/replacements |
| visual/audio parity | **Accepted: 8D presentation semantics** | accepted messenger/gate consumer returns and first Map19 input acceptance are reusable; remaining reached scene/resource/cue identities, semantic order/completion/readiness and H4 definitions remain open; hardware exactness is excluded |
| RNG and action trace | **Accepted: one deterministic H4 reference trace** | R3a–R3d own static control/action/completion/finalization topology only; viable seed and reached logical trace remain open, and ordinary interactive play is not scripted |
| intentional deviations | **Accepted: 10A explicit ledger** | controlled admission, optional scope, modern controls, no save, fixed reference trace, and out-of-domain engine behavior require named checks |
| optional tooling | **Deferred and non-blocking; no MCP adopted** | CLI gates remain authoritative; no tooling choice starts Phase 4 |

## Remaining Acceptance Plan

### Reuse and disposition of the existing audits

The [Research audit](../../research/map3-battle01-audit.md#research-gap-register) already owns
RA-01–RA-12; this ledger already owns Design readiness. Neither audit is missing. Research alone
updates its evidence register after accepted observations; Design updates the affected rows here
and the eventual continuous contract. Closing this planning work closes neither audit.

| Existing owner or gap | Reusable accepted surface | Remaining acceptance disposition |
| --- | --- | --- |
| RA-01 / controlled start | R1 exact controlled start/contract; separate acquisition read all 30 status words as 0 | Keep the bounded PASS and this attempt-specific no-POISON result; do not demand a visible New/load flow excluded by 1A. Any route-relevant initial field still needs its declared provenance. |
| RA-02–RA-05 / route, admission, state | Controlled-start prefix through first Map19 Up acceptance, R2b/R2c static chain and R2d controlled PlayerReady | Retain each closed local boundary. The next unobserved segment begins with Map19 next-tile displacement and continues through the remaining castle/tower route to Map21 and natural admission; route-carried caller/state facts remain required. |
| RA-06 / complete battle | R3a–R3d static owners and existing local battle contracts | Keep complete original trace, viable fixed seed and reached action/results open. Remake victories do not close this row. |
| RA-07 / RA-12 / return and endpoint | R4a static spine; remake comparison described below | Research must establish original after-program effects and the first stable controllable 5B endpoint. Design then binds exact fields, input readiness and no pending program/modal/transfer/battle. |
| RA-08 / RA-09 / menu and dialogue | R2/R2a `FieldMenu` NotReached; separate acquisition has no FieldMenu callback and real messenger/gate text returns | Reuse those bounded results; exact post-447 wait entry remains Inferred. Close only remaining route-required coverage. No blanket field-menu or optional-dialogue task; prose/captures stay private. |
| RA-10 / persistence | Accepted 6A exclusion | No save implementation or durability observation is needed for this milestone. H4 must check restart-to-admitted-state and absent user save/resume surfaces. |
| RA-11 / original presentation | Static presentation/resource owners plus accepted messenger/gate consumers and Map19 control | Reuse observed returns/readiness without treating them as all visual/audio completions. Remaining required 8D identities/order, completion, readiness and provenance stay open. Asset availability alone does not prove presentation consumption. Old hardware gaps are not milestone requirements. |
| Design contracts and product choices | Battle-functions contract and ADR 0010 profile | Keep these closures. The missing continuous contract and H4 definitions/execution remain separate gates, not a reason to reopen closed subsystem contracts. |

**Confirmed remake scope:** accepted outcome implementation
[`8a581a82`](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/8a581a82e297ea2947cc9837e661752163d2806d)
and the [exploration execution owner](../../../remake/docs/exploration-programs.md#battle01-outcome-after-program-and-return)
provide a continuous common session through victory or ordinary defeat and usable field return.
Accepted R4a comparison
[`26e91107`](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/26e91107160b2c6f088b01ad31704ce9282fcba6)
consumes the unchanged R4a fixture for program/join/flag/return order. Its comparison reads static
source expectations, not a natural original observation. Initial accounting/seeds and ordinary-defeat
Map3 egress remain controlled inputs. Defeat coverage is reusable implementation coverage, not an
additional original-reference winning trace required by 4A/5B.

**Unknown original fidelity / explicit implementation limits:** the
[capability owner](../../../remake/docs/capability-status.md#milestone-and-deferred-surfaces) retains
natural caller/accounting/seed/egress, complete presentation and H4 gaps. EGRESS and wider action,
status, item/setup-event and other unsupported consumers do not become work items unless the accepted
route/action set requires them. Modern white fades, mosaic, shiver and project-authored cues are
implemented presentation services; their presence alone does not establish original cue identity,
semantic order, completion or 7C compliance. Compare those 8D boundaries. Original pixel/frame/audio
waveform differences are outside the selected domain; no exact backend or hardware capture is required.
Do not exclude a gameplay-affecting difference or private-original content requirement as visual polish.

**Independent 7C audio gap:** the [Research presentation audit](../../research/map3-battle01-audit.md#presentation-sufficiency-under-8d)
and [remake presentation owner](../../../remake/docs/exploration-programs.md) identify
`MUSIC_JOIN`/`MUSIC_SAD_JOIN` as project-authored chord loops. An original command ID and an actual
`AudioStreamPlayer` start/fade/stop do not establish private-original asset identity/provenance or
its binding to the consumed cue. Accepted Map19 acquisition and 8D semantic comparisons do not close
that content gap; 9A/10A do not waive it.

**Confirmed product implementation and direct observation:** accepted
[9A implementation](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/445) supplies shared
configurable keyboard/gamepad actions across exploration, dialogue/choices, battle and return,
Confirm/Cancel convention swapping, reduced white flashes and instant/adjustable text. The
[capability owner](../../../remake/docs/capability-status.md) and
[native 9A observation owner](../../../remake/docs/development-and-verification.md#native-9a-observation)
define the accepted boundary and reproduction. Startup settings expose these choices; there is no
in-game settings UI or automatic settings save. Reduced-flash suppresses the white overlay while
retaining service duration and the same cue token/kind completion. Text reveal preserves the real
acknowledgement and choice waits.

Accepted direct observations cover default and remapped/swapped keyboard/gamepad, two authored
worlds/actors, paired flash/text behavior, and a private continuous session from opening through
Battle01 victory to usable return with remapped/swapped gamepad. The private case uses instant text and ordinary flashes;
the authored cases own the paired accessibility comparison. These are injected actual Godot input
events, not physical controller-driver or hot-plug acceptance. Complete export packaging remains
unverified. These existing results are reused without rerunning them. Product 9A is implemented and
directly observed; composition into continuous H4 definitions and their execution remain open.
The intentional 9A/10A deviations do not establish natural original continuity or complete 8D acceptance.

Accepted M5 at
[`9301ddac`](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/9301ddac0e07fd2c5a87caeb19c69179025d0698)
retired legacy reference consumers while retaining applicable movement comparisons. Preserve the
completed #431 presentation-input failure at
`PrivateExplorationTests.PrivatePresentationBytesAndRequiredSpriteLinksAreAdmittedBeforeStartup`
and its separate complete-private-world rerun (1 passed, 0 skipped). Neither overwrites the other;
neither proves 8D. No old runtime or aggregate suite is reinstated by this plan. The separate #434
map-exploration mirror label failure (13 English / 11 Chinese) is not this ledger's correction scope.

### Dependency and completion boundaries

This is an acceptance dependency map, not a dispatch queue. Each future Issue needs a named executor,
exact paths and independent main-gate admission under the
[Project lifecycle](../../operations/github-project-governance.md#task-lifecycle). Preparation may
proceed using accepted static facts; missing runtime values stay unfilled.

| Surface and accountable owner | Entry/dependency | Completion and stopping condition |
| --- | --- | --- |
| Disabled old replay path — main-gate | Preserved capability/lineage owner; no launch | Disposition belongs to the milestone Issue. Recovery, runtime validation and transport implementation do not gate 8D/H4. Separate concrete removal/replacement work only if needed; no budget reset. |
| Original behavioral evidence — Research | Accepted static facts and controlled-start prefix through first Map19 control; specific unresolved gameplay claims | Establish downstream natural route/caller/accounting/RNG, battle results, after-program and 5B state using an independently admitted evidence method. Name fields/checkpoints and why existing evidence is insufficient before any new observation. Preserve Unknowns and typed failures; no dependency on the disabled runner. |
| Continuous-scenario contract — Design | Accepted Research values for every asserted continuous boundary | Proposed `docs/design/contracts/map3-battle01-continuous-scenario.md` composes admitted start, natural route/admission, winning logical trace, after-program and exact endpoint. Prepare structure now; final evidence-bound acceptance waits for the required observations. Derive associations from evidence, never all 26 aggregate Map3 rows. |
| H4 definitions — Design | Continuous contract plus accepted Research comparison domains/provenance and ADR 0010 deviations | Specify each layer's input, observation, expected value/owner, exactness/tolerance, failure/unavailable and cleanup rules. Future `schemas/h4/` and `tests/fixtures/h4/` identities/registrations are selected by that slice, not invented here. Definitions can be prepared alongside the contract but cannot claim missing evidence. |
| 9A input/accessibility — accepted Remake capability; Design/H4 composition remains | Accepted implementation and direct observations above; independent of original replay recovery | Reuse the implemented settings and bounded input/state/projection results. Compose the accessibility assertions into the future continuous H4 definitions and execute them; report accessibility variants and their state equivalence separately. Physical controller-driver/hot-plug and export limitations remain. |
| H4 implementation and execution — Remake/harness | Definitions accepted on main; required supported engine behavior and private inputs available | Consume frozen accepted evidence without launching the original to generate a golden. Run actual state/input/projection and required private comparisons, report every applicable layer and deviation, including failures/unavailable. No screenshots for Godot acceptance. |
| Final milestone review — main-gate | Research and Design closures plus successful applicable H4 execution | Independently accept the continuous playable 5B endpoint and current 8D profile. A contract, preflight, replay, engine test or child Issue closure alone is insufficient. |

The [capability owner](../../research/original-reference-replay-capability.md) supplies a fixed
33-physical-row transport recipe, not an arbitrary scenario runner. The
[scenario API](../../research/original-reference-replay-scenario-api.md) supplies a data-only descriptor
and passive-observer protocol; its generic sample and synthetic artifact identities are not a real
movie or scenario execution. Both owners are accepted at
[`f00c20f3`](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/f00c20f317b3e1c5c63a858df5e5b5abcb4a309d).
If a new evidence method needs transport, assess reusable containment, receipt and observer policy;
changing a CLI entry point does not complete the missing transport or grant a new launch lineage.

### Original evidence methods and exhausted admission

[ADR 0015's method distinction](../../decisions/0015-original-reference-replay-and-h4-boundary.md#distinguish-interactive-acquisition-frozen-replay-and-remake-h4)
controls all runtime questions here:

- **Interactive acquisition:** Research's operator/host may choose explicit ordinary controller
  inputs from original observations; Lua delivers inputs and observes, without choosing the route
  or implementing mechanics. Preserve actual delivered frames, including neutral/bootstrap frames,
  and distinguish callback facts from paused frame-end state. Controlled R1 initialization and final
  restoration are declared interventions; no state injection occurs between R1 service/scratch
  restoration and final cleanup.
- **Frozen original replay:** immutable non-adaptive inputs and passive observer rules apply to this
  method. A recording is not a replay PASS. Freezing/replaying acquisition requires a separate
  determinism need and independent admission; it is not an automatic next step.
- **Remake H4:** consumes independently accepted Research evidence through accepted comparison
  definitions; it cannot generate original truth inside the comparison. A remake observer choosing
  actions from live state supplies implementation evidence, not original evidence.

The one additional interactive acquisition is complete: historical controlled starts are **3**,
including the preserved #460/#465 **FAIL** results. No fourth start, retry, replay, preparation or
budget reset is authorized. The disabled replay ordinal-1/2 lineage below is separate and unchanged;
its missing genuine ledger/receipts and ordinal-2 timeout/cleanup failure are not repaired by the
successful bounded acquisition. Any future runtime question needs a new explicit purpose/method and
lineage/budget decision; the question table grants no launch allowance.

### Original replay lineage and launch admission

The capability owner fixes consumed diagnostic ordinal 1 to candidate
`9F8417BC1A515FEB5D9466DCC1BC489B981D97741E44518D572E6B0E63380BDF` and receipt
`BDE38876750E51E59CF1D2897495EFFD8EE42955F7FE87C3F12A9DB853C14CA6`.
The [current lineage hard stop](../../research/original-reference-replay-capability.md#current-lineage-hard-stop)
also records recovered Research and independent main-gate tool outputs reporting an ordinal-2
process start, completed timeout FAIL and cleanup failure, followed by explicit prohibition of
ordinal 3/retry/reset. These saved outputs are coordination records, not recovered receipt/ledger
bytes or original-game evidence. The bounded review found no subsequent actual ordinal-3 launch;
it does not establish a complete history or an unused diagnostic.

Any separately authorized recovery must locate the full genuine ledger and both actual receipts, verify existing identities
and relationships with the owning validator, and preserve all failures. Synthetic scratch,
reconstructed JSON, an empty ledger, saved output or a new output root cannot replace those bytes.
The old physical checkout is absent and actual bytes remain unavailable. This plan performs no new
private search or runtime validation. The disabled path cannot resume without full recovery and an
independent main-gate lineage/budget disposition; neither missing files nor recovery itself resets
consumption or overrides the prior prohibition.

The recorded candidate preflight `B003732B61A375C7980BDC1F328E5C8B00553E6F38E669746041E9E6BC7BE0EA`
returned PASS with `ProcessStarts=0`. This is a retained preflight result, not a launch receipt or
runtime-compatibility proof, and is not rerun by this docs-only correction. No ordinal may be assumed
available. Ordinal 3 still requires a single same-candidate ordinal-2 PASS, actual receipt-hash
validation and matching replay digest; recovering a reported ordinal-2 FAIL cannot meet that condition.
No extra diagnostic, fourth launch or task/wrapper/scenario/transport reset is authorized. A genuinely
separate scenario slice needs explicit lineage/budget adjudication under ADR 0015, not an assumed
fresh allowance.

### Conditional runtime questions

These are admission dossiers for concrete acceptance gaps, not authorization to run. ADR 0014/0016
require caller dependence, material contract impact and inability to use an existing batched rail
before creating any new H3 fixture. Prefer extending an accepted rail; no per-NPC or per-Unknown work.
ADR 0015's method distinction above applies: only frozen replay requires a non-adaptive input trace;
interactive acquisition permits explicit observation-based controller choices. Both retain private
outputs, typed callback/exit/cleanup failures and their independently admitted limits. No allowance remains.

| Question and acceptance impact | Existing static evidence / rails to reuse | Independent admission and stop boundary |
| --- | --- | --- |
| Is a new behavioral observation necessary and admissible? | Reuse accepted facts/rails first; declare the missing gameplay assertion and required checkpoints. The disabled transport is not a prerequisite. | Independent ADR 0014/0015/0016 method and lineage/budget decision before launch; retain the old prohibition and stop at the named typed failure or endpoint. |
| After accepted first Map19 Up input, does next-tile displacement and the remaining castle/tower route naturally carry the real caller/accounting/RNG state into Battle01, and complete the selected winning route through a controllable 5B endpoint? This determines 2A/3A/4A/5B and RA-02–RA-07/RA-09/RA-12. | Accepted Map19 terminal and R1/R2/R2a/R2d observation fields; R2b's remaining royal/tower programs and F605/F607/F608/F401/F256 callers, then R2c/R3a–R3d/R4a static fixtures. R2d's bridge cannot prove natural state across the remaining route; R4a stops before exploration executes. | Research first names the unresolved caller/setup/program-return fields and why the Map19 observation plus static topology are insufficient. Prefer existing batched rails; new fixtures need a rail-reuse finding. Explicitly select acquisition or frozen replay under ADR 0015 and obtain separate admission; preserve controlled-start provenance and forbid live state injection. Stop at the first typed failure, admitted limit or declared endpoint; no execution is currently authorized. |
| Which reached presentation cues, order, completion and input-ready state does 8D require? This determines RA-11 and H4 layer 9. | Reuse accepted messenger/gate real text returns and Map19 input acceptance, plus dialogue/window, graphics/audio/resource/program owners. Exact post-447 wait entry remains Inferred; later cues and actual host consumption are not established by IDs/returns alone. | Name the reached cue/resource, caller, dispatch/consumer completion or acknowledgement, associated state effect and blocked/available input relation. Persistent music needs start/replacement/stop semantics where reached, not a fictitious end event. Reuse evidence first; only a material unresolved assertion can justify separately admitted observation. Hardware APIs/clocks are not prerequisites; 7C asset provenance remains independent. |

Only independently accepted Research evidence on main closes its explicitly observed bounded claims. Keep private
capture payloads and detailed receipts ignored. Complete original-reference playback is still not
H4 PASS; remake parity requires the separately accepted definition and execution path above.

## H4 Composition Rules

The future continuous adapter should report independently observable layers rather than one pass/fail
blob:

1. admitted start-state identity and provenance;
2. ordered input and exploration handoffs;
3. map, setup, event, program, dialogue, roster, and flag transitions;
4. Battle 01 admission and initialized encounter state;
5. ordered turn, movement, target, player-action, AI-action, RNG, resolution, replay, and after-turn
   traces;
6. controller victory state and after-battle program/handoff trace;
7. exact final scenario state at the product-selected observable endpoint;
8. selected save exclusion and 7C private-local asset identity/provenance assertions;
9. 8D reached scene/dialogue/animation/audio identities and semantic order, actual host consumption,
   completion/acknowledgement and input readiness, with accepted reference provenance and
   licensing-safe public reports; no hardware-exact assertions;
10. separately named expected deviations.

Each layer must reference its accepted comparison definition and Research evidence/provenance;
a bounded acquisition result does not itself supply an H4 fixture. The continuous adapter must not replace
subsystem fixtures, copy their expected numbers into engine-specific tests, require original RAM/ROM
addresses in the remake, or publish private original text, graphics, audio, or captures.

Definition readiness requires the complete accepted surface and executable check definitions.
Milestone acceptance additionally requires their successful execution against the remake. A ready
definition is not a passed comparison, and the implemented continuous route is not a substitute for
the missing original reference or any required 8D layer.

### Initial deviation inventory mapped to H4

This is the acceptance mapping of ADR 0010 section 10's existing inventory, not a new deviation
ledger. Layer numbers refer to this document's ten-layer list above; the ADR groups them differently.
Each result must remain visible in layer 10 even when it passes. The comparison owns fixture-domain
rules; a fixed round, actor sequence or receipt count never becomes production gameplay legality.

| Accepted deviation and owner | Affected H4 layers | Required expected result |
| --- | --- | --- |
| Controlled snapshot in place of visible New/load — 1A | 1, 2, 10 | Exact admitted snapshot/provenance matches; report omission of the visible flow. Do not describe it as natural New/load. |
| Exclude optional Map3 interactions/menus — 2A | 2, 3, 10 | Enumerate required reached route steps; show excluded optional scope. A mandatory interaction cannot be removed under this deviation. |
| Modern remappable input and accessibility — 9A | 2, 3, 5, 7, 9, 10 | Keyboard/gamepad produce the same logical decisions; swapped confirm/cancel retain their roles. Reduced-flash reaches the same completion without its suppressed cue; adjusted/instant text preserves acknowledgement and route results. Expose binding/convention/flash/text settings. Report these variants separately with state-equivalence checks; device cadence and altered visual timing are not exact-match assertions. |
| No user save/load/checkpoint/suspend — 6A | 1, 8, 10 | User persistence surfaces are absent; restart returns to admitted state. Harness reset is not a save feature. |
| Fixed seed/logical H4 trace; interactive play may diverge — 4A | 1, 2, 5, 7, 10 | Reference execution declares a Research-proven seed and immutable logical trace; ordinary controls remain available and valid changed state/content follows engine rules. Never reseed live play to force parity. |
| Engine-native safe behavior outside admitted fixture domains — 10A | affected state/action layer, 10 | Explicitly name each out-of-domain case and safe/Unsupported result. Do not infer original behavior from rejection or use this deviation to exclude an in-domain failure. |

7C private-only assets and no redistribution are layer-8 product/provenance boundaries, not extra
fidelity deviations. Missing private inputs yield Unavailable. Other modernization needs a separate
accepted decision; it cannot enter this table merely because the current adapter behaves that way.

## Public and Private Boundary

The public readiness artifact may retain record IDs, fixture IDs, contract links, counts, aggregate
metadata, accepted hashes, state-field names, branch/order summaries, product-choice slots, and
synthetic H4 trace shapes already permitted by their owners.

The following remain private unless a separate license and distribution review accepts them:

- ROM, SRAM, save states, traces containing copyrighted payloads, and emulator captures;
- complete extracted map, dialogue, graphics, music, sound, font, or cutscene content;
- raw source-derived asset payloads and private canonical import graphs;
- any replacement asset whose provenance or redistribution terms are not accepted.

Phase 4 should consume public contracts and project-owned fixtures for its tracked implementation and
CI. The selected private-local 7C profile may load ignored original assets and captures locally after
their provenance/inventory is accepted, but those inputs must not become tracked dependencies,
uploads, public-CI requirements, or distributable build contents.

## Readiness Checklist

| Gate | Current result | Closure owner |
| --- | --- | --- |
| exact milestone and engine baseline accepted | PASS | ADR 0008 / ADR 0009 |
| product acceptance profile selected | PASS | ADR 0010 |
| controlled admitted Map 3 start state exact | PASS | `sf2-map3-admitted-start-runtime-v1` and [Map 3 Controlled Admission](../contracts/map3-controlled-admission.md); not a natural New/load claim |
| controlled R1 through first original Map19 Up acceptance | PASS (bounded) | [separate accepted acquisition](../../research/map3-messenger-acceptance.md#single-admitted-interactive-acquisition-result); stops before next-tile displacement, no older fixture update |
| natural route continuity after first Map19 control | OPEN | Research, then scenario contract; remaining castle/tower route to Map21 and natural battle admission |
| required exploration/dialogue/menu/UI scope exact | OPEN | Research plus route-required conditional contracts; ADR 0010 fixes the minimum-scope rule |
| static Battle 01 admission spine accepted | PASS | `sf2-map3-battle01-admission-static-v1`; natural admission remains the separate OPEN row below |
| explicit-bridge Battle 01 PlayerReady seam observed | PASS (bounded) | `sf2-map3-battle01-player-ready-runtime-v1`; controlled bridge, not natural continuity or post-seam play |
| natural Battle 01 admission exact | OPEN | conditional runtime evidence, then scenario contract; R2d does not close the remaining natural Map19-to-admission continuity |
| player-turn contract present | PASS | [Battle Functions Control Flow](../contracts/battle-functions-control-flow.md) |
| R3a–R3d static control/action/completion/finalization chain accepted | PASS | accepted static fixtures; reached branches/results remain open |
| complete playable Battle 01 trace exact | OPEN | conditional runtime evidence plus existing/extended battle contracts |
| R4a static victory/after-program/return spine accepted | PASS | `sf2-map3-battle01-victory-return-static-v1` |
| after-battle program reach, completion, and effects exact | OPEN | conditional runtime evidence, then scenario contract |
| observable endpoint shape selected | PASS | ADR 0010 option 5B |
| exact endpoint state evidenced | OPEN | conditional runtime evidence, then scenario contract |
| save scope selected | PASS | ADR 0010 option 6A excludes save/load/checkpoint/suspend |
| accessibility/input product interface selected | PASS | ADR 0010 option 9A |
| 9A input/accessibility configuration implemented and directly observed | PASS (bounded) | Accepted Remake capability and native 9A observation owners above; physical controller-driver/hot-plug and complete export are unverified |
| 9A accessibility assertions composed into continuous H4 and executed | OPEN | Design H4 definitions, then Remake/harness execution; separate variant results and state-equivalence checks |
| 7C private-local asset mode and no-public-distribution boundary selected | PASS | ADR 0010 |
| exact private asset/capture inventory and provenance accepted | OPEN | Research/private-input acceptance; no payload enters Git/public CI |
| public/distributable asset rights or replacements | BLOCKED OUTSIDE PRIVATE MILESTONE | Separate licensing/replacement decision before any public build |
| 8D gameplay/presentation-semantic tier selected | PASS | ADR 0010; supersedes former 8C hardware gate |
| required reached 8D evidence and comparison fields accepted | OPEN | Research/provenance, then continuous H4 contract; no pixel/waveform/cycle capture requirement |
| continuous H4 acceptance surface and executable check definitions accepted | OPEN | Design H4 definitions after the continuous contract |
| old replay lineage/runtime recovery | NOT REQUIRED / PATH DISABLED | Main-gate disposition; preserve failures, missing ledger/receipts and launch prohibition |
| necessary natural behavioral reference evidence accepted | OPEN beyond first Map19 control | Research; reuse the bounded acquisition and accepted facts; no runtime allowance remains |
| continuous-scenario contract accepted | OPEN | Design, consuming accepted Research |
| all applicable remake H4 layers and deviations executed successfully | OPEN | Remake/harness then independent main-gate; separate from definition readiness |
| main-gate readiness report accepted | OPEN | Main-gate |
| separate user Phase 4 start action | PASS | User authorization recorded in [`remake/README.md`](../../../remake/README.md); implementation start only, not milestone readiness |

The ledger remains **NOT READY** for eventual continuous-milestone acceptance while any required row
is open. Those rows do not block a separately user-authorized concrete implementation slice by default.

## Evidence Matrix

Fixture rows retain their own limits; the separate Map19 observation does not update their projections.

| Ledger statement | Classification | Accepted owner | Boundary retained |
| --- | --- | --- | --- |
| controlled Map 3 start reaches the first exploration wait with an exact bounded state | **Contract-ready / bounded runtime** | `sf2-map3-admitted-start-runtime-v1`, [Map 3 Controlled Admission](../contracts/map3-controlled-admission.md) | Not a natural player-visible New/load flow or later Map 3 route |
| 26 Map 3 source-path records exist and are aggregate-owned | **Confirmed indexed inventory** | `sf2-map-data-static-v1`, [map-data research](../../research/map-data-inventory.md) | Not route chronology, reachability, effects, or automatic future associations |
| natural opening and messenger-acceptance prefixes are observed | **Contract-ready bounded runtime prefixes** | `sf2-map3-battle01-natural-route-runtime-v1`, `sf2-map3-messenger-acceptance-runtime-v1` | End at program-entry/follower-ready boundaries; later continuity is unproved and `FieldMenu` is NotReached |
| controlled R1 through real messenger/gate/F604/north-warp/init returns reaches first Map19 Up acceptance | **Confirmed bounded original observation** | [Research result/provenance](../../research/map3-messenger-acceptance.md#single-admitted-interactive-acquisition-result) and [audit disposition](../../research/map3-battle01-audit.md#interactive-acquisition-reached-map19-admission-consumed) | Map19 `(26,30)` before next-tile displacement; exact post-447 wait entry Inferred; downstream continuity, frozen replay and complete 8D/H4 unproved; older fixtures unchanged |
| R2b/R2c legal route, unlock, admission, and initialization topology exists | **Contract-ready static chain** | `sf2-map3-castle-battle-unlock-static-v1`, `sf2-map3-battle01-admission-static-v1` | Not natural execution, caller order, cutscene execution, initialized snapshot, or first actor |
| explicit-bridge continuation reaches first Battle 01 PlayerReady | **Confirmed bounded runtime** | `sf2-map3-battle01-player-ready-runtime-v1`, [admission research](../../research/map3-battle01-admission.md) | Natural R2a-to-R2b continuity, wholly natural snapshot/actor, post-seam play, victory, presentation, and complete 8C remain Unknown |
| R3a–R3d turn, action/effect, completion, replay, and finalization topology exists | **Contract-ready static chain** | the four accepted R3 static fixtures and linked research owners | Not reached player/AI/action/results, replay, next turn, multi-round play, or victory |
| 15 battle-functions records have accepted static evidence and one bounded design contract | **Contract-ready** | [Battle Functions Control Flow](../contracts/battle-functions-control-flow.md), `sf2-battle-functions-static-v1` | No camera-owner overlap and no runtime/input/presentation generalization |
| R4a victory, after-program, return, SwitchMap, and exploration-call spine exists | **Contract-ready static chain** | `sf2-map3-battle01-victory-return-static-v1`, [Battle Cutscene Routing](../contracts/battle-cutscene-routing.md) | Victory/program reach and completion, flags/join outcomes, exploration re-entry, stable endpoint, R4b, and H4 remain open |
| local battle contracts can be composed conceptually | **Synthesis-ready** | [Tactical Battle Loop](tactical-battle-loop.md) and linked contracts | Not a complete predictive Battle 01 simulation or scenario golden |
| Godot/C#, milestone, product profile, and deferral policy are selected | **Accepted decisions** | ADR 0008 / ADR 0009 / ADR 0010 / ADR 0016 | Bounded implementation authorization does not imply continuous-milestone, MCP, redistribution, or evidence closure |
| route class, endpoint shape, save exclusion, UI, private assets, RNG policy, 8D parity, and deviations | **Accepted product decisions** | ADR 0010 | Exact scenario values, natural chronology, private capture provenance, and parity facts remain Research/H4 gaps |
