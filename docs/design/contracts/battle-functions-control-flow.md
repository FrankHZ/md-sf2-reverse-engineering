# Battle Functions Control-Flow Contract

- **Confirmed original behavior:** the fixture-bounded individual-turn routes, Kiwi conversion gate,
  EGRESS and Angel Wing request order, battle-load and move-command selection order, cursor and
  target-list control, battle and battlefield menu branches, and the listed local action/result
  writes.
- **Inferred original role:** source names and comments suggest grid animation, AI target display,
  player-facing menu purpose, and tactical intent, but those meanings are not executable evidence.
- **Unknown or separate:** natural Map 3 or Battle 01 reachability, input acquisition and timing,
  target-list construction, callee success and persistent effects, complete cancellation nesting,
  battle resolution, rendered or audible output, malformed-state behavior, and platform ABI.

## Purpose

This contract defines an implementation-neutral control boundary for the shared battle functions
whose accepted static evidence is owned by `sf2-battle-functions-static-v1`. It closes the gap
identified by [Map 3 through Battle 01 Readiness](../synthesis/map3-battle01-readiness.md): player and
individual-turn control facts have accepted evidence, but previously had no evidence-bound design
contract.

The contract preserves abstract route decisions, fixture-explicit local results, and ordered source
request traces. It does not turn a source call into proof that the callee succeeded, that a player saw
or heard an effect, or that the branch is naturally reached in Battle 01. It is not a scenario
contract and does not start Phase 4.

## Judgment Boundary

**Confirmed static:** the selected fixture and pinned source establish fifteen named entry or table
identities, the field-closed control facts below, and the H1-resolved symbol addresses under accepted
ROM provenance. Source order confirms branch decisions, local writes, command operands, and call or
transfer requests only to the extent explicitly represented by the fixture.

**Inferred:** `CreatePulsatingBlocksForGrid`, `PerformAiTargetingVisualAct`, menu labels, action names,
and other source vocabulary suggest developer or player-facing roles. They do not prove animation,
visibility, tactical intent, accessibility, or ordinary runtime use.

**Unknown or excluded:** exact input cadence, repeat, simultaneity, and platform mapping; complete
caller state; target-list legality and construction; AI scoring and selection; success, atomicity,
or rollback of external services; persistent inventory, gold, Deals, save, or story mutations;
random distribution; register, CCR, and stack effects; complete H1/ROM instruction parity; VInt,
VDP, DMA, fade, music, text, window, cursor, or sound behavior; malformed lists or state; and natural
Map 3 through Battle 01 reachability.

## Evidence Contract

The sole executable owner consumed by this contract is
`sf2-battle-functions-static-v1` in
[`battle-functions-static-v1.json`](../../../tests/fixtures/h2/battle-functions-static-v1.json),
verified by [`battle_functions.py`](../../../src/sf2tool/h2/battle_functions.py) and explained by
[Shared Battle Functions](../../research/battle-functions.md). The extraction manifest is
[`battle-functions-static.json`](../../../manifests/extractions/battle-functions-static.json).

No battle-control, battlefield, battle-action, input, audio, save, camera, aggregate battle, or H3
fixture is consumed as an executable owner here.

### Field-closed fixture use

| Fixture field | Treatment in this contract |
| --- | --- |
| `romSha256`, `upstreamCommit` | provenance identity only |
| `function` | fifteen bounded symbol/address identities |
| `expected.representativeSymbols` | seven-file provenance witnesses only |
| `expected.functionFacts` | fixture-explicit route, local-result, operand, and request-order facts |
| `expected.playerControlFacts` | fixture-explicit cursor, target, menu, item, and chest branch facts |
| `expected.playerControlInputBits` | abstract consumer-observation names only |
| `expected.playerControlBattleActions` | source constant identities only |
| `expected.playerControlMenus` | source menu constant identities only |
| `expected.playerControlSelectedCallEdges` | bounded static call-edge inventory only |
| `expected.playerControlSummary` | denominator and provenance metadata only; not behavioral fidelity or H4 |
| `expected.indexedRecordIds`, `indexedSourcePaths`, `indexedRecordsBySourcePath` | association/source-membership audit only; not behavioral fidelity or H4 |

