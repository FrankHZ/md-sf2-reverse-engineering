# ADR 0019: State- and Content-Driven Remake Engine

- Status: **Proposed**; design for independent review, not implementation authorization
- Proposal date: 2026-09-13
- Scope: runtime authority, content admission, program execution, verification, and incremental migration
- Accepted evidence base: `41be8d415322769d4f81cef77998fe35707a3e5e`
- Problem owner: [architecture and verification audit](../../remake/docs/architecture-audit.md)

## Recommendation and decision boundary

Keep the four-assembly deterministic modular monolith and one `GameSession` facade. Make gameplay
admission a function of the live state, validated content, and explicitly implemented rule capability.
Move fixed walkthrough histories into reference runners. Use typed resumable programs for story
effects and let Application advance exploration, programs, battles, and return flow until actual
player input, a declared presentation/tick boundary, or an attributed failure is required.

This is a behavioral decoupling proposal. Merely moving existing guards into smaller files would
leave the audited problem intact. Conversely, removing every guard would discard valid rules,
atomicity, and unsupported boundaries. The migration below separates these cases before changing them.

All interfaces, normalized records, authored examples, and candidate filenames below are **Proposed
design**, not findings about the original or claims of implemented support. **Confirmed** and
**Unknown** in the evidence table retain the repository evidence meanings. Audit A1–A8 remain open.
No engine, test, fixture, schema, research owner, or capability ledger changes in this design slice.
Implementation slices require accepted scope and independent main-gate review.

The user's testing requirement is part of this design: new automated tests cover meaningful new-engine
behavior through small unit tests. Reference replay, probes, comparisons, fixture drivers, gates,
planners, reports, and test helpers are already verification; do not add tests of those verification
programs or a second validation framework. Audit and retire or migrate old tests and CI with the
engine. Keeping every old test unchanged and green is not a migration objective. Original research
evidence and completed results remain durable without requiring those old tests to run forever.

## Evidence used and its limits

The route was synthesis first: [tactical battle loop](../design/synthesis/tactical-battle-loop.md),
[story progression](../design/synthesis/story-progression.md),
[map design principles](../design/synthesis/map-design-principles.md), and
[Map 3 readiness](../design/synthesis/map3-battle01-readiness.md), followed by the direct owners below.
The newer admission owner refines the older readiness summary: an explicit-bridge player-ready H3
exists, while the omitted natural story segment remains Unknown.

Original evidence uses the USA baseline and pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`; each linked fixture retains its own ROM identity,
source/function/address, observer boundary, and reproduction owner. The proposal consumes those
tracked records; it does not treat remake output as evidence of original behavior or run new H3 work.

| Subsystem | Contract → research → representative executable fixture | Confirmed reuse and remaining boundary | Design consequence |
| --- | --- | --- | --- |
| Exploration and map entry | [exploration flow](../design/contracts/exploration-control-flow.md), [entry routing](../design/contracts/map-entry-routing-state.md) → [gameflow](../research/gameflow-core.md), [common maps](../research/common-maps.md) → [gameflow H2](../../tests/fixtures/h2/gameflow-core-static-v1.json), [maps H2](../../tests/fixtures/h2/common-maps-static-v1.json) | Main-loop routing, pending-event handling, ordered map/battle selectors are Confirmed static. Natural reach and visible timing are separate. | Application owns ordered routing; map number and route-input count do not choose a scripted endpoint. |
| Map definitions and mutations | [setup data](../design/contracts/map-setup-data.md), [exploration](../design/contracts/map-exploration.md) → [map inventory](../research/map-data-inventory.md), [common scripting](../research/common-scripting.md) → [setup H2](../../tests/fixtures/h2/map-setup-static-v1.json) | Setup uses the last set-flag alternative; events use first match. New-map rebuild and current-map preserving reload differ. Geometry alone does not prove passability or display. | Keep ordered selectors and working layout separate from immutable resources; reuse existing map reducers. |
| Programs, dialogue, and entity actions | [program data](../design/contracts/standalone-map-script-program-data.md), [dialogue](../design/contracts/dialogue-system.md) → [common scripting](../research/common-scripting.md) → [interpreter H2](../../tests/fixtures/h2/map-script-engine-static-v1.json), [dialogue H3](../../tests/fixtures/h3/map-script-dialogue-v1.json), [bridge H3](../../tests/fixtures/h3/map-entity-action-bridge-v1.json) | Full interpreter graph is 304 programs; standalone setup subset is 178. Cursor transfers and bounded handler/wait seams are Confirmed; bridge wait release is harness-controlled. Natural motion, dialogue chronology, and timing remain bounded/Unknown. | Parse typed operations and preserve PC, continuation, and wait state. Neither opcode counts nor injected release prove a complete story interpreter. |
| Battle admission | [entry routing](../design/contracts/map-entry-routing-state.md), [battle lifecycle](../design/contracts/battle-control-lifecycle.md) → [current admission owner](../research/map3-battle01-admission.md) → [player-ready H3](../../tests/fixtures/h3/map3-battle01-player-ready-v1.json) | The declared bridge seeds the R2b terminal; original code then executes 46 inputs through admission and actor-1 control. R2a → R2b natural continuity and later victory are Unknown. | Preserve bridge provenance in reference setup; interactive runtime cannot require that input count or claim a wholly natural start. |
| Actors, enemies, items, spells, terrain | [allies](../design/contracts/ally-definition-data.md), [enemies](../design/contracts/enemy-definition-data.md), [items](../design/contracts/item-definition-data.md), [spells](../design/contracts/spell-definition-data.md), [encounters](../design/contracts/battle-encounter-definition.md) → [ally inventory](../research/ally-data-inventory.md), [core inventory](../research/core-stats-data-inventory.md) → [ally H2](../../tests/fixtures/h2/ally-data-static-v1.json), [core H2](../../tests/fixtures/h2/core-stats-data-static-v1.json), [terrain H2](../../tests/fixtures/h2/battle-terrain-decode-v1.json) | Stored records, joins, aliases, and raw fields are Confirmed static; definitions are not complete consumer semantics. Spell radius 3 is valid stored data with Unknown geometry; enemy sprite tail is not extra enemy definitions. | Normalize identity and references without losing source projection. Distinguish structurally valid content from implemented effects. |
| Movement and AI | [navigation](../design/contracts/battlefield-navigation.md), [AI](../design/contracts/battle-ai-decision.md) → [pathfinding](../research/battlefield-pathfinding.md), [AI research](../research/battle-ai.md) → [movement H3](../../tests/fixtures/h3/battlefield-movement-matrix-v1.json), [AI choice H3](../../tests/fixtures/h3/battle-ai-action-choice-v1.json) | Weighted first-admission propagation, stable order, AI commandsets stopping at first success, separate thinking RNG, and bounded decision matrices are Confirmed. Wider caller states and natural multi-turn tactics remain Unknown. | Reuse algorithms within declared domains; AI scores cannot substitute for real damage, and unsupported AI cannot silently become STAY. |
| Action, arithmetic, replay, rewards | [construction](../design/contracts/battle-action-construction.md), [combat](../design/contracts/combat-resolution.md), [spells](../design/contracts/spell-resolution.md), [RNG](../design/contracts/randomness.md) → [actions](../research/battle-actions.md), [runtime math](../research/runtime-rng-and-battle-math.md) → [actions H2](../../tests/fixtures/h2/battle-actions-static-v1.json), [HEAL H3](../../tests/fixtures/h3/spell-healing-v1.json), [healing boundaries H3](../../tests/fixtures/h3/spell-healing-exp-boundaries-v1.json) | Temporary construction and persistent application differ. HEAL power/cap, eligible-class EXP, MP-first replay, source arithmetic, and RNG order have named bounded evidence. Adjacent unobserved families are not implicitly supported. | Reuse pure calculations and ordered effects; validate the full action's supported branch closure before persistent commit. |
| Scheduling, death, outcome, return | [individual turn](../design/contracts/battle-functions-control-flow.md), [lifecycle](../design/contracts/battle-control-lifecycle.md), [cutscene routing](../design/contracts/battle-cutscene-routing.md) → [battle loop](../research/battle-loop.md), [runtime math](../research/runtime-rng-and-battle-math.md) → [control H2](../../tests/fixtures/h2/battle-control-static-v1.json), [turn H3](../../tests/fixtures/h3/battle01-turn-order-v1.json), [after-turn H3](../../tests/fixtures/h3/after-turn-status-lifecycle-v1.json) | Ordered activation/spawn/scheduling, dead worklists, checks before and after after-turn, and static outcome mutations are Confirmed. Natural complete battle/after-program/return and presentation are not closed. | One Application loop owns continuation; victory requires after-program and return completion before reporting stable exploration. |

For reproduction, the owners retain commands including `uv run sf2 h2 gameflow-core`,
`uv run sf2 h2 common-maps`, `uv run sf2 h2 map-script-engine`,
`uv run sf2 h2 battle-loop`, `uv run sf2 h2 battle-control`,
`uv run sf2 h2 ally-data`, `uv run sf2 h3 spell-healing`,
`uv run sf2 h3 spell-healing-exp`, and `uv run sf2 h3 map3-battle01-player-ready`.
Listing these is not a new execution result. Later original-runtime work still needs ADR 0016's
specific material question and admission; an Unknown is not an automatic research queue.

## Legal behavior and internal responsibilities

A command is admissible exactly when its session/revision is current, its actor and control phase
permit it, its referenced content and targets are valid, its rule prerequisites hold, and the
reachable action branch lies inside implemented capability. A reference case ID, receipt count,
particular round, five-unit position tuple, or named character's kill ordinal is not a rule predicate.

Keep true source conditions even when identity-specific: healer-class EXP eligibility, enemy-leader
outcome routing, the Taros/Achilles Sword condition, and Battle 01's reward table are examples.
Represent each as a named, typed rule/table operand in the relevant rule profile, with an original
identity mapping in the private import. Do not replace them with a vague generic modifier list or
silently discard them as walkthrough coupling. An unimplemented special rule stops explicitly.

| Audit | Root cause | Proposed owner and change | Observable acceptance |
| --- | --- | --- | --- |
| A1 | State legality and one replay's prefix are combined. | Domain battle command admission and turn reducer use live actor/phase/range/resources/status; reference runner owns expected rounds and receipts. | HEAL succeeds in two valid different histories; dead-slot skipping works in any generated round. Wrong actor, stale selection, and unsupported branches still fail. |
| A2 | Import trust, fixed fixture identity, and runtime definitions share one gate. | Content readers separate source verification from typed package validation; Application admits capabilities; reference loader alone chooses a fixed golden package. | Two authored rosters/maps using the same capabilities load without adding IDs/digests to production. Tampered private provenance still fails. |
| A3 | A story program is represented by a terminal state assignment. | Internal Application program runner plus Domain operation reducers retain cursor, motion, dialogue, and continuation. | Changing a branch flag changes executed operations; a dialogue/motion wait prevents later mutation; an unknown operation stops at its own PC. |
| A4 | Godot dispatches individual turns and rounds. | Internal Application battle coordinator drives activation, actor routing, action completion, and next control. | The same command transcript without Godot reaches the same next actor; Godot input code contains no round/AI/death scheduler. |
| A5 | One facade name hides incompatible state/command APIs. | One common command result and snapshot; focused internal coordinators receive the single state and return transitions. | Public authored and private original sessions accept the same semantic command family; reading a snapshot never requires selecting a profile-specific accessor. |
| A6 | Scenario receipts become constructor-wide mutable lifecycle fields. | Typed world/entity/program/battle state owns live facts; observations retain history separately. | Movement preserves inventory, flags, pending waits, and unrelated entity state without copying Sarah/castle/palace fields through every constructor. |
| A7 | New case IDs spread through unrelated action/round/death handlers. | Spell resolver, shared ordered effect application, and generic turn completion replace case-specific integration branches. | A second supported healer or different lethal actor/order requires content and tests only; no new case comparisons in round, attack, or dead-slot code. |
| A8 | Runtime errors use startup exceptions and lose their reason. | Common failure categories at existing boundaries, internal typed reasons, one Application mapping. | Bad MP, stale revision, unsupported effect, bad reference, invariant breach, and presenter error remain distinguishable through session/Godot results. |

The four assemblies keep their existing dependency direction. Domain owns deterministic rules and
typed transitions. Application owns the sole session state, control flow, content port, and observation
projection. Content owns untrusted decoding and immutable definition construction. Godot owns
composition, semantic input, presentation, and platform errors. Internal coordinators are ordinary
focused types and direct calls, not public services or alternative session facades.

## One command, state, and result model

The following is abbreviated interface notation, not a promised source-compatible API:

```text
GameSession.Start(AdmittedPackage, StartDefinition, RunPolicy) -> StartResult
GameSession.Submit(CommandEnvelope) -> SessionResult
GameSession.Snapshot -> SessionSnapshot

