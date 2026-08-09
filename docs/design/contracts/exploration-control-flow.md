# Exploration Control Flow Contract

- Status: **Confirmed static main-loop and exploration dispatch control**
- Evidence date: 2026-08-09
- Scope: implementation-neutral reconstruction of the original top-level exploration handoffs,
  map-event and player-action precedence, interaction admission, item-refill boundary, and bounded
  exploration operation inventory, without importing startup, map-data, input timing, battle outcome,
  downstream service behavior, presentation, or story meaning

## Judgment Boundary

This contract begins at the source-shaped `MainLoop` and the accepted exploration control helpers. It
ends at helper-local dispatch choices, candidate identities, bounded state writes, or downstream
handoffs. It does not define the systems reached by those handoffs.

- **Confirmed**: `MainLoop` requests map switching before battle-candidate selection, treats battle
  index `-1` as the no-battle sentinel, and sends a returning `BattleLoop` invocation through another
  map-switch request before exploration; the exploration wait loop polls map event before action
  input, and the outer loop dispatches map event before player action; a pending map event is cleared
  before one of six named handler handoffs is selected; an unknown event type issues the
  `SFX_BATTLEFIELD_DEATH` command identity; player-action testing gives A priority over C and retains
  the field-menu fallback; activated-entity selection has 48 candidate slots, skips player and
  follower identities, and uses the accepted 384 internal fixed-point distance limit; the five area
  block-kind codes, full-inventory map-item refill fact, Pacalon branch flag identity `530`, and
  door/roof/chest/plane update inventory are accepted static facts.
- **Inferred**: none. Player intent, story meaning, and visible response are not inferred from static
  branch order or operation identities.
- **Unknown**: exact VInt-edge publication versus input-sampling timing; natural caller and story
  reachability; the validity and lifecycle of all 48 candidate slots; conversion of the 384 internal
  distance value to screen pixels or modern world units; private map and area content; event-handler,
  menu, dialogue, audio, item, party, and battle outcomes; state persistence; scroll, door, roof,
  chest, warp, and vehicle frames; rendered presentation; malformed or injected state; and balance,
  accessibility, or campaign intent.

The [map-entry routing contract](map-entry-routing-state.md) owns the accepted `SwitchMap` and
`CheckBattle` helper-local rules. This contract records only their top-level call order and result
handoffs. The [map and exploration contract](map-exploration.md) retains map import, construction,
working-layout mutation, map-script, entity, camera, and accepted runtime rails. Neither sibling
contract is silently absorbed here.

## Evidence Owner and Source Audit

`sf2-gameflow-core-static-v1`
([`gameflow-core-static-v1.json`](../../../tests/fixtures/h2/gameflow-core-static-v1.json)) is the sole
executable owner consumed by this contract. Its verifier is
[`gameflow.py`](../../../src/sf2tool/h2/gameflow.py), and its source-backed explanation is
[Startup, Main Loop, and Exploration Core](../../research/gameflow-core.md). This contract consumes
only `expected.explorationFacts` and the following six record identities:

- `gameflow.main-loop`;
- `gameflow.exploration.engine`;
- `gameflow.exploration.interaction`;
- `gameflow.exploration.item-handoff`;
- `gameflow.exploration.loop`;
- `gameflow.exploration.actions`.

A read-only audit of the pinned source confirms the same bounded order and identity facts. The audit
does not promote source comments into runtime or story semantics and does not expand the fixture-owned
claim set.

The fixture's complete source-membership set contains 14 records, while 13 records carry direct
`sf2-gameflow-core-static-v1` evidence. This contract deliberately selects only the six exploration
and main-loop records above. Five unassociated startup records remain outside this contract for a
separate startup boundary:

- `gameflow.start.game-init`;
- `gameflow.start.intro`;
- `gameflow.start.cold-start`;
- `gameflow.start.region`;
- `gameflow.start.system-init`.

Two directly fixture-linked records retain their existing contracts without gaining this one:
`gameflow.start.base-tiles` remains with
[UI graphics asset data](ui-graphics-asset-data.md), and `gameflow.start.z80-init` remains with the
[audio system](audio-system.md). The membership-only `map.block-mutation.copy-helper` retains its H3
owner and [map-exploration](map-exploration.md) association. These distinctions prevent one aggregate
fixture from becoming an excuse to absorb unrelated startup, asset, audio, or map-mutation surfaces.

## Main-Loop Handoff Order

