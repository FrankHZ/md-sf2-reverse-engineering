# Map 3 to Battle 01 Readiness Ledger

- Status: **NOT READY** for eventual continuous-milestone acceptance; not a default blocker for a separately authorized implementation start
- Initial gap-audit date: 2026-08-14
- Initial gap-audit base: commit `21f98cfc9dee5b3589d0612e1058be5a9666fd3a`, tree
  `6eb4208567f403685c303e9c5f1145aeadf67974`
- Product-decision date: 2026-08-19
- Accepted-state refresh: 2026-08-20, commit `9a7cbcb44322e309ef10d8afac76d9a98be76f98`,
  tree `28c5f9c00a2b095d8b990eb8adc5249ede911704`
- Static-owner refresh: 2026-08-30, commit `1647ea15c3fabd900d451d5e2bc9c52699137a62`,
  tree `dddc48d1c0e1d87016b35d9d8f79bf40c1ceef3f`
- Milestone owner: [ADR 0009](../../decisions/0009-first-phase4-playable-slice.md)
- Tooling boundary: [ADR 0008](../../decisions/0008-godot-csharp-cli-first-remake-tooling.md)
- Product profile: [ADR 0010](../../decisions/0010-map3-battle01-product-acceptance.md)
- Start-policy amendment: [ADR 0016](../../decisions/0016-remake-start-evidence-deferral.md)
- Scope: Layer B readiness accounting for one continuous playable scenario from an admitted Map 3
  start through observable completion of Battle 01

## Judgment Boundary

This document is a readiness ledger. It does not own original-game evidence, define a new scenario
contract, select a product experience, authorize Phase 4, or replace the fixtures and contracts it
links. Its purpose is to state what accepted `main` can already support, what remains open, who must
own each closure, and what eventual continuous-milestone acceptance must inspect.

The product-choice slots and battle-functions contract are closed. Accepted runtime evidence now
closes the controlled Map 3 start and two bounded natural-route prefixes, while accepted static owners
close the R2b-through-R4a source/H1/ROM topology. The current judgment is still **NOT READY** for
eventual milestone acceptance because those owners do not join the last runtime prefix to natural
Battle 01 admission, a complete playable battle, an executed after-battle program, and one exact
observable ending state.

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

The 2026-08-20 refresh records the integrated battle-functions contract and ADR 0010. The 2026-08-30
refresh records only the later accepted R1/R2/R2a runtime owners and R2b-through-R4a static chain
against the named accepted-main base. Neither refresh reruns the complete original gap audit or
promotes an unmerged Research, ignored/private, failed-replay, or tooling conclusion.

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
`1A + 2A + 3A + 4A + 5B + 6A + 7C + 8C + 9A + 10A`. It selects a private-local original-asset
profile with no public redistribution and frame/audio/hardware-exact parity. Those choices close the
product slots but expand Research, private-provenance, and H4 work; they do not make the scenario ready.

This ledger therefore remains **NOT READY** until all remaining closure rows below are accepted on
`main` for the eventual continuous milestone. Its status does not itself reject a separately
user-authorized bounded implementation start under ADR 0016.

[ADR 0016](../../decisions/0016-remake-start-evidence-deferral.md) controls that separate start
policy. It preserves this ledger's eventual acceptance target while allowing a user-authorized
implementation slice to require only the accepted owners it concretely needs. Natural continuity,
original-reference replay, complete 8C capture, the continuous-scenario contract, and H4 completion
remain **OPEN** acceptance work rather than default pre-start blockers.

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
the completed flag, and return `D4=1`. The accepted Battle 01 H3 entry uses Debug Battle Test and
skips the before/start cutscenes. Neither fact establishes natural Battle 01 admission, after-battle
program effects, or the milestone's observable endpoint.

### Accepted scenario evidence-chain refresh

The accepted scenario-specific owners now form this exact bounded chain. Runtime and static labels are
not interchangeable:

| Stage | Accepted owner | Closed surface | Retained boundary |
| --- | --- | --- | --- |
| R1 | `sf2-map3-admitted-start-runtime-v1`; [fixture](../../../tests/fixtures/h3/map3-admitted-start-v1.json); [research owner](../../research/map3-admitted-start.md) | controlled Map 3 state through the first `WaitForEvent` | not a natural New/load route, later Map 3 behavior, raw-time golden, or 8C capture |
| R2 | `sf2-map3-battle01-natural-route-runtime-v1`; [fixture](../../../tests/fixtures/h3/map3-battle01-natural-route-v1.json); [research owner](../../research/map3-battle01-natural-route.md) | natural opening through `cs_5149A` entry-before-body; `FieldMenu` **NotReached** | messenger body, later route, effects, Battle 01 admission, and presentation remain open |
| R2a | `sf2-map3-messenger-acceptance-runtime-v1`; [fixture](../../../tests/fixtures/h3/map3-messenger-acceptance-v1.json); [research owner](../../research/map3-messenger-acceptance.md) | accepted messenger continuation through follower-ready `WaitForEvent`; `FieldMenu` **NotReached** | natural continuation into the static castle/battle route, later effects, and Battle 01 remain open |
| R2b | `sf2-map3-castle-battle-unlock-static-v1`; [fixture](../../../tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json); [research owner](../../research/map3-castle-battle-unlock.md) | legal source-derived route and unlock topology | natural execution, caller order, endpoint, and R2c continuity are **Unknown** |
| R2c | `sf2-map3-battle01-admission-static-v1`; [fixture](../../../tests/fixtures/h2/map3-battle01-admission-static-v1.json); [research owner](../../research/map3-battle01-admission.md) | legal admission/initialization spine | natural admission, cutscene execution, initialized snapshot, first actor, and player-ready state are **Unknown** |
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
| Map 3 setup and content | [Map Setup Data](../contracts/map-setup-data.md), [Map and Exploration](../contracts/map-exploration.md), R1/R2/R2a and R2b owners above | **Contract-ready static/bounded-runtime owners** for selected default setup, reached prefixes, and legal continuation topology; **Missing design contract** for continuous scenario composition | natural R2a-to-R2b execution, selected later effects, and complete route-required content remain **Unknown/Deferred**; no bulk association of 26 rows |
| exploration loop and input | [Exploration Control Flow](../contracts/exploration-control-flow.md), [Input System](../contracts/input-system.md), [Map Entry Routing State](../contracts/map-entry-routing-state.md), R2/R2a owners above | **Contract-ready bounded runtime prefix** and local handoffs; **Accepted product decision** for modern logical controls/accessibility | natural continuation beyond the follower-ready wait and later input/result chronology remain **Unknown/Deferred** |
| dialogue and interaction | [Dialogue System](../contracts/dialogue-system.md), [Sprite Dialogue Property Data](../contracts/sprite-dialogue-property-data.md), [Text and Font System](../contracts/text-and-font-system.md), [Portrait Window State](../contracts/portrait-window-state.md), R2a owner above | **Contract-ready static seams and bounded messenger runtime result**; **Accepted product decision** for private-local original text | later dialogue/program effects, visible prose, speaker/window presentation, timing, and continuity remain **Unknown/Deferred** |
| field menu and UI | [Exploration Control Flow](../contracts/exploration-control-flow.md), [Window System](../contracts/window-system.md), [UI Layout Data](../contracts/ui-layout-data.md), [UI Graphics Asset Data](../contracts/ui-graphics-asset-data.md) | **Contract-ready** handoff/layout/resource seams; `FieldMenu` is **Confirmed NotReached** in the accepted R2/R2a prefix; **Explicit product decision** for required pages and presentation | no field-menu contract is required by the reached prefix; a later naturally reached route requirement would trigger a bounded conditional owner |
| map resources and camera | [Map Layout Data](../contracts/map-layout-data.md), [Map Palette Data](../contracts/map-palette-data.md), [Map Tileset Data](../contracts/map-tileset-data.md), [Map Sprite Graphics Data](../contracts/map-sprite-graphics-data.md), [Map Entity Data](../contracts/map-entity-data.md), [Map Camera Update](../contracts/map-camera-update-control-flow.md) | **Contract-ready static import/local-control owners**; **Accepted product decision** for private originals and 8C | reached pixel/palette/frame/hardware behavior, private capture provenance, and exact tolerances remain **Unknown/Deferred** |
| map-to-battle admission | [Exploration Control Flow](../contracts/exploration-control-flow.md), [Map Entry Routing State](../contracts/map-entry-routing-state.md), [Battle Encounter Definition](../contracts/battle-encounter-definition.md), [Battle Cutscene Routing](../contracts/battle-cutscene-routing.md), R2b/R2c owners above | **Contract-ready static route/admission spine**; **Missing design contract** for continuous scenario composition | natural R2a-to-R2b-to-R2c continuity, caller order, cutscene execution, and first battle-ready state remain **Unknown/Deferred** |
| Battle 01 encounter setup | [Battle Encounter Definition](../contracts/battle-encounter-definition.md), [Battle Control and Combatant Lifecycle](../contracts/battle-control-lifecycle.md), [Battlefield Navigation](../contracts/battlefield-navigation.md), R2c/R3a owners above | **Contract-ready static encounter/control spine** | natural initialized snapshot, first actor, player-ready state, and later-round state remain **Unknown/Deferred** |
| player turn and battle menus | [Battle Functions Control Flow](../contracts/battle-functions-control-flow.md), [Input System](../contracts/input-system.md), R3a owner above | **Contract-ready static branch/request/local-output owners**; **Accepted product decision** for manual agency and UI | reached player/AI branch, command, movement, target, action, cancellation, and result remain **Unknown/Deferred** |
| AI and navigation | [Battle AI Decision](../contracts/battle-ai-decision.md), [Battlefield Navigation](../contracts/battlefield-navigation.md), R3a/R3b owners above | **Contract-ready algorithms and static caller topology** | actual AI branch, command, movement, target, result, and multi-round decisions remain **Unknown/Deferred** |
| action construction and resolution | [Battle Action Construction](../contracts/battle-action-construction.md), [Combat Resolution](../contracts/combat-resolution.md), [Spell Resolution](../contracts/spell-resolution.md), [Randomness](../contracts/randomness.md), R3b/R3c owners above | **Contract-ready bounded algorithms and static action/effect/completion topology**; **Accepted product decision** for one deterministic H4 trace | reached seed, actions, resolution/status/death/EXP/gold/drop/follow-up outcomes, replay, and next-turn dispatch remain **Unknown/Deferred** |
| battle presentation | [Battle Scene Presentation](../contracts/battle-scene-presentation.md), dedicated graphics-data contracts, and R3c/R3d owners above | **Contract-ready loader/static asset and replay/finalization topology**; **Accepted product decision** for private originals and 8C | reached scenes, frames, audio, hardware chronology, private captures, and exact tolerances remain **Unknown/Deferred** |
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
| route | **Accepted: 2A smallest Research-proven natural route** | R2/R2a own only the reached runtime prefix and R2b/R2c own static continuation; complete natural continuity, effects, and backtracking remain open |
| natural battle/cutscene | **Accepted: 3A chronology, with placeholder subclause superseded by 7C/8C** | R2c owns static admission topology only; natural admission, before/start execution, rendered timing, and first battle-ready state remain open |
| completion endpoint | **Accepted: 5B first stable controllable post-after-program state** | exact return map/location/state remains Research-owned; `D4=1` alone is insufficient |
| save/load | **Accepted: 6A excluded** | restart returns to the admitted snapshot; later save support is a separate milestone |
| player control and UI | **Accepted: 4A/9A manual agency and modern accessible logical controls** | exact reached actions/input trace and executable accessibility assertions remain open |
| assets | **Accepted: 7C private-local originals only** | ignored private provenance/inventory must close; public distribution remains blocked without rights/replacements |
| visual/audio parity | **Accepted: 8C frame/audio/hardware-exact** | full reached pixel/palette/frame/audio/chip/VInt/DMA/CRAM/VDP evidence and H4 definitions remain open |
| RNG and action trace | **Accepted: one deterministic H4 reference trace** | R3a–R3d own static control/action/completion/finalization topology only; viable seed and reached logical trace remain open, and ordinary interactive play is not scripted |
| intentional deviations | **Accepted: 10A explicit ledger** | controlled admission, optional scope, modern controls, no save, fixed reference trace, and out-of-domain engine behavior require named checks |
| optional tooling | **Deferred and non-blocking; no MCP adopted** | CLI gates remain authoritative; no tooling choice starts Phase 4 |

