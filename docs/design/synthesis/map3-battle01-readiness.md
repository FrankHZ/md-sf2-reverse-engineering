# Map 3 to Battle 01 Readiness Ledger

- Status: **NOT READY** for Phase 4 implementation
- Audit date: 2026-08-14
- Product-decision update: 2026-08-19
- Accepted-main audit base: commit `21f98cfc9dee5b3589d0612e1058be5a9666fd3a`, tree
  `6eb4208567f403685c303e9c5f1145aeadf67974`
- Milestone owner: [ADR 0009](../../decisions/0009-first-phase4-playable-slice.md)
- Tooling boundary: [ADR 0008](../../decisions/0008-godot-csharp-cli-first-remake-tooling.md)
- Product profile: [ADR 0010](../../decisions/0010-map3-battle01-product-acceptance.md)
- Scope: Layer B readiness accounting for one continuous playable scenario from an admitted Map 3
  start through observable completion of Battle 01

## Judgment Boundary

This document is a readiness ledger. It does not own original-game evidence, define a new scenario
contract, select a product experience, authorize Phase 4, or replace the fixtures and contracts it
links. Its purpose is to state what accepted `main` can already support, what remains open, who must
own each closure, and what the later Phase 4 start gate must inspect.

The product-choice slots and battle-functions contract are now closed, but the current judgment is
still **NOT READY** because accepted evidence does not yet join the controlled Map
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

[ADR 0010](../../decisions/0010-map3-battle01-product-acceptance.md) accepts the exact profile
`1A + 2A + 3A + 4A + 5B + 6A + 7C + 8C + 9A + 10A`. It selects a private-local original-asset
profile with no public redistribution and frame/audio/hardware-exact parity. Those choices close the
product slots but expand Research, private-provenance, and H4 work; they do not make the scenario ready.

This ledger therefore remains **NOT READY** until all remaining closure rows below are accepted on
`main`.

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

## Continuous-Scenario Dependency Matrix