The ignored generated report contains deeper per-function catalogs, including complete source ranges
and hashes, branch targets, direct-call maps, global-state access rows, text IDs, and other source
inventory. Those catalogs are private verification material and are not imported into this public
contract.

### Provenance without full-body parity

The fixture resolves each selected symbol identity through H1 under the pinned upstream commit and
accepted ROM SHA provenance. It does not parse and byte-compare the complete instruction body of each
function against H1 and ROM. Complete source bodies, macro-expanded instruction bytes, and any future
body-parity comparison remain private optional verification inputs, not a **Confirmed** fact or H4
requirement.

## Exact Association Denominator

The fixture's source-root membership join contains sixteen records across seven source paths. The
direct `sf2-battle-functions-static-v1` evidence set contains exactly the following fifteen currently
unassociated records:

| Research-index record | Fixture identity | ROM entry/address |
| --- | --- | ---: |
| `battle.functions.pulsating-grid` | `CreatePulsatingBlocksForGrid` | `0x22C84` |
| `battle.functions.control-cursor` | `ControlCursorEntity` | `0x22D90` |
| `battle.functions.choose-target` | `ControlCursorEntity_ChooseTarget` | `0x230E2` |
| `battle.functions.set-cursor-target` | `SetCursorDestinationToNextBattleEntity` | `0x232BC` |
| `battle.functions.angel-wing` | `ExecuteBattleaction_AngelWing` | `0x23D98` |
| `battle.functions.execute-turn` | `ExecuteIndividualTurn` | `0x23EB0` |
| `battle.functions.update-targets` | `UpdateTargetsListForCombatant` | `0x24642` |
| `battle.functions.player-input` | `ProcessBattleEntityControlPlayerInput` | `0x24662` |
| `battle.functions.equip-in-battle` | `EquipNewItemInBattle` | `0x24C94` |
| `battle.functions.check-gold-chest` | `CheckGoldChest` | `0x250FC` |
| `battle.functions.battlefield-menu` | `BattlefieldMenu` | `0x2519E` |
| `battle.functions.ai-target-visual` | `PerformAiTargetingVisualAct` | `0x2548E` |
| `battle.functions.load-battle` | `LoadBattle` | `0x25610` |
| `battle.functions.relative-move-table` | `table_RelativeTileMoveX` | `0x256A2` |
| `battle.functions.move-sfx` | `SetMoveSfx` | `0x25790` |

The sixteenth membership row is `map.camera-control.destination-service`. It shares
`battlefunctions_0.asm`, but is not directly bound to this fixture. It remains associated only with
[Map Exploration](map-exploration.md), with its camera behavior bounded by
[Map Camera Update Control Flow](map-camera-update-control-flow.md). It MUST NOT gain this contract.

The future semantic association set is exactly the fifteen `battle.functions.*` rows in the table.
No other record gains or loses an association.

## Individual-Turn Control

### Actor and controller route

The source-static `ExecuteIndividualTurn` route preserves these decisions:

1. a dead actor skips the turn;
2. nonzero MUDDLE state, the AI-controlled bit, ally auto-battle, or an ordinarily uncontrolled
   enemy selects the AI-control route;
3. the opponent-control toggle is the bounded exception that may route an enemy to player control;
4. SLEEP, STUN, and an already committed STAY result consume the action without constructing a
   battle scene;
5. EGRESS and Angel Wing exit before battle-scene construction; and
6. an ordinary committed action requests `WriteBattlesceneScript`, `ExecuteBattlesceneScript`,
   `EndBattlescene`, and `LoadBattle` in that source order.

This is a routing and request-order contract. It does not own how status fields were produced, AI
behavior, player input timing, scene construction, scene execution, battlefield reload effects, or
the natural caller state of any route.