## Ordered Closure Plan

### Slice 0: durable readiness ledger

This document is the only owned artifact in its initial slice. It has no executable fixture
registration and no research-index `designContracts` association. After preliminary semantic review,
its only shared registrations should be the `docs/README.md` synthesis index and one pending entry in
`manifests/zh-translation-index.json`.

### Slice 1: battle-functions control contract

**CLOSED.** [Battle Functions Control Flow](../contracts/battle-functions-control-flow.md) consumes
only `sf2-battle-functions-static-v1` and associates exactly the 15 `battle.functions.*` records
listed in this ledger. Runtime input, complete cancellation, presentation, caller effects, and
natural Battle 01 reachability remain separate or **Unknown** as required.

### Slice 2: explicit product acceptance decision

**CLOSED.** [ADR 0010](../../decisions/0010-map3-battle01-product-acceptance.md) accepts the exact
profile recorded above without rewriting product choices as original behavior. Closing these choices
does not start Phase 4.

### Slice 3: accepted static chain and conditional runtime closures

The R1/R2/R2a runtime fixtures and R2b-through-R4a static fixtures listed above are now accepted.
Eventual continuous-milestone acceptance still requires bounded closure for:

1. natural R2a-to-R2b-to-R2c continuation and caller order into Battle 01;
2. selected later setup/event/program/dialogue/menu/state effects not closed by the bounded runtime prefix;
3. natural Battle 01 entry, including required before/start cutscene execution and initialized state;
4. one complete playable multi-round path through victory, identifying every reached player, AI,
   navigation, action, resolution, reward, and status branch;