**Confirmed static:** `MainLoop` preserves these helper and subsystem handoffs:

1. request map-switch selection;
2. request battle-candidate selection;
3. if the battle index is `-1`, enter exploration;
4. otherwise invoke `BattleLoop` with the candidate identity;
5. after that `BattleLoop` invocation returns, request map-switch selection again before entering
   exploration.

The first and fifth steps preserve `SwitchMap` operation identity, not its private table scan. The
second step preserves `CheckBattle` operation identity and the accepted no-battle sentinel, not its
coordinate/flag admission rules. Those details remain with `map-entry-routing-state`.

“Returning `BattleLoop` invocation” is intentionally narrower than “completed battle.” This fixture
proves source call/return ordering, not why the battle returned, which outcome was selected, whether a
save mutation occurred, or what the player sees next.

| Main-loop edge | Accepted contract | Deliberate boundary |
| --- | --- | --- |
| initial route | map-switch request precedes battle-candidate request | helper-local rules and private tables are separate |
| no-battle route | battle index `-1` enters exploration | invalid raw indexes and caller-visible diagnostics are **Unknown** |
| battle route | non-`-1` candidate is handed to `BattleLoop` | admission, rounds, outcome, rewards, and presentation are separate |
| returning battle route | map-switch request precedes exploration | no claim that return means victory, defeat, or persistence |

## Event and Action Precedence

**Confirmed static:** the exploration wait helper polls two state surfaces in this order:

1. `mapEvent`;
2. `actionInput`.

After the helper returns, the outer exploration loop tests and dispatches in this order:

1. `mapEvent`;
2. `playerAction`.

Therefore, when both values are already visible to one source-static polling/dispatch iteration, the
map event wins. This is a branch-precedence contract, not an observation of exact simultaneity. The
fixture leaves the VInt edge at which an entity script publishes an event versus the controller
sample explicitly **Unknown**.

The [input-system contract](input-system.md) retains sampling, current/repeat state, and wait-helper
behavior. This contract does not redefine controller ports, press/release timing, repeat policy, or
the provenance of the action-input value.

## Map-Event Dispatch Boundary

**Confirmed static:** `ProcessMapEvent` clears the pending map-event state before selecting a handler.
The accepted dispatch identities are ordered and closed to these six named handoffs:

1. `Warp`;
2. `GetIntoCaravan`;
3. `GetIntoRaft`;
4. `GetOutOfCaravan`;
5. `GetOutOfRaft`;
6. `ZoneEvent`.

An event type outside those six issues the `SFX_BATTLEFIELD_DEATH` command identity and returns. The
[audio-system contract](audio-system.md) owns accepted sound-command domains and playback-state
boundaries. No audible result, volume, timing, fallback intent, or presentation meaning is claimed
here.

The warp route contains a hard-coded Pacalon branch flag identity `530`. This contract preserves the
raw flag identity and the existence of that branch only. It does not assign campaign meaning to flag
530, prove natural reachability, define persistence, or import the downstream map-transition
chronology.

Handler names are dispatch targets, not complete behavior contracts. Caravan/raft state changes,
zone-event script effects, warp position changes, fades, and return routes remain with their dedicated
map, script, service, and presentation owners.

## Player-Action and Entity-Interaction Boundary

**Confirmed static:** the accepted player-action priority is A before C. The source-shaped fallback
opens the field-menu handoff when no earlier accepted C-route consumes the action, while A reaches the
field-menu handoff directly.

This priority does not import the complete debug, Caravan, entity-event, area-inspection, or menu
chronology. It also does not prove which button state is visible at a VInt edge. The
[service-interactions contract](service-interactions.md), menu contracts, and future runtime evidence
retain their downstream behavior.

`GetActivatedEntity` exposes this bounded admission surface:

- 48 candidate slots are considered;
- the player identity is skipped;
- follower identities are skipped;
- the accepted distance limit is 384 internal fixed-point units.

The 48-slot count is an iteration surface, not a roster size or proof that every slot is populated,
valid, visible, or reachable. The number 384 is retained in original internal units. This contract
does not relabel it as pixels, a modern-engine tile size, world meters, or a general interaction
radius for other systems.

Entity facing, desk/counter extension, exact candidate scan state, event-index lookup, dialogue
selection, and final caller-visible result remain outside the fixture-owned fact set unless a sibling
owner closes them.

## Area and Item Handoff Boundary

**Confirmed static:** area inspection classifies these accepted block-kind codes:

| Kind identity | Accepted code | Not established here |
| --- | ---: | --- |
| chest | 6144 | private chest contents, animation, dialogue, persistence |
| generic | 7168 | semantic area taxonomy or localization |
| vase | 11264 | visible art, contained item, or reset behavior |
| barrel | 12288 | visible art, contained item, or reset behavior |
| bookshelf | 13312 | dialogue/text payload or story meaning |

The codes are source-static classification values, not proof that every original map contains each
kind or that the values should become a modern renderer's material IDs.

The accepted full-inventory fact is narrow: when the item handoff cannot place the found item because
the relevant inventories are full, the map item is refilled. This does not define inventory capacity,
recipient priority, item identity, transaction rollback, save persistence, dialogue, sound, or visible
chest state. Item definitions, mutable party state, and service/menu behavior remain with their
separate contracts.

## Bounded Exploration Operation Inventory

**Confirmed static inventory:** the owner classifies door, roof, chest, and plane updates within the
exploration source boundary. This contract retains those four operation-family identities so a future
compatibility adapter does not silently drop the handoffs.

The inventory is not a complete mutation sequence. It does not establish exact call order, addresses,
frame cadence, map-content reachability, VInt/DMA timing, rendered tiles, collision effects, sound, or
visibility. Those questions stay with [map exploration](map-exploration.md), camera, map-animation,
graphics, and presentation owners.

## Cross-System Separation

Exploration control connects systems without owning all of them:

- map-entry routing owns map-switch and battle-candidate helper rules;
- battle contracts own `BattleLoop`, rounds, outcomes, rewards, and battle presentation;
- map exploration owns import, layout construction/mutation, scripts, entities, camera, and accepted
  runtime rails;
- input-system evidence owns raw sampling and repeat/wait behavior;
- global flags retain their addressing and persistence questions;
- item, party, Caravan, menu, and dialogue owners retain transaction and player-facing semantics;
- audio owns sound-command and playback boundaries;
- private map, area, event, item, and text payloads remain outside public contracts;
- VInt-edge timing, scroll/door/roof/chest/warp/vehicle frames, UI, audio timing, localization,
  accessibility, and balance remain separate or **Unknown**.

The [story-progression synthesis](../synthesis/story-progression.md) may place accepted handoffs in a
larger explanation, but it must not turn this static control graph into proof of natural reachability
or exact visible chronology.

## Implementation-Neutral Control Model

```text
MainLoopControl
  initialOrder:
    - requestMapSwitch
    - requestBattleCandidate
  noBattleSentinel: -1
  noBattleRoute: enterExploration
  battleRoute:
    - invokeBattleLoop(candidateIndex)
    - requestMapSwitch
    - enterExploration

ExplorationIteration
  waitPollOrder:
    - mapEvent
    - actionInput
  outerDispatchOrder:
    - mapEvent
    - playerAction
  precedenceScope: valuesAlreadyVisibleWithinOneIteration

MapEventDispatcher
  clearPendingBeforeDispatch: true
  orderedTargets:
    - Warp
    - GetIntoCaravan
    - GetIntoRaft
    - GetOutOfCaravan
    - GetOutOfRaft
    - ZoneEvent
  unknownTypeCommandIdentity: SFX_BATTLEFIELD_DEATH
  pacalonBranchFlagIdentity: 530

PlayerActionRouter
  buttonPriority:
    - A
    - C
  fallbackHandoff: FieldMenu

ActivatedEntityBoundary
  candidateSlotCount: 48
  excludedIdentities:
    - player
    - followers
  distanceLimit:
    value: 384
    unit: originalInternalFixedPoint

AreaKindCodes
  chest: 6144
  generic: 7168
  vase: 11264
  barrel: 12288
  bookshelf: 13312

FullInventoryBoundary
  mapItemRefilled: true

ExplorationOperationInventory
  - doorUpdate
  - roofUpdate
  - chestUpdate
  - planeUpdate
```

This is a logical parity model, not a required engine loop, memory layout, or threading model. The
model deliberately stores handoff identities and precedence separately from downstream effects.
`invokeBattleLoop` is not a battle outcome, `FieldMenu` is not a UI implementation, and a map-event
target name is not a complete handler contract.

## Original Fidelity and Modernization

Original-fidelity mode preserves the accepted main-loop order, both event-before-action precedence
layers, clear-before-dispatch rule, six target identities, action-button priority, interaction
admission metadata, area-kind codes, refill fact, flag identity, and bounded update inventory. It
keeps timing and downstream outcomes visible as separate tests or explicit Unknowns.