### Kiwi conversion gate

For the source class identity `28`, only the physical-attack route requests `RNG(4)`. Result zero
selects the Kiwi spell conversion; nonzero leaves the physical route unchanged. The selected spell
level is determined by ordered level thresholds `32`, `40`, and `50`, producing levels `0`, `1`,
`2`, and `3`.

The operand `4`, success result `0`, thresholds, and level identities are **Confirmed static**. RNG
state, distribution, call timing, natural class/action admission, damage, animation, and visible
meaning remain with [Randomness](randomness.md), the resolution contracts, presentation, or
**Unknown**.

### EGRESS and Angel Wing

The fixture labels are interpreted as source-shaped requests rather than completed service effects:

- Angel Wing requests item removal before entering the shared exit path;
- the EGRESS spell path requests spell-cost lookup and MP decrease;
- both paths request battlefield-window closure, unlocked-flag update, and egress-position lookup;
- both locally produce return code `D4 = 0`.

Only the local return result and exact request order are owned here. Item removal, MP mutation,
window closure, flag persistence, returned map state, and caller-visible exit behavior require their
own executable owners or remain **Unknown**.

## Battle Load and Move-Command Selection

`LoadBattle` issues these ten source requests in order:

1. fade out;
2. load map tilesets;
3. position battle entities;
4. initialize sprites;
5. load the map;
6. load entity map sprites;
7. install battle VInt handlers;
8. load battle terrain;
9. request map music; and
10. fade in.

The Fairy Woods branch additionally reaches the source-named timer-opening request. This list
preserves call identities and order only. It does not confirm that graphics, map, entity, terrain,
VInt, music, fade, or timer services completed, what intermediate state was visible, or how long any
step took.

`SetMoveSfx` selects source command identity zero outside battle and the walking identity in battle.
The Chirrup Sandals condition overrides either state with the source identity `BLOAB`. The selection
and override order are static facts; sound transport, command acceptance, waveform, audibility, and
timing remain with [Audio System](audio-system.md) or **Unknown**.

## Player-Control Surface

### Inventory counters are not behavior

The fixture records six selected entries across nine source ranges, with `1,039` statements, `231`
branch sites, `207` direct-call sites to `84` unique targets, `5` selected internal call edges, `59`
global-state identities, all `8` named input bits, `4` battle-action constants, and `4` menu
constants. These are closed inventory and provenance counters. A remake MUST NOT reproduce these
counts, source ranges, branch topology, or call-site totals.

The named input bits are abstract observations consumed by the original branches. This contract does
not own controller acquisition, edge publication, repeat, simultaneity, cadence, electrical behavior,
or modern platform mapping; those remain with [Input System](input-system.md) and later product
decisions.

### Cursor and supplied target list

The bounded cursor route recognizes A, B, or C as tile-confirmation identities, stores the chosen tile
coordinates, and reaches the source cursor-hide request. These are local-output and request facts,
not a guarantee of rendered cursor behavior.

Target selection consumes a supplied list:

- an empty list returns `-1`;
- B is the cancel identity;
- A or C confirms and returns a combatant index; and
- UP, LEFT, DOWN, and RIGHT wrap through candidates.

[Battlefield Navigation](battlefield-navigation.md) retains construction, legality, ordering, side
selection, and spatial meaning of the supplied list. This contract does not make an invalid or
malformed list safe, and it does not define target highlight or movement animation.

### Battle action and battlefield menus

The source-static battle action choice order is attack, magic, item, and search-or-stay. Cancelling
the movement/action route restores the source local position state and writes action result `-1`.
The item-menu order is use, give, equip, and drop. Fixture-listed committed result identities include
attack, cast-spell, use-item, stay, and trapped-chest.

The battlefield-menu choice order is members, minimap, options, and suspend. Battle zero rejects the
suspend route. The accepted suspend seam preserves the source request/local-write order that copies
the seconds value, sets flag `88`, requests save, and transfers toward `WitchSuspend`; the debug Start
route returns to the menu after its save request.