5. after-battle program effects, return routing, and exact observable end state;
6. full reached 8C presentation and hardware behavior: pixel/palette output, frame cadence,
   animation/timing, audio waveform/chip/timing, VInt/DMA/CRAM/VDP and other observable behavior;
7. private reference-capture provenance, deterministic capture conditions, exact or field-specific
   tolerances, and licensing-safe public reporting.

Research may group these observations into one or more fixtures only when ADR 0014's immediate
three-part gate admits the caller-dependent question. Static owners already accepted on `main` must be
reused rather than reopened. Failed R2b and original-reference candidates remain non-evidence under
ADR 0015. Design must not name unaccepted fixture IDs or consume unmerged conclusions in advance.
This list is not a default implementation-start queue.

### Slice 4: continuous-scenario contract

After the runtime/natural closures actually required by the eventual milestone are admitted and
accepted, the smallest coherent scenario contract is proposed as
`docs/design/contracts/map3-battle01-continuous-scenario.md`. Its exact fixture and association set
must be derived from those accepted owners. It must not automatically claim the 26 Map 3 aggregate
rows or duplicate existing battle owners.

The contract should define the admitted state, ordered route, transition, complete accepted battle
trace, after-battle effects, observable endpoint, and H4 composition. It must preserve every local
fixture as the authoritative subsystem golden rather than copying selected expectations into one
weaker aggregate fixture.