CommandEnvelope = { sessionId, expectedRevision, command }
Command = Move(Direction) | Interact(EntityRef) | Search | ChooseAction(ActionKind)
        | SelectSpell(SpellRef, Level) | SelectTarget(ActorRef) | Confirm | Cancel
        | ChooseDialogue(WaitToken, Choice) | Acknowledge(WaitToken)
        | AdvanceSimulation(WaitToken, TickCount)

SessionResult = {
  snapshot, orderedObservations, stopReason, failure?
}
StopReason = PlayerInput | PresentationWait | SimulationWait
           | Unsupported | Rejected | Faulted | Completed
```

`RunPolicy` selects the gameplay rule profile and observation/presentation policy before start;
content trust is established by its reader, not a caller-supplied boolean. A reference runner is an
external consumer of this same API and its observations. There is no `NextRound`, `EnemyAttack`,
`CompletePalace`, or `AcceptReceipt107` user command. `AdvanceSimulation` belongs to the host's
deterministic tick driver and replay runner, not an extra player button. Every accepted automatic
continuation uses the same Application dispatcher.

The state has one canonical value at each commit:

| State owner inside the session | Required typed contents and authority |
| --- | --- |
| Campaign | Ally actor records, party membership/order, inventory/equipment/spellbooks, flags, economy, and persistent counters. Actor IDs index typed records, never arbitrary `object` values. |
| Active world | Current map/resource selection, working layout, field entities and their movement/action state; selectors use live campaign flags. In battle, saved field return context is an anchor, not a second live player position. |
| Active encounter, when present | Encounter definition ref, enemy actor records, ally refs into Campaign, battlefield placements, terrain/occupancy, region/spawn state, ordered turn entries/cursor, AI memory, dead worklist, and outcome stage. Ally HP/MP/EXP are not copied into a competing roster authority. |
| Execution | A typed control phase, program frames/continuations when running, one blocking wait, and the current typed player selection when applicable. Constructors restrict which combinations can coexist. |
| Determinism and publication | Main and thinking RNG state, explicit simulation tick, session ID, revision, observation sequence, and pending acknowledgement identity. Presentation does not own these values. |

An immutable discriminated phase distinguishes exploration input, program execution, map transfer,
battle initialization, turn preparation, player selection, action resolution, after-turn, outcome,
and return. A program frame can carry a typed return continuation such as
`AfterBeforeBattleProgram` or `AfterVictoryProgram`; it does not own another Battle state.
Nested operations receive state explicitly and return a typed transition. Only `GameSession`
publishes it. The common snapshot is a read-only projection with a discriminated active-mode payload,
available actions, pending wait/selection, actor/world facts, and failure context; no null-forgiving
profile split and no scenario-named optional field collection.

### Provisional selection, atomicity, and stale input

Movement/target selection stores the turn origin, candidate destination/path, selected action/target,
and a state revision against which it was derived. It is the authoritative *selection*, not a second
committed actor position. Presenters may show its preview. Cancel restores the earlier selection
stage or turn origin and consumes no gameplay RNG, MP, HP, inventory, reward, or turn. Confirm
rechecks phase, actor, content, occupancy, target side/range, resources, and capability against the
current state. Cancel after action commit is not undo.

A restart creates a new session ID. Every mutation, including selection and wait consumption, advances
the revision. A command carrying an old ID/revision or a previously consumed wait token is rejected
without mutation. This is concurrency protection, not validation of an entire historical receipt chain.
A valid start comes from the admitted loader; arbitrary snapshot injection is not a production API.

Resolve a committed action against a temporary transaction value containing tentative actor changes,
ordered effects, main/thinking RNG changes, and rewards. Do not debit persistent MP or move the
committed actor while still discovering an unsupported counter, item drop, level-up, or after-effect
inside that action. Once the whole action is supported and valid, publish one atomic action commit
and its ordered effect observations. Reuse snapshot/restore evidence as an observation contract,
without reproducing the original command-buffer storage.

A valid but ineffective action is not a rejected command. For example, the confirmed SILENCE gate
can charge MP before suppressing a marked spell effect; failed recasts can mutate status counters.
Such behavior must follow its admitted rule contract. Only command rejection or an unsupported
uncommitted action guarantees unchanged gameplay state and RNG.

Automatic flow can contain multiple commit boundaries. A later unsupported after-turn/program
operation retains earlier successfully committed actions and reports the exact stopped phase and
last committed revision. It cannot report that the original command had no effect. Non-gameplay
failure/observation metadata may advance on that stop; actor resources and RNG for the failed
transaction remain unchanged. Invariant failure freezes the last coherent state with the original
cause; it never silently retries with a simplified rule.

### Ordered observations and deterministic advancement

Snapshots answer what is true now; observations describe ordered changes and presentation requests.
Observations are not a replayable second state authority. Publish stable sequence/revision, actor or
resource refs, effect payloads, and an optional blocking token at meaningful boundaries: movement
commit, dialogue request, MP/HP/status/reward effect, death cleanup, outcome, and map readiness.

A presentation acknowledgement means the requested presentation boundary completed, not permission
to set arbitrary gameplay values. Only the matching wait may resume its continuation. A headless
semantic runner supplies explicit acknowledgements/ticks under a named policy. It cannot satisfy an
8C claim by auto-acknowledging. Godot disposal, redraw, and render callbacks do not replay mutations.

Application runs automatically to the next real stop. Internal work quanta may yield to the host
without exposing a new gameplay command or dropping state. A valid timed wait advances only through
explicit ticks; a non-progressing immediate program cycle is an attributed content failure, not a
successful end. There is no one-round dispatch cap or additional player click just to advance AI.
The RNG implementation keeps the main generator and thinking stream distinct, preserves recorded
integer widths/wrapping/draw order, and uses no platform time or Godot random generator.

## Typed content and capability admission

Use the existing offline extraction owners to verify original bytes, pinned identity, structure, and
references and to produce ignored private packages. Runtime performs no ROM reads, disassembly,
Python extraction, or address-based service dispatch. Preserve original numeric IDs, raw fields,
aliases, and source projection in provenance/diagnostics; runtime references resolve to typed IDs.

Three boundaries stay distinct:

1. **Source and distribution trust:** private-original readers verify registered input identity,
   pinned extraction provenance, schema, and asset references. Public authored readers accept only
   project-authored or properly licensed content under their own format/trust rules. Neither may
   claim the other's provenance or auto-fallback to its profile. Existing private digests remain
   meaningful; no new hash registry is needed merely to notice edits.
2. **Runtime structure and capability:** decode closed records; reject duplicate/dangling refs,
   malformed enums, invalid numeric domains, bad geometry/placements, unsupported schema, and
   missing required assets. Link the start's dependency closure before session creation. Validate
   rule/opcode support separately from structural validity, including unsupported reachable branches.
   Preserve structurally valid original fields such as radius 3 even if its consumer cannot run.
3. **Reference setup and comparison:** the test/replay loader pins the selected fixture package,
   original IDs, seed, bridge/controlled inputs, expected trajectory, and comparison observations.
   Those exact fixture digests and setup identities do not become general runtime admission rules.

Public authored and private original content use the same rule implementations and semantic command
path wherever their capabilities overlap. Their byte readers, trust claims, content availability,
and distribution rights stay separate. Public validation must not be weakened into an unchecked
byte factory to achieve this. The fixed public smoke package remains a trusted regression input,
while the proposed authored-package entry point has its own strict, reviewed validation.

The minimal definition model is finite and typed:

| Definition | Required fields and joins |
| --- | --- |
| Map | Typed map/resource IDs, dimensions, layout and passability/terrain references, area/roof/warp/event lists, ordered setup alternatives, entity placements, init-program ref. Layout word bits and render geometry remain distinct. |
| Entity | Entity ID, actor/resource ref when applicable, initial position/facing, interaction program, action-program ref; mutable motion/cursor/wait belongs to session state. |
| Battle | Battle ID, map/area/terrain refs, ally placements and enemy spawn records, regions/orders, before/start/region/defeated/after program refs, leader/outcome/reward rule operands, explicit return context. |
| Actor/class | Actor and class-rule IDs, level/base stats, derived-stat inputs, HP/MP/EXP, inventory/equipment and known spell refs; authored starts and original growth-derived starts are explicitly different start policies. |
| Enemy | Enemy-definition ID, spawn baseline stats/status/items/spells, movement and AI definitions; spawn transformation and battle-local placement remain separate. |
| Item/spell | Typed definition refs and supported effect variant/parameters, equip/use restrictions, cost, ranges/targeting and presentation refs. Preserve original packed fields in source projection, not as universal inferred semantics. |
| Terrain | Fixed supported grid shape, typed cell values, movement-cost and land-effect tables. Original 48×48, byte arithmetic, neighbor order, and tie behavior constrain the initial fidelity profile. |
| Program | Program ID, ordered typed operations, validated branch/label refs, entity/text/resource refs; callable native subroutines require an explicitly implemented semantic mapping. |

Capability is a small code-owned catalog of implemented rule families and parameter domains, for
example ordinary healing, player movement, stable turn generation, and specific AI commands. Content
can require a family; it cannot declare its own implementation by setting `supported: true`.
A capability describes status/equipment/reward/target domains as well as the action name. Numeric
bounds must follow the owning rule, not a single sampled stat line. Broadening behavior beyond
evidence can be an explicit authored rule choice; it cannot be labeled Confirmed original behavior.
Do not create one capability per character, receipt, opcode helper, or test case.

### Minimal authored example

This is **Proposed, project-authored example data**, not extracted content, a current schema, or a
claim that these records can already load. The containing package supplies the referenced class,
terrain, text, and presentation records. The snippets illustrate closed variants and typed joins:

```json
{
  "formatVersion": 1,
  "profile": "public-authored",
  "ruleProfile": "sf2-semantic-subset-v1",
  "spells": [{
    "id": "mend", "level": 1, "mpCost": 3,
    "targeting": {"kind": "living-ally", "minRange": 0, "maxRange": 1},
    "effect": {"kind": "heal", "power": 15, "fullRecovery": false}
  }],
  "actors": [{
    "id": "medic-a", "classRule": "unpromoted-priest",
    "level": 1, "maxHp": 18, "hp": 18, "maxMp": 12, "mp": 12,
    "attack": 8, "defense": 5, "agility": 7, "move": 5,
    "exp": 0, "status": "none", "items": [],
    "spells": [{"id": "mend", "level": 1}]
  }],
  "battles": [{
    "id": "practice-a", "map": "yard-a", "terrain": "yard-ground",
    "allies": [{"actor": "medic-a", "position": [8, 8]}],
    "enemies": [{"id": "dummy-a", "definition": "dummy", "position": [20, 20]}],
    "leader": "medic-a", "beforeProgram": "intro-a",
    "startProgram": "empty", "afterProgram": "return-a",
    "rewardRule": "ordinary", "return": {"map": "yard-a", "position": [4, 4]}
  }]
}
```

`dummy` uses an explicitly supported source-shaped Stay commandset for the first implementation
slice; it is not an automatic fallback for arbitrary enemies. A second package can use `medic-b`,
another map and placements, changed legal stats/MP, and the same heal effect without production case
IDs. Source slot order remains an explicit ordering key where fidelity algorithms need it; authored
IDs are not cast to source combatant numbers. A private import maps its ally/enemy/leader/class
identities into those typed semantics and retains the original projection for comparisons.

Missing `yard-ground` is ContentError. A declared effect such as `resurrect` outside the implemented
catalog is UnsupportedCapability. A runtime cast with MP 2 is IllegalCommand. No loader fabricates a
missing record, replaces an unknown effect with HEAL, or treats the authored package as private
original evidence. Full packages retain the selected profile's capacity and grid constraints; the
example is not permission to expand to arbitrary dimensions or unlimited rosters.

## Program execution and connected flow

Use a small interpreter over typed operations inside Application, with effect reducers in Domain.
Reuse map selection, movement, block mutation, party/item, and flag mechanisms. This is not a general
scripting language, plugin runtime, ECS, event bus, service graph, reflection bridge, or network protocol.

The recommended initial operation vocabulary is:

| Operation family | Typed operations and execution rule |
| --- | --- |
| Control | `End`, `Jump(label)`, `BranchFlag(flag, setLabel, clearLabel)`, `Call(program)`, `Return`; retain PC and typed continuations. Source native `executeSubroutine` is not automatically a Program Call. |
| Story state | `SetFlag(flag, value)`, `JoinParty(actor)`, `GiveItem(actor, item)`; each delegates to its admitted domain rule and can fail on a real unsupported inventory/roster branch. |
| Dialogue | `SetTextCursor(ref)`, `ShowText(ref, mode, speaker)`, `ChooseYesNo(resultFlag)`, `CloseText`; preserve cursor and ordered show/close/wait behavior for the mapped source subset. User choice consumes its own wait token. |
| Entity action | `SetFacing(entity, facing)`, `StartMotion(entity, path)`, `WaitEntity(entity)`; movement progresses in explicit ticks and completion wakes the waiting program. No end-position assignment in place of traversal. |
| Map mutation/transfer | `CopyBlocks(region, destination)`, `TransferMap(map, position, facing, loadMode)`; distinguish rebuild, preserving reload, and reset. Transfer completes the ordered loading/setup continuation before field input resumes. |
| Timing/presentation | `WaitTicks(count)`, `Present(cue)`, `WaitPresentation(token)`; the cue is a closed variant such as fade, camera move, or sound request. A requested sound is not proof of audible output. |

A battle is admitted by Application's map/battle selector after a transfer, not by a story operation
that directly writes `Victory` or teleports to post-battle state. Battle hook programs reuse this
runner. Only reached supported operations are executable; the table is the recommended first
vocabulary to implement incrementally, not a claim that every operation listed is already supported.
An absent/malformed target fails content linking; a known but unimplemented operation reports
capability/PC and retains its previous commit boundary. A conditional path cannot skip validation of
its target refs. An admitted subset package may declare an explicit unsupported frontier; general
playable-package admission must not advertise that frontier as a complete flow.

For example, an authored gate program can be:

```text
0 BranchFlag(gateOpen, done, greet)
greet: ShowText(greeting, single, guard)   // wait for text completion
       StartMotion(guard, twoCellsEast)
       WaitEntity(guard)                // PC stays here until motion completes
       SetFlag(gateOpen, true)