Menu selection and local result identities are owned here. Menu layout, text, input timing, save
success, persistent bytes, Witch execution, suspension restoration, and visible transitions are
separate-owner or **Unknown**.

### Equipment, items, and chests

The fixture closes these branch and request identities:

- cursed equipped state blocks the source exchange route;
- the new-cursed branch reaches its named dialogue request;
- an equipped cursed item blocks give and drop;
- a full destination inventory selects the trade route;
- transfer clears the local equipped bit before the add-item request;
- a completed give route writes STAY locally;
- drop reaches a confirmation request, and a rare-item branch requests addition to Deals;
- chest search distinguishes no-content, empty, trap, gold, item, and full-inventory branches;
- the trap branch writes the trapped-chest action and requests enemy spawn;
- the gold branch reaches the threshold/amount and increase-gold requests;
- the item branch reaches add-item; and
- non-trap resolved routes write STAY, while full inventory requests chest closure and returns to
  the menu route.

These are not proofs of dialogue display, inventory or economy transaction completion, enemy-spawn
success, gold persistence, item delivery, Deals persistence, or atomic rollback. Item definitions,
state mutation, economy, services, and presentation remain separate.

## Identity-Only Records

Five associations are deliberately narrower than the behavioral sections:

- `battle.functions.pulsating-grid`;
- `battle.functions.update-targets`;
- `battle.functions.relative-move-table`;
- `battle.functions.set-cursor-target`; and
- `battle.functions.ai-target-visual`.

They retain only their fixture-supported identity, address, source-inventory membership, and any
explicit bounded call-edge metadata. The relative-move table does not become a data-fidelity owner;
its entries, coordinate meaning, consumers, and runtime use are not reconstructed here. Likewise,
names such as “pulsating” and “visual” do not prove rendered behavior.

## Cross-System Separation

- [Battle Control and Combatant Lifecycle](battle-control-lifecycle.md) retains battle entry, rounds,
  death, post-action, outcome, and caller-admission semantics.
- [Battlefield Navigation](battlefield-navigation.md) retains movement, range, target-list formation,
  and legal spatial state.
- [Battle Action Construction](battle-action-construction.md) retains committed-action to scene-script
  construction. This contract ends at selection and the ordered handoff.
- [Battle AI Decision](battle-ai-decision.md) retains scoring and action choice. This contract retains
  only the static AI-versus-player route and AI-visual entry identity.
- [Combat Resolution](combat-resolution.md), [Spell Resolution](spell-resolution.md), and
  [Randomness](randomness.md) retain arithmetic, effects, replay, and RNG behavior.
- [Input System](input-system.md) retains acquisition and publication of controller state.
- [Save System](save-system.md), service, item, and economy owners retain persistent mutations and
  completion semantics behind menu, equipment, item, chest, and suspend requests.
- [Battle Scene Presentation](battle-scene-presentation.md) and [Audio System](audio-system.md) retain
  visible and audible execution, loaders, fades, timing, and hardware-facing effects.
- [Map Exploration](map-exploration.md) retains the camera cross-owner record and destination service.
- [Tactical Battle Loop](../synthesis/tactical-battle-loop.md),
  [Gameplay Overview](../synthesis/gameplay-overview.md), and
  [Map 3 through Battle 01 Readiness](../synthesis/map3-battle01-readiness.md) remain synthesis
  consumers. They are not executable evidence owners.

## Implementation-Neutral Model

```text
BattleControlInput {
  actorState
  committedOrPendingAction
  suppliedTargetList
  abstractInputObservations
  abstractMenuResults
  boundedServiceResults
}

BattleControlOutput {
  routeIdentity
  localActionOrTargetResult
  orderedLocalWrites[]
  orderedRequests[]
}
```

The original source/H1 identities and addresses are provenance and private round-trip anchors. After
verification, a conforming remake may use engine-native references, state machines, collections,
events, and services. It is not required to reproduce Mega Drive addresses, registers, stack layout,
branch counts, source ranges, or the original in-memory structures.

