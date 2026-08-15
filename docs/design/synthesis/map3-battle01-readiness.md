# Map 3 to Battle 01 Readiness Ledger

- Status: **NOT READY** for Phase 4 implementation
- Audit date: 2026-08-14
- Accepted-main audit base: commit `21f98cfc9dee5b3589d0612e1058be5a9666fd3a`, tree
  `6eb4208567f403685c303e9c5f1145aeadf67974`
- Milestone owner: [ADR 0009](../../decisions/0009-first-phase4-playable-slice.md)
- Tooling boundary: [ADR 0008](../../decisions/0008-godot-csharp-cli-first-remake-tooling.md)
- Scope: Layer B readiness accounting for one continuous playable scenario from an admitted Map 3
  start through observable completion of Battle 01

## Judgment Boundary

This document is a readiness ledger. It does not own original-game evidence, define a new scenario
contract, select a product experience, authorize Phase 4, or replace the fixtures and contracts it
links. Its purpose is to state what accepted `main` can already support, what remains open, who must
own each closure, and what the later Phase 4 start gate must inspect.

The current judgment is **NOT READY** because accepted evidence does not yet join the controlled Map
3 handoff, a natural chronological exploration route, Battle 01 admission, a complete playable
battle, the after-battle program, and one exact observable ending state.

The following distinctions are normative:

- a controlled helper or debug seam is not a natural-story route;
- a static source graph is not an observed chronological playthrough;
- a fixture-local H4 surface is not an end-to-end scenario golden;
- an indexed file is not automatically a future design association;
- a numeric ID or source label is not player-facing meaning;
- a private original asset is not a distributable remake asset;
- a readiness closure is not authorization to create `remake/` or begin Phase 4.

No unmerged Research result contributes to this ledger. A future update may consume new evidence only
after it is accepted on `main`.

## Readiness Classification

Every dependency is assigned one or more of these exact classifications:

| Classification | Meaning in this ledger |
| --- | --- |
| **Contract-ready** | Accepted implementation-neutral contracts close the named local input, order, state, or output boundary. This does not imply continuous-scenario readiness. |
| **Synthesis-ready** | Accepted owners can support a bounded Layer B explanation without creating new evidence or a scenario-wide claim. |
| **Missing research owner** | Accepted `main` lacks the natural-caller, runtime, route, persistence, or presentation evidence required by the milestone. |
| **Missing design contract** | Accepted research exists, but no evidence-bound design contract yet owns the implementation-neutral surface required by the milestone. |
| **Explicit product decision** | The answer is a remake scope, experience, asset, accessibility, fidelity, or deviation choice rather than a recoverable original-game fact. |

A row may be contract-ready locally and still contain a scenario-level research or decision gap.
That is expected: this milestone requires composition, not merely the existence of subsystem files.

## ADR Gate

[ADR 0008](../../decisions/0008-godot-csharp-cli-first-remake-tooling.md) accepts Godot 4.7.1 .NET,
C#, a CLI-first toolchain, a plain-C# deterministic domain layer, and a thin Godot adapter. It does
not install Godot, select an MCP adapter, choose distributable assets, create a remake project, or
authorize implementation. The first implementation acceptance profile remains CLI-only.

[ADR 0009](../../decisions/0009-first-phase4-playable-slice.md) accepts exactly one first milestone:
a continuous playable scenario from Map 3 through **completion** of Battle 01. It requires Research
and Design gap closure, a main-gate readiness report, and a separate user start action. Battle entry,
initialization, or an isolated mechanic cannot satisfy the milestone.

This ledger therefore remains **NOT READY** until all closure rows below are accepted on `main` and
the product-choice slots are resolved by a later user-accepted decision.

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

### Battle-functions contract gap

`sf2-battle-functions-static-v1` directly binds exactly 15 research-index records. All 15 are
currently unassociated:

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
for the future battle-functions contract.

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

## Continuous-Scenario Dependency Matrix