| Scenario segment | Accepted owner surface | Readiness classification | Required closure |
| --- | --- | --- | --- |
| admitted Map 3 start | [New-Game State Initialization](../contracts/new-game-state-initialization.md), [Save System](../contracts/save-system.md), [Story Progression](story-progression.md) | **Contract-ready** controlled initialization and Map 3 handoff; **Accepted product decision** for a controlled admitted snapshot; **Missing research owner** for its exact values/provenance | identify every scenario-relevant map, position, facing, flag, party, stat, item, spell, gold, difficulty, RNG, and time field without presenting it as a canonical original New/load state |
| Map 3 setup and content | [Map Setup Data](../contracts/map-setup-data.md), [Map and Exploration](../contracts/map-exploration.md), accepted aggregate map inventory | **Contract-ready** selectors and generic structures; **Missing research owner** for selected Map 3 rows and chronology; **Missing design contract** for scenario content | observe the selected setup/event/program chain from the admitted state and add a dedicated evidence-bound scenario/data contract without bulk-associating aggregate rows |
| exploration loop and input | [Exploration Control Flow](../contracts/exploration-control-flow.md), [Input System](../contracts/input-system.md), [Map Entry Routing State](../contracts/map-entry-routing-state.md) | **Contract-ready** local priority/handoff rules; **Accepted product decision** for modern logical controls/accessibility; **Missing research owner** for natural inputs and results | record the route's ordered player inputs and natural loop handoffs; keep product mapping/repeat/accessibility distinct from original behavior |
| dialogue and interaction | [Dialogue System](../contracts/dialogue-system.md), [Sprite Dialogue Property Data](../contracts/sprite-dialogue-property-data.md), [Text and Font System](../contracts/text-and-font-system.md), [Portrait Window State](../contracts/portrait-window-state.md) | **Contract-ready** command/storage/window seams; **Accepted product decision** for private-local original text; **Missing research owner** for route content/effects | identify required dialogue/interaction programs, cursor/state effects, and completion boundaries; retain ignored private inputs and block public distribution without rights/replacements |
| field menu and UI | [Exploration Control Flow](../contracts/exploration-control-flow.md), [Window System](../contracts/window-system.md), [UI Layout Data](../contracts/ui-layout-data.md), [UI Graphics Asset Data](../contracts/ui-graphics-asset-data.md) | **Contract-ready** handoff/layout/resource seams; **Missing design contract** if the route requires FieldMenu behavior; **Explicit product decision** for required pages and presentation | explicitly include or exclude field-menu, item, status, option, and cancellation paths; if included, create a bounded field-menu control contract from accepted evidence |
| map resources and camera | [Map Layout Data](../contracts/map-layout-data.md), [Map Palette Data](../contracts/map-palette-data.md), [Map Tileset Data](../contracts/map-tileset-data.md), [Map Sprite Graphics Data](../contracts/map-sprite-graphics-data.md), [Map Entity Data](../contracts/map-entity-data.md), [Map Camera Update](../contracts/map-camera-update-control-flow.md) | **Contract-ready** private import and local service/control surfaces; **Accepted product decision** for private original assets and 8C parity; **Missing research owner** for complete reached visual/hardware behavior | establish ignored private asset/capture provenance and exact pixel/palette/frame/hardware acceptance without making original payloads distributable |
| map-to-battle admission | [Exploration Control Flow](../contracts/exploration-control-flow.md), [Map Entry Routing State](../contracts/map-entry-routing-state.md), [Battle Encounter Definition](../contracts/battle-encounter-definition.md), [Battle Cutscene Routing](../contracts/battle-cutscene-routing.md) | **Contract-ready** static handoffs; **Missing research owner** for natural route and cutscene effects; **Missing design contract** for scenario handoff | observe the exact map/setup/event/flag path into Battle 01, before/start cutscene execution, and first battle-ready state |
| Battle 01 encounter setup | [Battle Encounter Definition](../contracts/battle-encounter-definition.md), [Battle Control and Combatant Lifecycle](../contracts/battle-control-lifecycle.md), [Battlefield Navigation](../contracts/battlefield-navigation.md) | **Contract-ready** placement, terrain, activation, first turn, and controller seams; **Missing research owner** for natural complete encounter state | bind the natural entry snapshot to the exact roster/stats/items/spells/positions/flags and later-round state actually used by the scenario |
| player turn and battle menus | [Battle Functions Control Flow](../contracts/battle-functions-control-flow.md), accepted [battle-functions research](../../research/battle-functions.md), [Input System](../contracts/input-system.md) | **Contract-ready** static branch/request/local-output surface; **Accepted product decision** for manual agency and UI; **Missing research owner** for the complete naturally reached trace | identify the exact action families and cancellation paths reached by the accepted winning trace without generalizing fixture-local behavior |
| AI and navigation | [Battle AI Decision](../contracts/battle-ai-decision.md), [Battlefield Navigation](../contracts/battlefield-navigation.md) | **Contract-ready** bounded algorithms; **Missing research owner** for complete naturally reached multi-turn decisions | capture every reached Battle 01 AI/navigation branch and close only fixture gaps required by the accepted playthrough |
| action construction and resolution | [Battle Action Construction](../contracts/battle-action-construction.md), [Combat Resolution](../contracts/combat-resolution.md), [Spell Resolution](../contracts/spell-resolution.md), [Randomness](../contracts/randomness.md) | **Contract-ready** bounded subsets; **Accepted product decision** for one deterministic H4 reference trace; **Missing research owner** for its viable seed and reached unsupported branches | record reached actions in order and extend only owners needed for the playthrough; do not constrain other interactive play or generalize subset fixtures |
| battle presentation | [Battle Scene Presentation](../contracts/battle-scene-presentation.md) and its dedicated graphics-data contracts | **Contract-ready** command/loader/static asset seams; **Accepted product decision** for private-local originals and 8C frame/audio/hardware exactness; **Missing research owner** for complete reached parity | close pixel/palette/frame cadence, animation/timing, waveform/chip/timing, VInt/DMA/CRAM/VDP, private capture provenance, exact tolerances, and licensing-safe reporting |
| victory and after-battle | [Battle Control and Combatant Lifecycle](../contracts/battle-control-lifecycle.md), [Battle Cutscene Routing](../contracts/battle-cutscene-routing.md) | **Contract-ready** generic victory order; **Missing research owner** for natural victory, after-program effects, and final route; **Missing design contract** for observable completion | observe victory through the normal controller, after-battle MAPSCRIPT execution, return routing, and final scenario-relevant state |
| save/load scope | [Save System](../contracts/save-system.md), [Global Flag State](../contracts/global-flag-state.md), roster/state contracts | **Contract-ready** bounded service/storage seams; **Accepted product decision** to exclude milestone save/load/checkpoint/suspend | enforce restart-to-admitted-snapshot behavior and keep later save support outside this milestone |
| end-to-end H4 | all named subsystem fixtures and contracts | **Synthesis-ready** for a ledger; **Accepted product decision** for observable layers/deviations; **Missing research owner** for the continuous original/8C trace; **Missing design contract** for scenario composition and executable definitions | add one evidence-bound continuous-scenario contract that consumes, rather than weakens, subsystem fixtures and reports declared deviations separately |

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
without filling any Research-owned exact value.