Compatibility is judged by fixture-admitted abstract inputs and observable route/local-result/request
traces. It is not judged by instruction count, exact call topology, frame count, or platform-specific
implementation.

## Public and Private Boundary

The public contract may retain:

- fixture ID, ROM/upstream provenance hashes, and canonical digest;
- the fifteen selected symbols and H1-resolved addresses;
- bounded seven-file representative identities;
- inventory counters explicitly labeled as nonbehavioral metadata;
- the fixture-listed route decisions, constants, local results, and ordered request summaries; and
- the exact `15 + 1` association/membership boundary.

It MUST NOT publish complete source bodies, full branch/call/global/text catalogs, instruction bytes,
private RAM or ROM dumps, screenshots, audio, dialogue payloads, or derived copyrighted assets.
Private material may support local verification but must not become a tracked runtime dependency.

## Fidelity and Modernization

An original-fidelity adapter preserves:

- the fixture-admitted route distinctions and precedence;
- exact fixture-explicit local action/target results;
- ordered source request traces where order is part of the accepted fact;
- the distinction between selection/control, service execution, and presentation; and
- every separate-owner and **Unknown** boundary in this document.

A modernization may replace source-shaped loops and calls with engine-native state transitions,
collections, futures, or events if the abstract compatibility traces remain equivalent. It may not
silently claim that an external request succeeded, normalize a missing or malformed domain, assign
player intent, or elevate source vocabulary into visible behavior.

## H4 Acceptance Surface

Before a remake adapter can claim this contract, project-owned synthetic checks MUST:

1. cover dead, AI-admitted, opponent-controlled, sleep, stun, stay, special-exit, and ordinary-action
   individual-turn routes;
2. cover the Kiwi class/action/RNG gate and all four level bands without treating the RNG request as a
   distribution guarantee;
3. verify Angel Wing and EGRESS local results plus ordered request traces without asserting callee
   completion;
4. verify the ten LoadBattle request identities and Fairy Woods conditional request without asserting
   fade, VInt, graphics, map, music, or timer completion;
5. verify move-command selection and override identities without asserting audible output;
6. cover empty, cancel, wrap, and confirm cases over supplied target lists;
7. cover battle-action and battlefield-menu choice/cancel result identities and bounded suspend
   request order;
8. use controlled abstract service-result inputs to select each fixture-listed equipment, item, and
   chest branch, while asserting only fixture-explicit local writes/results and ordered requests,
   not service outcomes;
9. retain identity-only records without inventing table contents, visual effects, or deeper behavior;
10. exclude source counters, raw addresses, register/CCR/stack state, complete body parity, natural
    Map 3 or Battle 01 reachability, and all visible/audible/timing claims from the pass condition.

The tests assert abstract route decisions, local outputs, and ordered request traces. They do not
require source-shaped micro-control flow.

## Evidence Matrix

| Claim | Evidence | Judgment |
| --- | --- | --- |
| fifteen entry/table identities and addresses | `function` plus H1 resolution | Confirmed static identity |
| individual-turn, Kiwi, exit, load, and move-command facts | `expected.functionFacts` and pinned source checks | Confirmed static route/local-result/request order |
| cursor, target, menu, item, equipment, and chest facts | `expected.playerControlFacts` and pinned source checks | Confirmed static route/local-result/request order |
| summary counters and source membership | fixture audit fields | Confirmed inventory/provenance only; not H4 behavior |
| five narrow records | address/inventory/call-edge surface only | Confirmed static identity only |
| pulsating/visual/menu/tactical purpose | source names and comments | Inferred |
| input timing, callee completion, persistence, AI/target legality, presentation, natural scenario reachability | not established by selected owner | Unknown or separate owner |

## Reproduction

```powershell
uv run sf2 h2 battle-functions
uv run sf2 design-contracts test
uv run sf2 research-index test
```

Generated output remains under ignored `local/derived/battle-functions-static.json`.