| Scenario segment | Accepted owner surface | Readiness classification | Required closure |
| --- | --- | --- | --- |
| admitted Map 3 start | [New-Game State Initialization](../contracts/new-game-state-initialization.md), [Save System](../contracts/save-system.md), [Story Progression](story-progression.md) | **Contract-ready** controlled initialization and Map 3 handoff; **Missing research owner** for exact admitted snapshot; **Explicit product decision** for entry UX | identify start provenance and every scenario-relevant map, position, facing, flag, party, stat, item, spell, gold, difficulty, RNG, and time field; decide whether entry is a canonical snapshot, visible New flow, or load |
| Map 3 setup and content | [Map Setup Data](../contracts/map-setup-data.md), [Map and Exploration](../contracts/map-exploration.md), accepted aggregate map inventory | **Contract-ready** selectors and generic structures; **Missing research owner** for selected Map 3 rows and chronology; **Missing design contract** for scenario content | observe the selected setup/event/program chain from the admitted state and add a dedicated evidence-bound scenario/data contract without bulk-associating aggregate rows |
| exploration loop and input | [Exploration Control Flow](../contracts/exploration-control-flow.md), [Input System](../contracts/input-system.md), [Map Entry Routing State](../contracts/map-entry-routing-state.md) | **Contract-ready** local priority/handoff rules; **Missing research owner** for natural inputs and results; **Explicit product decision** for controls | record the route's ordered player inputs and natural loop handoffs; choose platform mapping, repeat/cancel/accessibility policy without attributing it to the original |
| dialogue and interaction | [Dialogue System](../contracts/dialogue-system.md), [Sprite Dialogue Property Data](../contracts/sprite-dialogue-property-data.md), [Text and Font System](../contracts/text-and-font-system.md), [Portrait Window State](../contracts/portrait-window-state.md) | **Contract-ready** command/storage/window seams; **Missing research owner** for route content/effects; **Explicit product decision** for visible text and localization | identify required dialogue/interaction programs, cursor/state effects, and completion boundaries; decide whether original text is private-only and what distributable replacement/localization appears |
| field menu and UI | [Exploration Control Flow](../contracts/exploration-control-flow.md), [Window System](../contracts/window-system.md), [UI Layout Data](../contracts/ui-layout-data.md), [UI Graphics Asset Data](../contracts/ui-graphics-asset-data.md) | **Contract-ready** handoff/layout/resource seams; **Missing design contract** if the route requires FieldMenu behavior; **Explicit product decision** for required pages and presentation | explicitly include or exclude field-menu, item, status, option, and cancellation paths; if included, create a bounded field-menu control contract from accepted evidence |
| map resources and camera | [Map Layout Data](../contracts/map-layout-data.md), [Map Palette Data](../contracts/map-palette-data.md), [Map Tileset Data](../contracts/map-tileset-data.md), [Map Sprite Graphics Data](../contracts/map-sprite-graphics-data.md), [Map Entity Data](../contracts/map-entity-data.md), [Map Camera Update](../contracts/map-camera-update-control-flow.md) | **Contract-ready** private import and local service/control surfaces; **Explicit product decision** for visible fidelity and assets | select placeholder/licensed presentation and acceptance tier; do not make private original payloads distributable |
| map-to-battle admission | [Exploration Control Flow](../contracts/exploration-control-flow.md), [Map Entry Routing State](../contracts/map-entry-routing-state.md), [Battle Encounter Definition](../contracts/battle-encounter-definition.md), [Battle Cutscene Routing](../contracts/battle-cutscene-routing.md) | **Contract-ready** static handoffs; **Missing research owner** for natural route and cutscene effects; **Missing design contract** for scenario handoff | observe the exact map/setup/event/flag path into Battle 01, before/start cutscene execution, and first battle-ready state |
| Battle 01 encounter setup | [Battle Encounter Definition](../contracts/battle-encounter-definition.md), [Battle Control and Combatant Lifecycle](../contracts/battle-control-lifecycle.md), [Battlefield Navigation](../contracts/battlefield-navigation.md) | **Contract-ready** placement, terrain, activation, first turn, and controller seams; **Missing research owner** for natural complete encounter state | bind the natural entry snapshot to the exact roster/stats/items/spells/positions/flags and later-round state actually used by the scenario |
| player turn and battle menus | accepted [battle-functions research](../../research/battle-functions.md), [Input System](../contracts/input-system.md) | **Missing design contract** for the exact 15-record fixture set; **Explicit product decision** for required agency and UI | create `battle-functions-control-flow` as a separate accepted-evidence contract; decide required player actions, cancel paths, optional menus, and platform controls |
| AI and navigation | [Battle AI Decision](../contracts/battle-ai-decision.md), [Battlefield Navigation](../contracts/battlefield-navigation.md) | **Contract-ready** bounded algorithms; **Missing research owner** for complete naturally reached multi-turn decisions | capture every reached Battle 01 AI/navigation branch and close only fixture gaps required by the accepted playthrough |
| action construction and resolution | [Battle Action Construction](../contracts/battle-action-construction.md), [Combat Resolution](../contracts/combat-resolution.md), [Spell Resolution](../contracts/spell-resolution.md), [Randomness](../contracts/randomness.md) | **Contract-ready** bounded subsets; **Missing research owner** for any reached unsupported branch; **Explicit product decision** for deterministic acceptance | choose an acceptance seed/input policy, record reached actions in order, and extend only owners needed for the playthrough; do not generalize subset fixtures |
| battle presentation | [Battle Scene Presentation](../contracts/battle-scene-presentation.md) and its dedicated graphics-data contracts | **Contract-ready** command/loader/static asset seams; **Missing research owner** if original rendered fidelity is required; **Explicit product decision** for visual/audio tier | decide state-only, structural, screenshot, animation, and audio expectations; gather runtime presentation evidence only for the selected fidelity tier |
| victory and after-battle | [Battle Control and Combatant Lifecycle](../contracts/battle-control-lifecycle.md), [Battle Cutscene Routing](../contracts/battle-cutscene-routing.md) | **Contract-ready** generic victory order; **Missing research owner** for natural victory, after-program effects, and final route; **Missing design contract** for observable completion | observe victory through the normal controller, after-battle MAPSCRIPT execution, return routing, and final scenario-relevant state |
| save/load scope | [Save System](../contracts/save-system.md), [Global Flag State](../contracts/global-flag-state.md), roster/state contracts | **Contract-ready** bounded in-process service/storage seams; **Missing research owner** if durability is required; **Explicit product decision** | explicitly exclude save/load or select checkpoint, in-process, suspend, or durable scope; if included, prove every used scenario field survives the chosen boundary |
| end-to-end H4 | all named subsystem fixtures and contracts | **Synthesis-ready** for a ledger; **Missing research owner** for continuous original trace; **Missing design contract** for scenario composition; **Explicit product decision** for observable acceptance | add one evidence-bound continuous-scenario contract that consumes, rather than weakens, subsystem fixtures and records declared deviations separately |

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