A modern engine may use an event queue, typed commands, immutable state snapshots, validated entity
handles, explicit transactions, asynchronous scene loading, or input actions rather than raw buttons.
Those are deliberate design choices. A compatibility adapter must still reproduce the accepted
source-facing precedence and handoff facts, and intentional divergences must be recorded separately.

Public parity fixtures need only structural metadata, identities, codes, and synthetic state. Original
map, event, item, dialogue, graphics, and audio payloads remain private/generated inputs.

## H4 Acceptance Gates

A future remake exploration-control adapter passes this contract only when:

1. its compatibility route requests map switching before battle-candidate selection and preserves
   battle index `-1` as the no-battle sentinel;
2. a returning `BattleLoop` invocation reaches a map-switch request before exploration without
   treating the return as proof of victory, defeat, or any other outcome;
3. both wait polling and outer dispatch preserve map-event-before-action precedence for values already
   visible within one iteration, while exact VInt-edge simultaneity remains separately tested;
4. a pending map event is cleared before dispatch, the six target identities remain ordered and
   distinct, and unknown event types preserve the fallback command identity without inventing audible
   behavior;
5. player-action compatibility preserves A-before-C priority and the field-menu fallback identity
   without redefining controller sampling or UI behavior;
6. activated-entity compatibility preserves 48 candidate slots, player/follower exclusions, and the
   384 original internal fixed-point distance limit without converting those facts into roster size,
   pixels, or a universal interaction radius;
7. the five area-kind codes, full-inventory map-item refill fact, Pacalon branch flag identity `530`,
   and door/roof/chest/plane inventory remain reproducible without importing private content or
   unaccepted transaction/frame semantics;
8. startup, map-entry helper rules, map import/mutation, battle, input, item, party, menu, dialogue,
   audio, persistence, presentation, malformed-state behavior, story meaning, and balance remain
   separately tested or explicitly **Unknown**;
9. public artifacts contain structural metadata and synthetic state rather than copyrighted payloads.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| MainLoop map-switch/battle/exploration call and return order | **Confirmed static** | `sf2-gameflow-core-static-v1` ([`gameflow-core-static-v1.json`](../../../tests/fixtures/h2/gameflow-core-static-v1.json)) | Helper rules, battle outcome, map load, visible transition |
| map-event-before-action polling and dispatch | **Confirmed static precedence** | `sf2-gameflow-core-static-v1` ([`gameflow-core-static-v1.json`](../../../tests/fixtures/h2/gameflow-core-static-v1.json)) | Exact VInt-edge publication/sampling timing |
| clear-before-dispatch, six targets, unknown-type command, flag 530 | **Confirmed static** | `sf2-gameflow-core-static-v1` ([`gameflow-core-static-v1.json`](../../../tests/fixtures/h2/gameflow-core-static-v1.json)) | Handler effects, audible result, story and persistence meaning |
| A-before-C priority and field-menu fallback identity | **Confirmed static** | `sf2-gameflow-core-static-v1` ([`gameflow-core-static-v1.json`](../../../tests/fixtures/h2/gameflow-core-static-v1.json)) | Input sampling, debug/service/menu chronology, UI behavior |
| 48 candidates, exclusions, 384 internal-unit limit | **Confirmed static** | `sf2-gameflow-core-static-v1` ([`gameflow-core-static-v1.json`](../../../tests/fixtures/h2/gameflow-core-static-v1.json)) | Slot validity, roster meaning, coordinate conversion, caller-visible result |
| area codes, full-inventory refill, bounded update inventory | **Confirmed static** | `sf2-gameflow-core-static-v1` ([`gameflow-core-static-v1.json`](../../../tests/fixtures/h2/gameflow-core-static-v1.json)) | Private content, transactions, persistence, frames and presentation |
| startup five, base-tile/Z80 existing owners, block-copy membership-only record | **Excluded sibling records** | Separate existing or future contracts; no additional fixture consumed | Do not expand the six-record association boundary |
| runtime timing and downstream map/battle/input/service/presentation meaning | **Separate owner / Unknown** | Adjacent contracts and future runtime/synthesis work | Do not infer a complete exploration experience from static control |

## Reproduction

```powershell
uv run sf2 h2 gameflow-core
uv run sf2 design-contracts test
uv run sf2 verify
```