### Slice 5: conditional contract extensions

Only the accepted route and product profile may trigger these:

- a field-menu control contract when the route requires it;
- bounded AI, combat, spell, save, dialogue, or presentation extensions for actually reached gaps;
- no blanket closure of unrelated optional content.

### Slice 6: final readiness update

This ledger may change from **NOT READY** to **READY FOR PHASE-TRANSITION DECISION** only after:

- every required runtime/natural closure is either accepted on `main` or explicitly excluded by an
  accepted owner without weakening the continuous milestone;
- the continuous-scenario contract is accepted (the battle-functions contract is already closed);
- all route-required conditional owners are accepted;
- the accepted ADR 0010 profile remains internally consistent with every scenario/H4 owner;
- the private-local asset inventory, provenance, ignored-input handling, and no-public-distribution
  boundary are closed; a distributable build remains separately blocked until rights/replacements exist;
- the complete H4 acceptance contract and matrix, executable check definitions, observable layers,
  tolerances, and declared expected deviations are fully specified and accepted on `main`;
- main-gate independently reports readiness.

The separate user start action required by ADR 0009 occurred on 2026-08-28 for bounded Phase 4
implementation under ADR 0016. It satisfies the implementation-start gate but does not change this
ledger's **NOT READY** status or waive any eventual continuous-milestone gate above.

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
9. 8C pixel/palette/frame cadence, animation/timing, audio waveform/chip/timing,
   VInt/DMA/CRAM/VDP, other reached hardware-observable assertions, deterministic capture conditions,
   exact or field-specific tolerances, and licensing-safe public report shape;
10. separately named expected deviations.

Each layer must reference its owning accepted fixture. The continuous adapter must not replace
subsystem fixtures, copy their expected numbers into engine-specific tests, require original RAM/ROM
addresses in the remake, or publish private original text, graphics, audio, or captures.

Before the continuous milestone is accepted, readiness requires this acceptance surface and its
executable check definitions to be complete and accepted, not executed successfully against a remake
that does not yet close this scenario. Building the continuous remake adapter and obtaining H4 PASS
results remain Phase 4 implementation and milestone gates after the already-recorded separate user
start action.

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
| natural Map 3 route exact | OPEN | Research, then scenario contract |
| required exploration/dialogue/menu/UI scope exact | OPEN | Research plus route-required conditional contracts; ADR 0010 fixes the minimum-scope rule |
| static Battle 01 admission spine accepted | PASS | `sf2-map3-battle01-admission-static-v1`; natural admission remains the separate OPEN row below |
| natural Battle 01 admission exact | OPEN | conditional runtime evidence, then scenario contract |
| player-turn contract present | PASS | [Battle Functions Control Flow](../contracts/battle-functions-control-flow.md) |
| R3a–R3d static control/action/completion/finalization chain accepted | PASS | accepted static fixtures; reached branches/results remain open |
| complete playable Battle 01 trace exact | OPEN | conditional runtime evidence plus existing/extended battle contracts |
| R4a static victory/after-program/return spine accepted | PASS | `sf2-map3-battle01-victory-return-static-v1` |
| after-battle program reach, completion, and effects exact | OPEN | conditional runtime evidence, then scenario contract |
| observable endpoint shape selected | PASS | ADR 0010 option 5B |
| exact endpoint state evidenced | OPEN | conditional runtime evidence, then scenario contract |
| save scope selected | PASS | ADR 0010 option 6A excludes save/load/checkpoint/suspend |
| accessibility/input product interface selected | PASS | ADR 0010 option 9A |
| accessibility observable checks composed | OPEN | Continuous H4 contract; deviations separate from the 8C exact reference run |
| 7C private-local asset mode and no-public-distribution boundary selected | PASS | ADR 0010 |
| exact private asset/capture inventory and provenance accepted | OPEN | Research/private-input acceptance; no payload enters Git/public CI |
| public/distributable asset rights or replacements | BLOCKED OUTSIDE PRIVATE MILESTONE | Separate licensing/replacement decision before any public build |
| 8C visual/audio/hardware parity tier selected | PASS | ADR 0010 |
| complete reached 8C evidence, capture domain, and tolerances accepted | OPEN | conditional Research/private-reference acceptance, then continuous H4 contract |
| continuous H4 acceptance surface and executable check definitions accepted | OPEN | Scenario contract |
| main-gate readiness report accepted | OPEN | Main-gate |
| separate user Phase 4 start action | PASS | User authorization recorded in [`remake/README.md`](../../../remake/README.md); implementation start only, not milestone readiness |