## Product-Choice Slots

All entries below are deliberately unresolved. They require a later user-accepted decision, proposed
as `docs/decisions/0010-map3-battle01-product-acceptance.md`, after the readiness ledger presents the
accepted evidence and viable choices.

| Decision slot | Current state | Decision must state |
| --- | --- | --- |
| admitted start | **Undecided** | canonical snapshot, visible New flow, or load; first observable state and required provenance |
| route | **Undecided** | mandatory maps, interactions, dialogue, menus, transitions, and allowed optional/backtracking behavior |
| completion endpoint | **Undecided** | exact success observation after Battle 01; controller return alone is not silently sufficient |
| save/load | **Undecided** | excluded, checkpoint-only, in-process, suspended battle, or durable cross-process behavior |
| player control and UI | **Undecided** | required field/battle menus, cancellation paths, device mapping, accessibility, and localization |
| assets | **Undecided** | placeholder or properly licensed replacements, provenance, distribution terms, and private-input separation |
| visual/audio parity | **Undecided** | state/structure, screenshot, animation/frame, palette, audio, and timing acceptance tier |
| RNG and action trace | **Undecided** | fixed seed/input trace, bounded invariant set, or another reproducible policy |
| intentional deviations | **Undecided** | every allowed rules, safety, UI, timing, asset, or presentation difference plus expected-deviation coverage |
| optional tooling | **Undecided and non-blocking** | whether any removable MCP adapter earns adoption after the ADR 0008 bakeoff; CLI gates remain authoritative |