| Decision slot | Accepted state | Remaining closure |
| --- | --- | --- |
| admitted start | **Accepted: 1A controlled admitted snapshot** | exact values and provenance remain Research-owned; it is not a canonical original New/load claim |
| route | **Accepted: 2A smallest Research-proven natural route** | exact ordered route, mandatory content, effects, and backtracking remain Research-owned |
| natural battle/cutscene | **Accepted: 3A chronology, with placeholder subclause superseded by 7C/8C** | exact natural admission, before/start effects, rendered timing, and first battle-ready state remain open |
| completion endpoint | **Accepted: 5B first stable controllable post-after-program state** | exact return map/location/state remains Research-owned; `D4=1` alone is insufficient |
| save/load | **Accepted: 6A excluded** | restart returns to the admitted snapshot; later save support is a separate milestone |
| player control and UI | **Accepted: 4A/9A manual agency and modern accessible logical controls** | exact reached actions/input trace and executable accessibility assertions remain open |
| assets | **Accepted: 7C private-local originals only** | ignored private provenance/inventory must close; public distribution remains blocked without rights/replacements |
| visual/audio parity | **Accepted: 8C frame/audio/hardware-exact** | full reached pixel/palette/frame/audio/chip/VInt/DMA/CRAM/VDP evidence and H4 definitions remain open |
| RNG and action trace | **Accepted: one deterministic H4 reference trace** | viable seed and logical trace remain Research-owned; ordinary interactive play is not scripted |
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

### Slice 3: Research closures

Research must merge dedicated evidence for:

1. exact admitted Map 3 start provenance and the natural chronological route into Battle 01;
2. selected setup/event/program/dialogue/menu/state effects along that route;
3. natural Battle 01 entry, including required before/start cutscene behavior;
4. one complete playable multi-round path through victory, identifying every reached player, AI,
   navigation, action, resolution, reward, and status branch;
5. after-battle program effects, return routing, and exact observable end state;
6. full reached 8C presentation and hardware behavior: pixel/palette output, frame cadence,
   animation/timing, audio waveform/chip/timing, VInt/DMA/CRAM/VDP and other observable behavior;