The ledger remains **NOT READY** for eventual continuous-milestone acceptance while any required row
is open. Those rows do not block a separately user-authorized concrete implementation slice by default.

## Evidence Matrix

| Ledger statement | Classification | Accepted owner | Boundary retained |
| --- | --- | --- | --- |
| controlled Map 3 start reaches the first exploration wait with an exact bounded state | **Contract-ready / bounded runtime** | `sf2-map3-admitted-start-runtime-v1`, [Map 3 Controlled Admission](../contracts/map3-controlled-admission.md) | Not a natural player-visible New/load flow or later Map 3 route |
| 26 Map 3 source-path records exist and are aggregate-owned | **Confirmed indexed inventory** | `sf2-map-data-static-v1`, [map-data research](../../research/map-data-inventory.md) | Not route chronology, reachability, effects, or automatic future associations |
| natural opening and messenger-acceptance prefixes are observed | **Contract-ready bounded runtime prefixes** | `sf2-map3-battle01-natural-route-runtime-v1`, `sf2-map3-messenger-acceptance-runtime-v1` | End at program-entry/follower-ready boundaries; later continuity is unproved and `FieldMenu` is NotReached |
| R2b/R2c legal route, unlock, admission, and initialization topology exists | **Contract-ready static chain** | `sf2-map3-castle-battle-unlock-static-v1`, `sf2-map3-battle01-admission-static-v1` | Not natural execution, caller order, cutscene execution, initialized snapshot, or first actor |
| R3a–R3d turn, action/effect, completion, replay, and finalization topology exists | **Contract-ready static chain** | the four accepted R3 static fixtures and linked research owners | Not reached player/AI/action/results, replay, next turn, multi-round play, or victory |
| 15 battle-functions records have accepted static evidence and one bounded design contract | **Contract-ready** | [Battle Functions Control Flow](../contracts/battle-functions-control-flow.md), `sf2-battle-functions-static-v1` | No camera-owner overlap and no runtime/input/presentation generalization |
| R4a victory, after-program, return, SwitchMap, and exploration-call spine exists | **Contract-ready static chain** | `sf2-map3-battle01-victory-return-static-v1`, [Battle Cutscene Routing](../contracts/battle-cutscene-routing.md) | Victory/program reach and completion, flags/join outcomes, exploration re-entry, stable endpoint, R4b, and H4 remain open |
| local battle contracts can be composed conceptually | **Synthesis-ready** | [Tactical Battle Loop](tactical-battle-loop.md) and linked contracts | Not a complete predictive Battle 01 simulation or scenario golden |
| Godot/C#, milestone, product profile, and deferral policy are selected | **Accepted decisions** | ADR 0008 / ADR 0009 / ADR 0010 / ADR 0016 | Bounded implementation authorization does not imply continuous-milestone, MCP, redistribution, or evidence closure |
| route class, endpoint shape, save exclusion, UI, private assets, RNG policy, 8C parity, and deviations | **Accepted product decisions** | ADR 0010 | Exact scenario values, natural chronology, private capture provenance, and parity facts remain Research/H4 gaps |