No default in this table is implied by omission. Excluding a feature also requires an accepted
decision showing that the remaining scope is still one continuous playable milestone.

## Ordered Closure Plan

### Slice 0: durable readiness ledger

This document is the only owned artifact in its initial slice. It has no executable fixture
registration and no research-index `designContracts` association. After preliminary semantic review,
its only shared registrations should be the `docs/README.md` synthesis index and one pending entry in
`manifests/zh-translation-index.json`.

### Slice 1: battle-functions control contract

The next accepted-evidence design candidate is
`docs/design/contracts/battle-functions-control-flow.md`. It should consume only
`sf2-battle-functions-static-v1` and associate exactly the 15 `battle.functions.*` records listed in
this ledger. It must retain runtime input, complete cancellation, presentation, caller effects, and
natural Battle 01 reachability as separate or **Unknown**. This candidate is not started or owned by
the current slice.

### Slice 2: explicit product acceptance decision

A later decision must resolve the product-choice slots without rewriting them as original behavior.
Design may prepare alternatives, but user acceptance is required. Closing a choice does not start
Phase 4.

### Slice 3: Research closures

Research must merge dedicated evidence for:

1. exact admitted Map 3 start provenance and the natural chronological route into Battle 01;
2. selected setup/event/program/dialogue/menu/state effects along that route;
3. natural Battle 01 entry, including required before/start cutscene behavior;
4. one complete playable multi-round path through victory, identifying every reached player, AI,
   navigation, action, resolution, reward, and status branch;
5. after-battle program effects, return routing, and exact observable end state;
6. presentation or persistence only to the extent required by the accepted product decision.

Research may group these observations into one or more fixtures. Design must not name unaccepted
fixture IDs or consume unmerged conclusions in advance.

### Slice 4: continuous-scenario contract

After the required Research owners merge, the smallest coherent scenario contract is proposed as
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

- every required Research closure is accepted on `main`;
- the battle-functions and continuous-scenario contracts are accepted;
- all route-required conditional owners are accepted;
- the product decision resolves every slot or explicitly excludes it;
- distributable assets and the private-input boundary are closed;
- the complete H4 acceptance contract and matrix, executable check definitions, observable layers,
  tolerances, and declared expected deviations are fully specified and accepted on `main`;
- main-gate independently reports readiness.

Even then, Phase 4 begins only after the separate user start action required by ADR 0009.

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
8. selected save, visual, audio, and asset assertions;
9. separately named expected deviations.

Each layer must reference its owning accepted fixture. The continuous adapter must not replace
subsystem fixtures, copy their expected numbers into engine-specific tests, require original RAM/ROM
addresses in the remake, or publish private original text, graphics, audio, or captures.

Before Phase 4, readiness requires this acceptance surface and its executable check definitions to be
complete and accepted, not executed successfully against a remake that does not yet exist. Building
the remake adapter and obtaining H4 PASS results are Phase 4 implementation and milestone gates after
the separate user start action.

## Public and Private Boundary

The public readiness artifact may retain record IDs, fixture IDs, contract links, counts, aggregate
metadata, accepted hashes, state-field names, branch/order summaries, product-choice slots, and
synthetic H4 trace shapes already permitted by their owners.

The following remain private unless a separate license and distribution review accepts them:

- ROM, SRAM, save states, traces containing copyrighted payloads, and emulator captures;
- complete extracted map, dialogue, graphics, music, sound, font, or cutscene content;
- raw source-derived asset payloads and private canonical import graphs;
- any replacement asset whose provenance or redistribution terms are not accepted.

Phase 4 should consume public contracts and project-owned fixtures. Private immutable inputs may
support local verification but must not become tracked remake dependencies.

## Readiness Checklist

| Gate | Current result | Closure owner |
| --- | --- | --- |
| exact milestone and engine baseline accepted | PASS | ADR 0008 / ADR 0009 |
| admitted Map 3 start state exact | OPEN | Research, then scenario contract and product decision |
| natural Map 3 route exact | OPEN | Research, then scenario contract |
| required exploration/dialogue/menu/UI scope exact | OPEN | Research plus product decision; conditional contracts |
| natural Battle 01 admission exact | OPEN | Research, then scenario contract |
| player-turn contract present | OPEN | `battle-functions-control-flow` design slice |
| complete playable Battle 01 trace exact | OPEN | Research plus existing/extended battle contracts |
| after-battle effects exact | OPEN | Research, then scenario contract |
| observable endpoint selected and evidenced | OPEN | Product decision plus scenario contract |
| save scope selected and evidenced | OPEN | Product decision; Research only if included |
| placeholder/licensed assets accepted | OPEN | Product/licensing decision |
| visual/audio parity tier accepted | OPEN | Product decision; Research only where original fidelity is required |
| continuous H4 acceptance surface and executable check definitions accepted | OPEN | Scenario contract |
| main-gate readiness report accepted | OPEN | Main-gate |
| separate user Phase 4 start action | OPEN | User |

The ledger remains **NOT READY** while any required row is open.

## Evidence Matrix

| Ledger statement | Classification | Accepted owner | Boundary retained |
| --- | --- | --- | --- |
| controlled New action reaches MainLoop with current/egress map 3 | **Contract-ready / bounded runtime** | [Story Progression](story-progression.md), [Save System](../contracts/save-system.md) | Not a natural player-visible New flow or exact Map 3 start snapshot |
| 26 Map 3 source-path records exist and are aggregate-owned | **Confirmed indexed inventory** | `sf2-map-data-static-v1`, [map-data research](../../research/map-data-inventory.md) | Not route chronology, reachability, effects, or automatic future associations |
| static exploration, selector, map, input, dialogue, UI, and service seams exist | **Contract-ready local surfaces** | linked contracts in the dependency matrix | Not a complete Map 3 experience |
| Battle 01 placement, terrain, region activation, first turn, and generic outcome order exist | **Contract-ready static/runtime subsets** | [Battle Encounter Definition](../contracts/battle-encounter-definition.md), [Battle Control](../contracts/battle-control-lifecycle.md) | Debug entry skips cutscenes; complete natural encounter and endpoint remain open |
| 15 battle-functions records have accepted static evidence but no design contract | **Missing design contract** | `sf2-battle-functions-static-v1`, [battle-functions research](../../research/battle-functions.md) | No camera-owner overlap and no runtime/input/presentation generalization |
| after-battle route/program identities exist | **Contract-ready route structure** | [Battle Cutscene Routing](../contracts/battle-cutscene-routing.md) | Program effects, natural reachability, persistence, and visible sequence remain open |
| local battle contracts can be composed conceptually | **Synthesis-ready** | [Tactical Battle Loop](tactical-battle-loop.md) and linked contracts | Not a complete predictive Battle 01 simulation or scenario golden |
| Godot/C# and milestone are selected | **Accepted decisions** | ADR 0008 / ADR 0009 | No Phase 4 start, asset choice, MCP adoption, or product acceptance profile |
| route, endpoint, save, UI, assets, RNG, parity, and deviations | **Explicit product decisions** | future user-accepted ADR | Must not be inferred from original source labels or silence |