done:  End
```

Changing `gateOpen` changes executed operations. Before the dialogue acknowledgement the guard
has not started moving; during the motion wait the flag is still clear; obstruction follows the
motion rule rather than completing the move instantly. Exact source script text/assets are private.
This authored sequence does not claim to implement the existing palace visit or original timing.

The connected Application lifecycle is:

1. **Explore:** process semantic movement/interactions, live setup/event selection, and pending
   events in their owning order. An event pushes its program; free movement pauses while its
   blocking execution owns control.
2. **Program/transfer:** execute operations, yields, and returns. A map transfer resolves the target,
   builds or preserves the working layout as required, applies flags/mutations, selects setup and
   runs init, then evaluates map/battle routing at its owned boundary.
3. **New battle:** use the selected encounter and live party. Preserve the more precise accepted
   Map 3 admission spine: before-battle program, region reset/party and enemy initialization,
   battle load, then battle-start program before first-round scheduling. The shared intro flag is
   checked by both hooks and set by the start wrapper. Other entry paths need their own admitted
   order; an older broad lifecycle summary must not override this named source seam.
4. **Round/turn:** activation → region program → spawn → turn generation. Preserve source signed
   altered agility, byte wrapping, stable ties, and draw order. Skip dead entries using current state;
   route an eligible player to input, and a supported AI/status route directly to its next transition.
   A sentinel/exhausted order generates the next round without Godot choosing it.
5. **Action:** validate/select/confirm, build ordered targets/effects, resolve and atomically apply,
   then deliver required presentation boundaries. Persistent application and presentation are
   distinct; an acknowledgement cannot apply the same effect again.
6. **Completion:** defeated-program seam → death cleanup → faction/outcome check → after-turn
   processing → death cleanup → faction/outcome check → next actor. If an earlier check ends
   battle, do not run later after-turn merely to simplify the reducer.
7. **Outcome/return:** victory heals eligible party, runs the after-battle program, applies the
   completion/unlock mutations, and returns through map loading/routing to stable player control.
   Ordinary defeat applies its own leader/gold/egress path. Battle-4 loss, special joins, EGRESS,
   suspension, or any other unreconciled branch remain explicit capabilities/Unknowns. Reporting
   battle outcome alone cannot mark ADR 0010's 5B return endpoint complete.

The after-battle wrapper's source join tail, including the zero-filled table, remains a separate
admitted service boundary. Do not invent joins or silently drop the call because the table looks
unused. This plan preserves that unresolved integration instead of claiming a complete winning route.

## Failures and diagnostics

Keep five top-level categories, with precise typed reasons and relevant field/actor/program context.
The names below are proposed semantic categories, not five new public exception hierarchies.

| Category | Example and ownership | State/result behavior |
| --- | --- | --- |
| IllegalCommand | Application: wrong actor/phase, stale revision/token, insufficient MP, out-of-range target. | Reject before gameplay commit; preserve specific reason and current snapshot. Available actions may explain the same reason. |
| UnsupportedCapability | Domain/Application: missing effect/status/AI/program/return branch with otherwise meaningful input. | Stop at the last coherent boundary; identify capability and phase/operation. Never silently substitute STAY or an endpoint. |
| ContentError | Content: malformed record, duplicate/dangling ref, invalid numeric field, schema/provenance mismatch. | Reject start/package or the explicit loading boundary; preserve safe field/resource context. No partial package publication. |
| InvariantFailure | Domain/Application: impossible internally constructed state, duplicate effect application, inconsistent actor/control authority. | Freeze and surface original cause with diagnostic context; do not reclassify as user input or startup refusal. |
| AdapterError | Godot/platform: missing scene binding, failed asset presentation, transport/probe error. | Preserve gameplay snapshot; expose the presentation failure. Do not fabricate its acknowledgement or report a passed observation. |

Expected failures are typed results. Map exception messages and causes only at the owning boundary;
do not catch every `ArgumentException` and discard its message into a generic startup diagnostic.
Public diagnostics use logical resource IDs and safe field names, with private details confined to
local output. No new logging service, diagnostic manifest, or per-helper public result family is needed.

## Independent verification and concrete counterexamples

Separate product unit tests from direct verification:

- **New-engine unit tests:** project-owned independent expected values and invariants from the linked
  contracts/fixtures, plus authored state/configuration variation. Compare arithmetic, targets,
  content semantics, command/state transitions, RNG, effects, and atomicity without calling production
  helpers to calculate expected values. A small in-memory GameSession with actual reducers can be a
  useful unit boundary. Generation of legal histories uses commands from admitted starts.
- **Reference verification:** ADR 0009/Map 3 and existing Battle01 comparisons pin exact inputs, seeds,
  controlled bridges, receipts, expected states and refusal boundaries. Preserve original H2/H3
  fixtures unchanged. A reference runner reports divergence; the runtime does not refuse legal
  play merely because the player departed from that transcript. Do not unit-test the runner, its
  fixture driver, comparison/report code, or setup helpers.
- **Direct adapter observation:** a semantic transcript under public/private readers and the Godot
  adapter verifies routing and projection, not original facts. Observe actual
  session/node state, visibility, geometry, focus, input routing, and error output where the gate
  needs them. Expected source values alone are not observations of a running Godot instance.
  Run the needed observation itself; do not add tests of the probe, smoke host, launch wrapper,
  exported marker, or verifier, and do not rebrand those tools as engine features to preserve tests.

At minimum, implement these **authored test specifications** when the corresponding rule domains
are migrated; they are not new original goldens. State/rule rows become ordinary engine unit tests;
the running-adapter row is direct verification, with no additional testing layer:

| Test | Concrete setup and expected behavior |
| --- | --- |
| HEAL positive, changed history (A1/A7) | Admitted healer at HP 95/100, MP 20, EXP 0, unpromoted priest rule, known power-15/cost-3 self HEAL, no unsupported equipment/status. Reach player control once in round 2 and again in a separately generated round-5 history. Use the same resolver seed `0x1234` in isolated rule-seam cases: recovery 5, accumulated EXP 10, award rolls 14/0, ordered MP `20→17`, HP `95→100`, EXP `0→9`. Neither case requires Sarah, raw offset 4, or 102 receipts. Full session tests use naturally carried seeds and independent expected draws, not a runtime seed-reset command. |
| HEAL negative, real legality (A1/A2) | In either history, set the authored start's available MP so it reaches the cast with MP 2, or choose a living ally outside configured range. Refuse the cast/confirmation with the exact resource/range reason; no MP/HP/EXP/turn/RNG commit. A stale selection from the earlier revision also refuses without effect. |
| HEAL positive, different configuration (A2/A7) | A second package has a different actor, encounter, map and legal placement; priest max HP 20/current HP 8, MP 12, EXP 0, power 15/cost 3. Self recovery is 12, accumulated EXP is `floor(25×12/20)=15`; at isolated seed `0x1234`, award is 14, MP becomes 9, HP 20. Different unrelated equipment/status is allowed only once its actual rule capability exists, not by erasing its guard. |
| Capability/content negatives (A2/A8) | Replace that spell's effect by an unimplemented resurrection family: report UnsupportedCapability rather than a case-ID mismatch. Remove the referenced terrain or duplicate an actor ID: ContentError. Tamper with private original provenance: retain the trust rejection even if the JSON shapes otherwise match. |
| Turn and kill order (A1/A7) | Generate two valid histories in which different supported physical actors kill an ordinary enemy in opposite orders. For each, assert cleanup/worklist/reward rules and the next live actor. Death count and named character order never gate legality; preserve actual leader-death and special-enemy rules. Gate this case on physical/death/reward migration, not the HEAL-only first slice. |
| Program wait and branch (A3/A6) | Execute the authored gate example with flag clear/set. Only the clear path emits greeting and motion. Wrong/double ACK does nothing; obstructed motion cannot set the flag early. A supported inventory change survives subsequent movement and dialogue. An unknown next operation stops at its PC without terminal-state injection. |
| Direct Application/Godot observation (A4/A5/A8) | Run the same command sequence headlessly and through the adapter; compare session snapshots, next actor, RNG, and ordered observations. Failed presentation is AdapterError and leaves its wait pending, rather than advancing the scheduler. The actual session's wait/failure behavior belongs in an engine unit test; the live probe is used directly, never tested by another harness. |

Classify each touched test family by the behavior it still protects. Migrate useful illegal-command,
content/trust, invariant, and unsupported assertions to direct new-engine unit tests. Fixed-prefix
assertions belong in selected reference cases, including a historically useful old refusal when it
explains a boundary; obsolete structural refusals can be deleted. If a guard combines rule and trace
conditions, separate them and keep the independent behavior assertions. There is no requirement to
preserve each old test method, run every historical case, or add a replacement test for each deletion.
Do not loosen original research goldens/digests or call general unit-test success original H4 parity.

Required invariants include no resources/RNG consumed by cancellation or rejection, ordered replay,
single application, stable tie behavior, equal semantic result for equivalent state regardless of
reference-history length, and deterministic replay under the same policy/inputs. A valid ineffective
spell is tested separately from rejection. Unsupported post-commit continuation tests must retain
the committed preceding action. Add only unit tests needed to protect those engine behaviors; use no
test-count, coverage-percentage, helper-validation, or meta-test acceptance target.

Screenshots are currently prohibited. State/node probes can validate semantic and adapter claims;
they cannot establish exact pixels, palette, audio waveform/chip behavior, VInt/DMA, or frame timing.
A debug/probe transport is not selected by this ADR. Future observation work must use an explicitly
chosen read-only interface and the existing running Godot instance where applicable, with no hidden
screenshot side effect. An unavailable observation remains unverified, not passed by source review.

## Old-test migration and retirement

The table classifies actual families at the accepted base. Rows can contain mixed assertions; move
the useful behavior, not the old private-field setup or method count. A new ordinary xUnit project,
proposed at `remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj`, references the
existing Domain, Application, and Content projects and reuses central package versions/locks. This
single project permits engine unit tests to run independently of legacy tests and the Godot SDK.
It is not a new test framework, runner, coverage platform, or source of gameplay truth. Do not leave
duplicate assertions running in both projects once that family is migrated.

| Existing directory/family | Keep as new-engine behavior assertions | Reference-only or retire | Migration time and old-gate closure |
| --- | --- | --- | --- |
| `remake/tests/Sf2.Remake.Domain.Tests/Battles/Battle01PlayerHealingTests.cs`, `Battle01PlayerMovementTests.cs`, `Battle01FirstRoundTests.cs` | Healing arithmetic/cost/EXP/RNG, range/occupancy, cancellation, turn ordering and genuine legality. Use small authored inputs through the new engine. | Fixed Sarah/round/offset/receipt assertions become selected reference expectations; reflection into turn internals, exact private object identity and giant copied history setup retire. The current HEAL test mixes valid effect assertions with receipt 103 and round 13: separate those claims. | M0/M1. Stop selecting these legacy families for the new engine once their product behaviors have direct unit assertions and relevant trace facts have an explicit reference home. No preservation of test totals. |
| Remaining Domain `Battles/Battle01*Tests.cs`, including physical, pursuit, standby, completion, defeat | Actual AI choices, physical effects, rewards, death/status/outcome rules and atomic transitions as each capability migrates. | Exact named kill order, generated transcript length and historical frontier positions are reference cases. Internal constructors/receipt-chain identity, reflection and duplicate helper checks retire. | M2/M4 capability by capability. A retired unsupported feature is reported absent rather than retaining a meaningless old passing test. Legacy gate closes for each migrated family; it need not await all battle features. |
| Domain `Maps/*Tests.cs`, `Items/ItemInventoryTests.cs`, `Battles/TacticalBattleTests.cs` | Existing map/flag/layout/inventory rules with real new-engine consumers. | Synthetic micro-battle rules are authored reference behavior unless intentionally adopted; do not import them as original-game rules. Redundant layout-copy and setup assertions retire. | M1/M3/M4 when the consuming engine function moves. Keep one direct assertion per useful behavior, not parallel old and new suites. |
| `remake/tests/Sf2.Remake.Application.Tests/GameSessionTests.cs`, `OriginalMapGameSessionTests.cs`, `OriginalMapVisualGameSessionTests.cs`, `PrivateOriginalBattle01*Tests.cs` | One-authority command admission, stale/cancel/atomicity, content linking, program wait/return, failures, and deterministic continuation. | Private field injection into `_privateOriginalMapSnapshot` and locomotion fields, null profile accessors, constructor shape and repeated whole-snapshot copying retire. Selected scenario endpoints/receipt projections move to reference verification. | M1–M4 with each common-session path. Gate removal depends on the new path's actual behavior, not source-compatible access to old fields or a green entire Application project. |
| `remake/tests/Sf2.Remake.Content.Tests/*ReaderTests.cs`, Application content-definition tests | Product loader semantics: malformed/duplicate/dangling refs, supported variants, numeric/range rules, public/private trust, atomic package admission. | Exact fixed original/smoke package identity belongs in the selected reference/trust verification case; repeated digest mutation matrices and duplicate parse checks retire. Preserve the loader's genuine provenance rejection as a unit behavior. | M1/M2/M3 with reader migration. One strict loader behavior test can replace many equivalent digest-only cases; no test is required solely to justify deleting another. |
| `remake/tests/Sf2.Remake.Godot.Tests/*PresenterTests.cs`, input/viewport/profile/catalog tests | Only actual rule/state logic that was misplaced in the adapter moves with that production logic into engine unit tests. | Needed visible/input/asset-mount behavior is checked directly against the running adapter. Retire old presenter signature/name, exact marker/copy, reflection, duplicate projection and probe/fixture self-check constraints where still present. Do not rename adapter or probe utilities as engine behavior. | M1/M3 as routing/projection moves. Remove this project from mandatory new-engine CI at M0; keep only explicitly needed direct adapter verification for changed surfaces. Prior audit repairs already removed some of these constraints; do not recreate them. |
| Test helpers inside the four `remake/tests/Sf2.Remake.*.Tests/` projects | Small data builders directly used by engine unit tests, with no tests of the builders. | Giant walkthrough constructors, reflection mutators, fixture self-checks, signature/count inventories, and duplicate assertions about verification output retire. Retain necessary selected trace data in existing reference/observation formats. | Delete with the last owning family, without a replacement-helper test suite or a new helper-validation matrix. |
| `tests/python/test_remake_architecture.py` | No migration into engine unit tests merely to preserve source-shape checks. Real inward dependencies remain build/review constraints. | Retire exact four-project discovery, source-string/name/visibility, workflow text, lock/version and helper-placement assertions from new-engine CI. Review dependencies and build the affected product instead. | M0 CI/policy cutover. Remove the old CI step and planner mappings together; no new test of the new workflow. |
| `tests/python/test_verification_plan.py` and engine/gate/CLI portions of `test_native_harness.py` | No new-engine unit assertions: these test verification infrastructure. | Retire engine planner/gate dispatch, preflight, command registration, report/count and driver meta-tests; exercise the needed verification command directly. The native harness also contains original-data/research checks: separate that other owner's responsibilities before deleting mixed content. | M0 with tooling/main-gate ownership. Remove these files from new-engine required checks immediately; research-only content stays isolated until its owner selects its direct verification route. |
| `tests/python/test_dotnet_environment.py`, `test_remake_godot.py`, `test_godot_ai_probe.py` | None: process setup, download/launch/probe wrappers are not engine gameplay. | Remove wrapper, protected-environment, archive, version, timeout, probe transport and artifact-report tests from engine CI; maintain required environment protections in actual launches and observe actual failures directly. Retire obsolete verification tests without replacing them. | M0. Shared process/private-input protections remain owned by tooling; this is not permission to weaken those protections or delete shared code without owner review. |
| `tests/python/test_remake_assets.py` | New-engine Content unit tests cover actual runtime content/asset-reference admission where relevant. | Export/preflight/descriptor/report tests do not become engine tests. Remove from engine CI; asset owner decides retention/retirement of actual asset-product assertions separately from redundant verification tests. | M0 for CI isolation, M2/M3 for runtime admission migration. Do not delete authored assets, licensing checks, private-input protections, or original evidence to shrink this suite. |
| `tests/fixtures/h2/**`, `tests/fixtures/h3/**`, research schemas/manifests and existing research executables | Consume minimal licensed facts as independent engine unit-test inputs where needed. | Preserve original evidence and named reproduction routes. They are neither obsolete engine implementation tests nor a mandate to run every old case for each engine commit. No additional tests of fixture drivers or reference verifiers. | Research/main-gate ownership throughout. Changed research contracts select their own direct verification; engine-only edits do not close or invalidate original 8C/H4 by themselves. |

Deleting a test does not delete its historical Git object or completed run record. Preserve prior
failures, exact failing nodes when any, skipped boundaries, and whether processes completed in the
handoff/owning evidence. The old [test suite audit](../../remake/docs/test-suite-audit.md) is a
historical diagnostic, not a permanent retention rule or numerical acceptance target.

## Remote CI and local verification cutover

The accepted [.github/workflows/public-checks.yml](../../.github/workflows/public-checks.yml) runs
on every PR and main push in one Windows `tracked-inputs` job. It installs Python 3.12, uv, .NET
10.0.204/8.0.424, runs Ruff, all seven named Python families above, design-contract traceability,
then locked restore/build/test of the entire `remake/Sf2.Remake.sln`. It does not launch the native
Godot executable, but the whole solution includes the Godot SDK and old test projects.

Locally, [harness.py](../../src/sf2tool/harness.py) selects
`tests/python/test_native_harness.py` through `COMMIT_PYTEST_TARGETS`; normal `verify` also runs
design traceability, research index, ROM identity and upstream toolchain provenance.
[verification_plan.py](../../src/sf2tool/verification_plan.py) always adds `public-core`, maps
non-document remake paths to the old .NET solution and often Godot, and maps workflow/architecture/
planner/tooling changes back to those old gates. Changing only the CI test command would therefore
leave both local selection and accepted guidance able to restore the obsolete requirements.

The recommended replacement keeps the existing workflow's PR/main-push trigger and splits its job
responsibilities. A small inline PowerShell step reads the actual Git changed paths and emits only
the three needed scope booleans; it performs no verification. Conditional jobs then use the explicit
path scopes below. Keep this wiring in the existing YAML, coordinated with the existing local planner;
do not add a generic dispatcher, workflow-validation framework, coverage job, meta-test job, matrix
generator, or test-discovery manifest.

| Job/entry | Trigger scope after cutover | Required execution/environment and exclusions |
| --- | --- | --- |
| New `engine-unit` job | PR/main changes to Domain, Application, Content, the new engine unit-test project and its authored inputs, shared remake build/package/SDK files, or this workflow. Add a genuinely consumed reference input explicitly if it becomes a unit-test dependency. Documentation-only and unrelated research changes do not execute it. | Windows runner initially; existing pinned .NET SDK 10.0.204 and net8 runtime 8.0.424, central xUnit packages and locked restore. Build/test only `Sf2.Remake.Engine.Tests.csproj` and its three production dependencies. No Python, native Godot, ROM, upstream checkout, private assets, old solution tests, probe tests, or coverage target. Preserve explicit CLI selection, PATH opt-out and per-run writable state in the real job. |
| New `adapter-build` job | PR/main changes to `remake/game/**`, its shared build/lock configuration, Domain/Application/Content API dependencies, or this workflow. | Locked restore/build of the actual Godot C# project using the tracked Godot.NET.Sdk/4.7.2 and .NET pins. This is compilation, not an extra automated test layer or native visual acceptance. It does not run the old Godot test project or download/launch the native editor. |
| Existing public workflow, narrowed to `research-public` scope | Changes to research/design contracts, owning research source/schema/fixture/manifest/toolchain paths, genuinely shared Python/private-input protections, or its workflow. Engine-only changes no longer trigger whole-solution restore/build/test or the seven old Python test families. | Keep uv locked dependencies and relevant lint/direct public traceability/index checks under repository/tooling ownership. Separate mixed native-harness responsibilities; remove engine/meta-test steps, not research evidence. Private H0/H1/H3 runs cannot become public CI inputs. Ordinary engine unit CI has no dependency on this job when its paths are unchanged. |
| Reference replay and live adapter checks | Only an affected comparison/observation or explicit milestone/user request; use the existing local reference/probe route. | Execute the verification itself with admitted inputs and existing host/instance policy. No tests of runners, fixture drivers, gate/planner/report code or probes. Retain exact results and unsupported/private boundaries; no automatic screenshot or full-route claim. |
| Local new-engine selection | New-engine source/content/test changes map to the same targeted engine project and applicable compile/direct observation. | Remove the engine requirement for normal legacy `verify`, whole-solution tests, blanket native Godot and tooling pytest fanout. Research/shared changes retain their owning selection. The planner can report the selection, but gets no new tests of that selection. |

The implementation enumerates the scope predicates from those concrete roots and actual dependencies,
including the workflow itself, in the same reviewed change as local selection. A missing base or failed
path comparison fails scope determination rather than declaring all jobs irrelevant. Main-gate updates
the required check contexts from the old monolithic job to the new scope/unit/build/research jobs;
the workflow remains triggered so path-irrelevant jobs can report explicitly skipped. An applicable
unit job must have an actual result; neither a missing workflow nor failed scope determination is
accepted as success. Review wiring, required-check settings and the actual first CI run directly;
do not unit-test the scope step, build a GitHub-filter test harness or generate synthetic PR matrices.

The new engine project can use the ordinary existing commands:

```powershell
dotnet restore remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj --locked-mode
dotnet build remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj --configuration Release --no-restore
dotnet test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj --configuration Release --no-build --no-restore
```

These are proposed future paths, not commands run or a project created by this ADR. Native executable
selection and environment reuse still follow the owning host policy. No new test framework, SDK
installation, or run is required to review this design.

Cutover is part of M0/M1, not a backlog item after the engine is declared complete:

1. **Engine owner** creates the small unit project with the first migrated real behavior and removes
   the corresponding duplicated legacy assertions. Keep old runtime entry points only while actual
   unmigrated capabilities still need them; old tests are not an architecture constraint.
2. **Repository/tooling owner**, serialized with the engine owner, changes CI workflows,
   `src/sf2tool/harness.py`, `src/sf2tool/verification_plan.py` and any affected CLI registration
   to distinguish engine execution from research verification. Remove obsolete pytest mappings,
   whole-solution fanout and deleted-test fallback for deliberately retired engine test paths.
   Do not remove conservative unknown-path handling for unrelated research, and do not add tests
   of the revised gate or planner.
3. **Main-gate/guidance owner** synchronizes root `AGENTS.md`, `README.md`,
   [ADR 0012](0012-dependency-aware-partitioned-verification.md),
   [ADR 0017](0017-heavy-boundaries-light-internals.md), `remake/README.md`,
   [development and verification](../../remake/docs/development-and-verification.md),
   [test suite audit](../../remake/docs/test-suite-audit.md), and any engine routing references in
   `docs/operations/agent-resume.md`. Review the Phase 2 runbook's shared gate references but keep
   research lane requirements with their owner. Replace blanket preservation/all-suite/meta-test
   obligations for the new engine and label retired gate recipes as historical, so a resumed agent
   cannot reinstate them from old guidance.
4. **Close each old gate** when its actual migrated behavior has useful engine unit assertions,
   selected trace facts have their reference home, no unmigrated product path depends on that gate,
   CI/local mappings and required checks point to the intended new commands, and guidance agrees.
   Confirm this from the diff, direct selected commands, and actual CI outcome. No test-count parity,
   coverage quota, green old aggregate, extra test of the cutover, or per-deletion replacement test
   is a closure condition. Unrelated research/private-input protections remain in their own scope.

This design slice changes none of those shared CI/test/planner/guidance owners. Their changes must be
declared and coordinated in the implementation handoff, rather than inferred as permission to edit
shared files now. The current completed verification results remain exactly as observed; this policy
addition does not justify replaying normal/full/.NET/Godot suites.

## Incremental migration and stopping conditions

Recommended order is below. Candidate paths name future responsibility, not writes authorized by
this design slice. Each implementation slice starts from accepted main, declares exact paths/tests,
checks competing writers, and obtains independent review. Do not create empty scaffolding for later
rows or execute this entire table as one rewrite.

| Stage | Concrete change and candidate owners | Dependencies, acceptance, stop and rollback |
| --- | --- | --- |
| M0: classify and cut over verification | Inspect the existing four remake test projects; move useful selected HEAL/movement/turn assertions into the proposed `Sf2.Remake.Engine.Tests` project. Extract only their needed pure mechanics from `Battle01PlayerHealing.cs`, `Battle01PlayerMovement.cs`, `Battle01FirstRound.cs`. Repository/tooling/guidance owners perform the CI/local-policy cutover above in coordinated, serialized changes. | No unintended product behavior change during extraction; direct engine unit assertions protect the moved mechanics and selected reference verification retains meaningful old observations. Retire obsolete/reflection/meta-tests rather than characterizing their structure. Stop when M1 has real behavioral tests and the new gate path; no repository-wide test inventory or extra tests of the cutover. Revert incorrect extraction, retain completed failures; do not require a green old aggregate. |
| M1: first connected functional vertical slice | Content: internal authored package decoder/validator beside existing readers, e.g. `AuthoredScenarioPackageReader.cs`; Application content typed definitions in `Content/ScenarioDefinition.cs`. Application: evolve `Sessions/GameSession.cs`, internal `SessionCommandDispatcher.cs` and `BattleSessionCoordinator.cs`. Domain: focused `Battles/PlayerHealing.cs`, `PlayerMovement.cs`, `BattleTurnFlow.cs`, `BattleTurnOrder.cs` using M0 mechanics. Godot: `remake/game/src/PrivateBattle01Composition.cs` and the owning input/presenter composition receive the common result for the new path. | After M0, load an authored controlled encounter into player control; move/preview/cancel, HEAL or STAY, atomically commit, skip a dead queued entry when present, execute the explicitly configured Stay AI commandset, generate another round and return to player input automatically. Two authored packages and the first four examples above require engine unit assertions plus direct observation of the applicable adapter path. No tests of that observation. No physical attack, level-up, general AI, event program, or victory claim. Stop at unsupported branches precisely. Keep the accepted reference entry selectable at composition until M2; rollback selects that entry before session creation, never switches a live session between authorities. |
| M2: migrate battle rules and private admission | Replace trace guards in `Battle01FirstRound.cs`, `Battle01EnemyStandby.cs`, `Battle01EnemyPursuit.cs`, `Battle01EnemyPhysicalAttack.cs`, `Battle01PlayerPhysicalAttack.cs`, `Battle01TurnCompletion.cs` with reusable internal rule modules in the same Domain battle directory. Evolve `PrivateOriginalBattle01StartupReader.cs`, `Application/Content/OriginalBattle01StartupDefinition.cs`, and controlled presets into trust/import mapping plus reference setup. Route private battle commands through the common Application dispatcher. | M1 is accepted. Extract one actual dependency chain at a time: AI movement/decision → physical effects/replay → rewards/death/after-turn → next control. Bring level-up or other reached branches only through their owning accepted rules; otherwise report Unsupported. Preserve private provenance and selected reference observations while adding actual alternate-actor/kill-order engine unit assertions. Retire the migrated legacy families. Remove Godot `DispatchNext` once Application runs those paths. Stop each chain at a coherent capability frontier, not the next receipt. Roll back affected start binding/commits if parity or atomicity fails. |
| M3: programs and exploration on the common session | `Application/Sessions/OriginalMapGameSession.cs`, `GameSession.cs`, existing `Map*Lifecycle.cs` and `PrivateOriginalMap*cs` owning the reached event; internal `Programs/MapProgramRunner.cs` and Domain typed operation/state reducers. Content: `OriginalMapRuntimeAdmission.cs`, `PrivateCanonicalMap3ImportReader.cs`, and a typed `ProgramDefinition.cs` at the existing content boundary. Replace one endpoint-assignment handler, beginning with the palace owner's behavior only when its complete reached operations are admitted. | Common facade exists; actual program operation/wait/effect behavior has small engine unit tests before replacing a story handler. Connect authored explore → branch/dialogue/motion → transfer → battle admission, then migrate private original operations with source provenance. Reuse layout/entity/dialogue reducers. Retain selected endpoints as reference comparisons only after actual execution reaches them; retire obsolete constructor/receipt tests. Stop at unsupported native subroutine or timing/presentation frontiers; never inject terminal state. Roll back the selected route if the affected reference comparison disagrees. |
| M4: outcome and return | `BattleSessionCoordinator.cs`, program runner, map transfer coordinator; existing `Battle01DefeatRecovery.cs`, `Battle01DefeatReturn.cs`, Application counterparts, and the owning after-program content mapping. | Accepted action/death/after-turn and required program operations first. Implement authored battle outcome → after-program → completion flags → return map → stable input, then directly compare only admitted original paths. Unit-test actual outcome/state behavior; retire old endpoint-only implementation tests. Preserve leader-loss and special outcome boundaries. Do not claim ADR 0009 complete until natural continuity, accepted endpoint and required fidelity evidence are satisfied. Roll back incomplete outcome publication; never mark victory-as-return. |
| M5: remove migrated coupling | Delete superseded runtime trace predicates, scenario-specific live snapshot fields, duplicate profile dispatch, unused scheduling entry points and obsolete legacy tests in the migrated owners. Preserve original evidence and selected reference verification. | All affected callers use the common session; useful engine unit tests and affected direct reference checks pass; review confirms no production dependency on reference history predicates. Trust and observation boundaries remain explicit. Remove the temporary legacy start binding after its last admitted capability migrates. Confirm old CI/local/guidance obligations are retired under the closure rules above. No permanent parallel engine, compatibility layer, or mandatory old whole-solution suite. |

M1 is deliberately a useful connected battle capability with a small content domain, not a declaration
that its authored direct battle start satisfies ADR 0009. Its configured Stay enemies exercise actual
automatic scheduling and RNG without pretending general enemy behavior exists. Test-only legal-state
builders may isolate a dead-slot or seed seam; end-to-end histories must come from admitted starts
and commands. Starting HP deficits must be an explicit authored controlled-start policy, separate
from original new-battle party healing, so the first HEAL cannot accidentally rely on a bypassed rule.

The migration reuses confirmed arithmetic, navigation order, AI selectors, turn generation,
after-turn/status logic, scene-effect replay, and reward/accounting rules. It does not mechanically
reuse their walkthrough guards. Existing `TacticalBattle.cs` public-synthetic behavior remains a
characterized consumer until migrated; do not treat its simplified rules as an SF2 oracle or install
a second permanent generic battle engine beside it.

For implementation stages, the synchronized CI/local cutover above is part of acceptance. Run the
new engine's useful unit tests and affected product build/direct verification, not every legacy
Domain/Application/Content/Godot project or tests of tooling. Update the committed planner and its
guidance so the selected paths agree; engine-only changes must not resurrect the old always-run
`verify`, whole-solution, full/native, or infrastructure pytest obligations. Research/shared changes
retain their independently owned requirements. Preserve completed failed nodes and process state;
rerun only corrected engine behavior or newly affected direct verification, not an old aggregate
merely to replace its result. This is an explicit proposed amendment to the old engine gate route.

For this **documentation-only proposal**, acceptance is the exact three owned documents, working
relative links/anchors, honest current/Proposed distinction, no private payload or absolute private
input paths, the honestly retained normal public verification result, and a clean committed planner
inspection. The added CI/test policy changes only documentation: retain completed checks and perform
only necessary document-increment checks. Do not replay normal/full/.NET/Godot suites or run H3/native
visual tests for this slice. Current planner output is reported as current behavior, not proof that
the future CI cutover is implemented.

## Existing decisions and user decision items

| Accepted decision | Compatibility and proposed amendment, if adopted |
| --- | --- |
| [ADR 0009](0009-first-phase4-playable-slice.md) | Keep its Map 3 → completed Battle 01 reference milestone and source evidence. Runtime capabilities serve general valid play; that walkthrough becomes an external comparison/acceptance route, never a source of legality predicates. Completing general engine slices does not complete 0009. |
| [ADR 0010](0010-map3-battle01-product-acceptance.md) | Keep the named start/return, control, content, restart-only, access, and deviation boundaries. 4A already permits interactive divergence under deterministic rules. Recommend a separately reported modern semantic playable deliverable while 7C content trust and the accepted 8C target retain their own gates. Any removal/redefinition of 8C or original start/return requires an explicit user decision and owning ADR amendment. |
| [ADR 0011](0011-phase4-remake-runtime-architecture.md) | Preserve four layers, one state authority, semantic commands, deterministic RNG, typed content/programs, and stable observations. This proposal makes those boundaries concrete and repairs their currently incomplete implementation. An exact 8C presentation backend remains undecided. |
| [ADR 0012](0012-dependency-aware-partitioned-verification.md) | Preserve dependency-aware selection for original research. Amend new-engine partitions, always-run public-core/whole-solution assumptions, obsolete-test deletion fanout and planner/tooling-test requirements during M0/M1. The explicit user policy is new-engine unit tests plus needed direct verification, with no tests of verification infrastructure. Update executable mappings and guidance together. |
| [ADR 0016](0016-remake-start-evidence-deferral.md) | Preserve bounded start and static-first deferral. Authored connected slices and accepted static semantics can proceed with explicit Unknowns; no blanket new H3 requirement, natural-route claim, or waiver of eventual evidence. |
| [ADR 0017](0017-heavy-boundaries-light-internals.md) | Preserve heavy trust/mutation/versioned-observation boundaries and lightweight internal calls. Add the explicit behavioral migration path: M0 protects moved product behavior with useful unit assertions; later stages expand valid state/content admission. Amend blanket characterization/old-test retention assumptions; tests of internal structure and verification infrastructure may retire without replacements. This expansion is not a behavior-preserving file split. |

The proposal does not silently amend those accepted decisions. The recommended product tradeoff is
to deliver and measure semantic playability first, while separately reporting original presentation
parity as incomplete. This avoids coupling engine structure to one route without lowering an accepted
fidelity bar by implication.

The new-engine unit-test-only addition policy and consideration of old-test retirement are already
explicit user requirements, not unresolved permission questions. The remaining design/product
decisions for the user/main gate before implementation planning are:

- Adopt this state/content/capability direction and M0 → M1 starting boundary, with implementation
  scoped separately; this Proposed ADR itself does not dispatch a new implementation task.
- Confirm whether modern semantic playability is a separately named deliverable while original
  8C/H4 remains accepted but incomplete, or explicitly revise that product target in its owner.
  Recommendation: separate reporting and preserve 8C until an explicit revision.
- Select the later observation interface and permitted hardware/audio/frame evidence when needed.
  Recommendation: deterministic session and actual node/input probes for ordinary engine work,
  with screenshot prohibition retained; do not claim those probes close visual or audio parity.

No user save/load, engine replacement, universal scripting framework, new public asset rights, or
hardware emulation backend is introduced. The design slice stops after a frozen Draft PR and
independent review; unanswered product/fidelity choices and audit findings remain visible.