7. private reference-capture provenance, deterministic capture conditions, exact or field-specific
   tolerances, and licensing-safe public reporting.

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
- the continuous-scenario contract is accepted (the battle-functions contract is already closed);
- all route-required conditional owners are accepted;
- the accepted ADR 0010 profile remains internally consistent with every scenario/H4 owner;
- the private-local asset inventory, provenance, ignored-input handling, and no-public-distribution
  boundary are closed; a distributable build remains separately blocked until rights/replacements exist;
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
8. selected save exclusion and 7C private-local asset identity/provenance assertions;
9. 8C pixel/palette/frame cadence, animation/timing, audio waveform/chip/timing,
   VInt/DMA/CRAM/VDP, other reached hardware-observable assertions, deterministic capture conditions,
   exact or field-specific tolerances, and licensing-safe public report shape;
10. separately named expected deviations.

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

Phase 4 should consume public contracts and project-owned fixtures for its tracked implementation and
CI. The selected private-local 7C profile may load ignored original assets and captures locally after
their provenance/inventory is accepted, but those inputs must not become tracked dependencies,
uploads, public-CI requirements, or distributable build contents.

## Readiness Checklist

| Gate | Current result | Closure owner |
| --- | --- | --- |
| exact milestone and engine baseline accepted | PASS | ADR 0008 / ADR 0009 |
| product acceptance profile selected | PASS | ADR 0010 |
| admitted Map 3 start state exact | OPEN | Research, then scenario contract and product decision |
| natural Map 3 route exact | OPEN | Research, then scenario contract |
| required exploration/dialogue/menu/UI scope exact | OPEN | Research plus route-required conditional contracts; ADR 0010 fixes the minimum-scope rule |
| natural Battle 01 admission exact | OPEN | Research, then scenario contract |
| player-turn contract present | PASS | [Battle Functions Control Flow](../contracts/battle-functions-control-flow.md) |
| complete playable Battle 01 trace exact | OPEN | Research plus existing/extended battle contracts |
| after-battle effects exact | OPEN | Research, then scenario contract |
| observable endpoint shape selected | PASS | ADR 0010 option 5B |
| exact endpoint state evidenced | OPEN | Research, then scenario contract |
| save scope selected | PASS | ADR 0010 option 6A excludes save/load/checkpoint/suspend |
| accessibility/input product interface selected | PASS | ADR 0010 option 9A |
| accessibility observable checks composed | OPEN | Continuous H4 contract; deviations separate from the 8C exact reference run |
| 7C private-local asset mode and no-public-distribution boundary selected | PASS | ADR 0010 |
| exact private asset/capture inventory and provenance accepted | OPEN | Research/private-input acceptance; no payload enters Git/public CI |
| public/distributable asset rights or replacements | BLOCKED OUTSIDE PRIVATE MILESTONE | Separate licensing/replacement decision before any public build |
| 8C visual/audio/hardware parity tier selected | PASS | ADR 0010 |
| complete reached 8C evidence, capture domain, and tolerances accepted | OPEN | Research, then continuous H4 contract |
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
| 15 battle-functions records have accepted static evidence and one bounded design contract | **Contract-ready** | [Battle Functions Control Flow](../contracts/battle-functions-control-flow.md), `sf2-battle-functions-static-v1` | No camera-owner overlap and no runtime/input/presentation generalization |
| after-battle route/program identities exist | **Contract-ready route structure** | [Battle Cutscene Routing](../contracts/battle-cutscene-routing.md) | Program effects, natural reachability, persistence, and visible sequence remain open |
| local battle contracts can be composed conceptually | **Synthesis-ready** | [Tactical Battle Loop](tactical-battle-loop.md) and linked contracts | Not a complete predictive Battle 01 simulation or scenario golden |
| Godot/C#, milestone, and product profile are selected | **Accepted decisions** | ADR 0008 / ADR 0009 / ADR 0010 | No Phase 4 start, MCP adoption, public redistribution, or evidence closure |
| route class, endpoint shape, save exclusion, UI, private assets, RNG policy, 8C parity, and deviations | **Accepted product decisions** | ADR 0010 | Exact scenario values, natural chronology, private capture provenance, and parity facts remain Research/H4 gaps |
