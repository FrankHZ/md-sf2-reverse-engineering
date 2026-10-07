# Gameplay Rules and Composition

Domain owns deterministic calculations and immutable state transitions. Application selects one
immutable composition and owns when each prepared effect is published. Content admits data;
Godot presents semantic choices and effects. Start with [architecture](../architecture.md) for
assembly/state authority and [profiles](../content/profiles-and-trust.md) for admission.

## Scope and Fixed Services

The independently accepted finite [#617 scope](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/617)
supports replacing admitted HEAL/physical/Herb/Stay algorithms, automatic decisions, progression,
ordinary outcomes and source-story policy through compiled C# composition. External typed programs
provide conditional story authoring. Rebuild and start a new session; there is no hot reload,
runtime class discovery, general effect language, plugin sandbox or live rule switch.

Refresh, activation/control, movement-cost tables, field motion/text/music and battle-scene logical
services remain fixed source-specific algorithms. They are deliberate exclusions, not evidence
that all gameplay policy is replaceable. Broader replacement needs a separate scope decision.
Save/load, new effects/statuses/outcomes and complete game coverage are outside this boundary.
Compile-free extensible scripting remains **Unknown** as a future requirement.

The implementation used High rigor because rules, RNG, source compatibility and presentation share
commit boundaries. Its current agreement is the finite behavior and exclusions here; completed
dispatch/resource estimates remain in [the historical plan](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/1c4c786a6e2c630e1cfadf4daa88dbd7687caa19/remake/docs/architecture.md#replaceable-gameplay-logic-plan).
[#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438) still awaits UI/UX discussion;
semantic affordances do not choose its layout/art. #638 remains paused and Chinese substantive
synchronization remains deferred.

### Implemented HEAL seam

[`SessionRules`](../../src/Sf2.Remake.Application/Runtime/SessionRules.cs) retains one internal
[`IHealingRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IHealingRule.cs), adapted to the common finite
action interface, for the session's lifetime.
[`RuleCompositions`](../../src/Sf2.Remake.Application/Gameplay/RuleCompositions.cs) owns the default
SF2 and the two authored factories; its `ForGame` method is the ordinary host's compile/restart
selection. Source-start and definition/start overloads retain explicit composition without
mutable selection. Default direct starts use SF2. Diagnostic rule identity is separate from Content
profile and never chooses arithmetic in dispatcher/query/view.

[`Sf2HealingRule`](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2HealingRule.cs) owns source admission
and preparation; the single [scalar](../../src/Sf2.Remake.Domain/Gameplay/Sf2/HealingRules.cs) and
[target helper](../../src/Sf2.Remake.Domain/Gameplay/Sf2/HealingTargetRules.cs) are shared with Herb.
[`AuthoredHealingRules`](../../src/Sf2.Remake.Domain/Gameplay/Authored/AuthoredHealingRules.cs)
implements capped/proportional award A and injured-only half-missing/no-award B. The default
source class/award assertions remain independent of the demonstrations.

`GameSession.QueryBattleChoices` returns immutable session/revision/actor/stage/destination facts,
spell labels/presentation/range and ordered same-side living candidates with enabled/reason data.
Query runs no preparation or RNG and retains no future seed. The actual view consumes these spell
choices through the common action query described below. Confirmation
rechecks the selected rule and private cast capability, then `ValidateHealing` checks the existing
resolution before `Begin`: only selected movement/main construction RNG, bounded target recovery,
caster MP cost and later caster progress are permitted. Foreign state, queue, thinking RNG,
accounting, definition and observation effects cannot be published by malformed HEAL output.
Construction random facts must form a complete `BattleRandom` main-seed chain; their count,
purpose names and ranges belong to the selected rule. SF2's two range-16 EXP draws are asserted at
its own behavior boundary, rather than imposed on every HEAL implementation.
Unexpected selected-rule errors report `InvariantFailure` with rule/operation context, without
exposing exception details. Expected rule errors retain their existing categories. Source unknown
EXP remains a reached preparation failure; selection does not prematurely spend or require it.

The existing scene owns cost, HP, EXP, fairy work and later growth. Growth consumes the then-current
seed. A reached later growth failure retains the earlier committed resources, progress, token and
queue; no whole-action rollback or successful release is implied. The cursor's source operations
are unchanged. [Verification](../verification/battles.md#replaceable-heal-observation)
owns engine assertions and the bounded actual A/B observation. Original timing and additional action families remain outside this HEAL seam; the selected
AI/progression seams below and [story policy](../application/session-and-programs.md#source-story-policy)
have their own finite replacement boundaries.

### Implemented semantic actions

[`IBattleActionRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleActionRule.cs) consumes the finite
`BattleActionRef` (Stay, HEAL spell, physical, or inventory slot). Each selected implementation owns
its action offers/admission, ordered target candidates, target admission and preparation.
`SessionRules` binds one immutable instance per admitted family; the accepted HEAL strategy uses an
adapter to this interface, without a second calculator. `IPhysicalActionRule.EstimateDamage` supplies
the existing AI estimate as well as the same rule's preflight and eventual action preparation.

[`Sf2PhysicalAction`](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2PhysicalAction.cs) and its single
[`PhysicalStrikeRules`](../../src/Sf2.Remake.Domain/Gameplay/Sf2/PhysicalStrikeRules.cs) own source strike,
reversal/double/counter and land-protection policy. The movement-cost table stays in
`BattleTerrainRules`. [`Sf2ItemAction`](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2ItemAction.cs) owns the
admitted Herb calculation and source numeric item decoding; Content trust and the inventory model
are unchanged. [`Sf2StayAction`](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2StayAction.cs) prepares ordinary
player Stay. Standalone scalar comparisons use explicitly named source-default convenience entries;
production entry, scene, movement and program continuations always pass the selected composition.

`QueryBattleChoices().Actions` orders physical, learned spells, actual inventory slots, then Stay.
Offers carry the action reference, label, presenter kind, range, empty state and enabled/reason data;
targets retain the rule's order, including disabled candidates. `Spells` and `Items` are derived
projections of those same offers. Empty packed source slots remain raw and unchanged in live state,
while the view receives semantic Empty/label data. Unknown or unsupported slots retain a diagnostic.
Selection remains provisional; `SelectBattleAction`, `ChooseAction`, `SelectSpell` and `SelectItem`
join the same admission path. Confirmation rechecks the live envelope, selected actor, destination,
action, target and supported scene before one common preparation/validation call. Godot cycles query
results and formats labels; it does not infer target factions or mask item words.

`BattleActionResolution.Validate` permits only the finite existing publications: selected movement
and main construction RNG, one selected consumed slot/compaction for Herb, and physical death gold;
HP/MP/reward/death accounting and queue consumption retain their later owners. It verifies immutable
state identity, complete main-seed chains, bounded ordered reactions, eligible reward recipients and
matching construction facts. It does not impose a source draw count on replacement algorithms.
Malformed or throwing rules fail before construction publishes; unknown accounting still rejects at
its reached source boundary. A failed post-movement preparation retains the earlier decision and
movement delivery, stops the automatic continuation, and publishes no damage or queue consumption.

The current `SourceEnemyAi`/`AttackThenApproachAi` policies pass the selected physical rule through
estimate, discarded preflight and actual post-movement execution. Their priorities, movement,
activation/memory and thinking RNG are preserved through the selected decision seam below. Reward,
growth and outcome policy use the selected seams below. The [verification owner](../verification/battles.md#semantic-action-selection-observation)
records actual choices/presenter checks and independent source-stage RNG expectations.

### Implemented automatic decisions

[`BattleStrategyRef`](../../src/Sf2.Remake.Domain/Battles/State/BattleStrategyRef.cs) is a content key,
independent of control/faction/processing order. Content parses the key without selecting C# code.
[`SessionRules`](../../src/Sf2.Remake.Application/Runtime/SessionRules.cs) snapshots an explicit composition
into an immutable key-to-rule map; session start checks every encounter, including dead/unreached
actors and battles that programs may enter later. Unknown, duplicate and missing bindings reject
before a session publishes. Selected deployment/start admission retains the physical-only empty
spellbook/item/MOV restriction and source initialization requirement; common status, faction, order,
provenance and supported source commandset/loadout guards remain at their existing boundaries.

[`IBattleDecisionRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleDecisionRule.cs) receives the live
immutable battle, actor and the same selected physical calculator used by player/query/scene work.
The scheduler calls the resolved rule; it owns neither target scoring nor strategy-name branches.
Source `AttackThenApproachAi`/`SourceEnemyAi`, scoring/priority and standby/pursuit helpers live in
`Domain/Gameplay/Sf2`. [`Sf2ClassRules`](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2ClassRules.cs) is the
single named-class/source-ID mapping used by AI and private growth admission; AI rank remains in
`PhysicalTargetRules`. Shared decreasing-cost walk/move-string primitives remain in `BattleMovement`.

The finite `BattleAutomaticAction.Validate` authority admits only the selected actor's decision
memory/last target/activation and carried thinking draws, plus the existing tested-region clear.
It preserves definition/deployments, positions, resources, main seed, queue/round, accounting and
unrelated actors. Routes require matching endpoints, adjacent traversable steps within MOV budget,
an unoccupied destination and a live opposing physical target. Source occupancy/commandset policy
still belongs to the selected source rule. `QueueOnly` expresses the existing no-action Stay result;
it requires the unchanged battle and origin-only route, then consumes one queue entry without an
action publication. Source MOVE1 origin Stay retains ordinary movement/action completion instead.

A selected physical target receives one discarded preflight before movement. Thinking and decision
memory publish once with the route, while actual physical construction waits for movement completion.
No preflight's future main seed is retained, no completion reruns thinking, and the same physical
instance handles estimate/preflight/execution. Throwing rules or malformed decisions become typed
InvariantFailure with identity/operation; unsupported source capability stays explicit. Failure
before decision publication retains earlier actions and the current queue item; a later construction
failure preserves the committed decision/delivery. The accepted HEAL release distinction between
same-step rejection and a subsequent automatic failure is unchanged.

`RuleCompositions.AuthoredFirstLegal()` and `AuthoredLowestHp()` bind the same `authored-priority`
reference in [`decision-rule-demo.json`](../../content/authored/decision-rule-demo.json) to different
ordinary C# algorithms. FirstLegal selects the first reachable physical target by processing order;
LowestHp selects lowest current HP with that same stable tie. Both use selected physical range/target
admission, the existing movement grid/preview and the common presenter; neither copies source scoring.
These are authored demonstration policies, not fidelity profiles or added source action families.
`ForGame()=>Sf2()` is the restored default. [Verification](../verification/battles.md#replaceable-automatic-decision-observation)
owns rebuild/restart selection, actual consumer recipes and independent RNG expectations.

### Implemented progression and outcome policies

`SessionRules` retains one [`IBattleProgressionRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleProgressionRule.cs),
[`IBattleOutcomeRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleOutcomeRule.cs) and
[`IOutcomeReturnPolicy`](../../src/Sf2.Remake.Application/Runtime/Exploration/IOutcomeReturnPolicy.cs).
The progression policy computes eligible physical/counter, HEAL and Herb awards, gold/counters,
credit and growth. SF2 arithmetic lives under `Domain/Gameplay/Sf2`; action preparation no longer
calculates source award draws before consulting a replacement. Half-missing HEAL still has no reward.
The common action and AI preflight receives the same selected policy, validates its finite output
on discarded state, and never publishes preview resources or RNG.

Credit precedes EXP text; growth follows acknowledgement using the then-live seed, including HEAL
fairy work. Common validators constrain actor identity, allowed state mutations, matching facts,
admitted spell mappings and a complete main-seed chain. Source caps and numerical draw schedules
belong to source assertions. Malformed/throwing policies report `InvariantFailure` with identity and
operation. A reached credit/growth/death/outcome failure retains the current stage, prior commits,
token and queue. Program policy faults retain the accepted cursor; retry resumes without replaying
acknowledged text or credit. Existing unsupported AI stop behavior is unchanged.

The source outcome policy owns leader-loss precedence and the defeated hook. The source return
policy selects programs/anchors, living/immortal recovery, completed-flag skip, join/flag decisions
and defeat gold. `BattleOutcome` and `ProgramRunner` still publish and execute the existing stages.
One validated finish decision precedes join/flag publications; no later policy call can discard that
partially published tail. No new continuation state or generic effect interpreter is introduced.

`AuthoredProgressionOutcome()` demonstrates fixed7 eligible awards with no award draws and a
saturating10-gold defeat cost. It preserves source growth and ordinary return machinery. The two
small public v8 packages reach real victory/growth and defeat from ordinary field interaction;
their bounded admission and exclusions are in [trust](../content/profiles-and-trust.md#authored-growth-and-ordinary-outcomes).
The [native recipe](../verification/battles.md#replaceable-progression-and-outcome-observation)
uses the same packages under both factories and observes real return movement. This is authored
replaceability evidence, not new original-game or private-route evidence. `ForGame()=>Sf2()` remains
the default. The [source-story compatibility seam](../application/session-and-programs.md#source-story-policy)
has its Application owner.

### Selected module and composition design

**Accepted design:** keep the four assemblies. A gameplay module is an
ordinary set of C# source implementations under `Domain/Gameplay/Sf2` and
`Application/Gameplay/Sf2`, selected by explicit construction. New authored variants live beside
that module under `Gameplay/Authored`; they are project-authored examples, not original-fidelity
profiles. No separately distributed binary or third-party plugin API is required.

The existing assembly friendship already permits Application/Content/tests to use Domain internals.
Use narrow internal strategy interfaces under `Domain/Battles/Rules` and
`Application/Runtime/Exploration`, instantiated by the gameplay module. Engine executors call those
interfaces; only composition/default factories reference concrete SF2 implementations. Domain never
references Application, Content, Godot or reference verification. The Application module can combine
Domain rules and Application story policy using the existing inward dependency. Content decodes
validated definitions, not executable C# names or factories.

A single immutable `SessionRules` composition in Application holds the selected Domain rule bundle
and story policy. `GameSession.Start` accepts it and retains it for the session; default overloads
delegate to one explicit SF2 factory. That default is a composition decision, never a branch
inside arithmetic, dispatch, query, scheduler or view. Existing direct-start callers keep working;
Godot's root and standalone battle-view source starts use the same composition. New project-authored implementation files
and their composition factory can be compiled in the existing projects. A caller selects a factory
before start, not a package-name switch during play. There is no mutable global current profile.

Retain a diagnostic identity for the selected composition independently of the content's format/trust
profile. The authored demonstrations select a C# factory in ordinary startup composition and
rebuild; they need no new package schema or runtime selector. Authored variants must not be reported
as SF2 fidelity. A private source entry keeps its admitted default rule/resource contract; it cannot
silently select an authored fallback. Immutable action/AI content keys resolve once against the
selected bundle, with missing/duplicate bindings rejected at start, not discovered through reflection.

Do not create empty policy interfaces for future families. A new seam needs an actual consumer;
retire the replaced implementation with its final caller. Ordinary
private helper functions stay private. The source modules are independently editable/selectable;
they are not assembly-enforced isolation or a security sandbox. A fifth gameplay assembly would
force currently internal state/transition/program types into cross-assembly contracts without a
current binary-distribution need. If that need becomes real, replan and amend ADR0011's production
assembly section, ADR0017's preserved outer architecture, and ADR0019's four-assembly direction
before changing references. This selected source-module design needs no reversal of those ADRs.

```text
Godot composition -> Content admission -> definition + explicit start
                 -> selected SessionRules -> one GameSession
GameSession executors -> narrow rule contracts -> chosen C# implementations
chosen implementations -> existing numerical/state primitives
GameSession -> ordered state/scene projection -> Godot -> completion token
```

### Implementable API and transaction boundary

HEAL, finite actions, AI, progression/outcome and the linked source-story seam are implemented. Reuse the existing state, reference,
command, resolution, wait and failure types; do not add copies whose only purpose is forwarding.

| Seam | Inputs/result and owner | Required behavior |
| --- | --- | --- |
| HEAL rule | Internal `IHealingRule`: spell admission, target query/check and preparation over `EngineBattleState`, `ActorRef`, provisional `MapPosition`, `SpellRef`, optional target; preparation returns the existing `BattleActionResolution`. | The same selected implementation owns query and confirm legality/formula. No RNG in queries. The default implementation is `Sf2HealingRule`; shared scalar/target helpers have one owner. |
| Action selection (implemented) | `BattleSelection.ActionReference` and `SelectBattleAction` carry a typed action reference and immutable option/target data. `GameSession.QueryBattleChoices` returns choices for its current session/revision/actor/stage/origin, selection step, enabled status and `SessionFailure` reason. | Options carry stable action/spell/item identities, display labels, supported presentation kind and ordered candidate `ActorRef`s/range. Preserve `SelectSpell`/`SelectItem` as semantic inputs while callers migrate. Query is disposable; `CommandEnvelope` and confirm remain authoritative. |
| Action preparation/application | Selected action implementation returns existing prepared/reaction/reward structure; engine preparation validates it before `Begin`. Existing scene continuation and publication apply it. | Validate actor/target references, destination, supported reaction/consumer, HP/MP bounds and permitted field changes. Reject unrelated actor/queue/definition/story changes and malformed effects before publication. Do not execute string observations or accept arbitrary callbacks that mutate the session. |
| AI (implemented) | Internal `IBattleDecisionRule` consumes the immutable battle/actor plus selected action rules; returns the existing `BattleAutomaticAction` shape with candidate state, path and target. | Stable tie order and separate RNG streams; no hidden strategy state. Memory/last target/activation stay in actor/region state. Scheduler owns turn/wait/publication, not strategy selection or target scoring. |
| Progress/outcome (implemented) | Selected progression functions use current actor, award, copied seed and existing effects; selected outcome policy uses current battle/story plus admitted outcome content. | Keep credit, later growth, outcome check and story return as separate calls at their existing commit seams. Policies choose rules/results; generic runner executes the continuation. |
| Source story operation (implemented) | Internal `ISourceStoryPolicy` takes the current definition/snapshot, `SourceStoryInstruction` and `ProgramLocation`, returning only a finite entity-retirement candidate or a `BattleRuleException`. | Only the source-specific instruction family delegates. Generic branch/call/return/wait does not ask a rule-name registry. Missing support fails at the exact PC; policy cannot advance/publish the facade or supply arbitrary active/story state. |

An enabled query option means eligible for preparation under the known prerequisites, not a promise
that every reached random-dependent branch is supported. Query and confirm share those prerequisite
checks; confirm additionally constructs and validates the action. For example, a reached growth branch
may still reject during preparation without publishing its speculative draws. Keep that diagnostic
distinct from a stale or illegal selection. Queries must not run the whole scene, consume RNG, or
cache a prepared result whose seed could later be mistaken for live state.

Action preparation uses the existing typed result, not a universal effect algebra. Its result needs
structural validation because an injected implementation can return an impossible transition even
when trusted project code is not malicious. Validate allowed deltas at this real authority boundary;
do not copy whole state into a new editable request object or add a separate validation service.
For HEAL, prepared deltas are selected-actor movement and construction main seed only; reactions may
change the selected living target's bounded HP, spell cost the selected actor's MP, and reward the
selected actor's progress at the declared later phase. Preserve other actors, thinking seed, queue,
accounting, definition identity and selection ownership. The action slice extends this finite validation to
physical/item construction's existing inventory/gold/death obligations, not arbitrary property writes.

At every call, rules receive immutable definitions/state and local RNG values. Use `BattleRandom`
for deterministic draws; only the rule determines which draw is required and its stable iteration
order. Preserve existing ushort/byte widths, signed comparisons, truncation, saturation and explicit
unchecked source arithmetic. No wall clock, host RNG, unordered collection iteration or persistent
rule-object state may influence results. The local preflight result is discarded. Later growth/AI
must consume the then-current seed at the owning boundary; storing a preview's future seed is wrong.

Failures before preparation publishes preserve state, provisional selection, RNG and observations.
Stale session/revision/actor input rejects before rule invocation. Selection changes may increment
revision but never spend resources or random draws. At a later scene/program boundary, failure keeps
earlier accepted commits and the live wait/cursor/queue; it does not rewind already displayed effects
or rerun a spent award. Map expected rule errors to the existing IllegalCommand/UnsupportedCapability
categories; an invalid implementation result or unexpected rule exception becomes InvariantFailure
with rule identity, operation and existing field/PC context. Catch only around the selected rule call,
not the entire host, and preserve the last committed snapshot. Do not convert failure into Stay or
successful completion. Direct adapter errors remain AdapterError.

Content admission owns schema, references, provenance and known unsupported definition shapes.
Rule admission owns learned action, faction/target/range/resources and accepted class policy.
Application joins that result with implemented reaction/scene capability before starting an effect.
For example, HEAL4 cannot become supported by deleting `healing-animation`; the query must expose
Unsupported and confirm must enforce the same boundary. Private missing cast resources never use
the authored gesture fallback. Actual delivery failure holds the pending token and exposes an adapter
failure. Godot consumes options/targets/semantic inventory and scene requests; it may format labels,
cycle options, and select an existing presenter by effect kind, but cannot decode source inventory
bits, choose factions/ranges, calculate effects, or branch on strategy/module names. That is the #438
interface handoff; it specifies no layout, artwork or new input-device requirement.

### Rule author workflow and diagnostics

Start from the implemented contract and an existing example in the four production assemblies:

| Author's change | Contract and example | Selection |
| --- | --- | --- |
| HEAL eligibility, targets, recovery or award preparation | [`IHealingRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IHealingRule.cs), `Gameplay/Authored/AuthoredHealingRules.cs` | Pass the implementation as the healing argument in `SessionRules`; the finite action adapter uses that same object. |
| Physical, item or Stay offers/targets/preparation | [`IBattleActionRule`/`IPhysicalActionRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleActionRule.cs), `Gameplay/Sf2/Sf2PhysicalAction.cs`, `Sf2ItemAction.cs`, `Sf2StayAction.cs` | Bind the corresponding constructor argument. Keep query, confirm and physical estimate/preflight on the selected implementation. |
| Automatic priority/decision | [`IBattleDecisionRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleDecisionRule.cs), `Gameplay/Authored/AuthoredBattleDecisionRules.cs` | Bind the content's `BattleStrategyRef` once in the explicit factory's decision list; unknown/duplicate bindings reject at start. |
| Credit, counters, gold or later growth | [`IBattleProgressionRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleProgressionRule.cs), `Gameplay/Authored/AuthoredBattleProgressionRule.cs` | Pass `progression:`; preserve the separate credit and live post-scene growth calls. |
| Victory/defeat/hook and recovery/return choice | [`IBattleOutcomeRule`](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleOutcomeRule.cs), [`IOutcomeReturnPolicy`](../../src/Sf2.Remake.Application/Runtime/Exploration/IOutcomeReturnPolicy.cs), `Gameplay/Authored/AuthoredOutcomeReturnPolicy.cs` | Pass `outcome:` and/or `outcomeReturn:`; common programs, scene/queue and staged publication remain the consumers. |
| Story branch, call, flag, text and wait | [External typed-program authoring](../application/session-and-programs.md#authoring-conditional-programs) | Change admitted JSON and restart. Both demo files run through `AuthoredStory()` without changing runner or view. |
| Source-only retired scratch compatibility | [`ISourceStoryPolicy`](../../src/Sf2.Remake.Application/Runtime/Exploration/ISourceStoryPolicy.cs), `Gameplay/Sf2/Sf2StoryPolicy.cs` | `sourceStory:` selects the named policy. `AuthoredStoryPolicy` rejects this family; it is not a custom-opcode extension language. |

Implement a C# rule under the existing `Domain/Gameplay/Authored` or `Application/Gameplay/Authored`
module as appropriate. Reuse numerical/state primitives and an admitted reaction/presenter family.
The rule receives immutable state and returns only the contract's finite candidate. It must not keep
future state/RNG in object fields or mutate the session. Ordinary integer widths, deterministic
ordering and the separate seed channels still matter. Unsupported effects require a separately
scoped capability change; deleting a capability guard or returning a source-name string cannot add one.

Add an explicit factory in [`RuleCompositions`](../../src/Sf2.Remake.Application/Gameplay/RuleCompositions.cs).
For example, a new HEAL implementation can use
`new("my-game", new MyHealingRule(), SourcePhysical(), SourceItem(), SourceStay(), sourceStory: new Authored.AuthoredStoryPolicy())`.
Other optional constructor arguments select the seams above. Their defaults remain SF2; choose each
intended variant explicitly. A project author changes `ForGame()` to that factory for the ordinary
host, or passes it to `GameSession.Start(source, rules)` in a behavior test. The same composition is
retained throughout the session. Content's `ruleProfile`, package IDs and strategy keys do not discover
or instantiate C# classes. There is no runtime environment switch or mutable policy setter.

Write small engine assertions for changed valid actors/states/content, not just the demonstration
endpoint: query/confirmation agreement, allowed candidates, exact RNG/commit boundaries, malformed or
throwing selected rules, and stale/duplicate completion. Run the affected engine tests and adapter
build with the [protected locked workflow](../development-and-verification.md#locked-net-workflow).
Build the existing Godot project in Debug, then restart the ordinary host with its admitted input.
Rebuilding does not replace a running session's rules; JSON is read/admitted once as well, so content
edits also need a fresh start. Use the existing short observer for the affected presenter/flow.
Restore the intended default factory and rebuild before freezing a source-default candidate.
No new assembly, language, installation, view setter or probe case is needed to choose a factory.

Diagnose at the boundary that owns the failure:

| Result | Meaning and next inspection |
| --- | --- |
| ContentError before start | Inspect package shape, all references, provenance/profile and start input. A rule cannot repair rejected Content. |
| IllegalCommand | Inspect the current envelope session/revision/actor, selected action/destination/target and enabled/reason query. A previous query is not authority to confirm. |
| UnsupportedCapability | Inspect the precise rule or consumer capability: source accounting, admitted spell/item/status, source story context or missing private scene binding. Keep the guard and unmet boundary explicit. |
| InvariantFailure with rule identity/operation | Inspect the selected implementation and finite result at the named commit seam; source-story diagnostics also name the exact program/PC. Earlier committed effects, live token and caller context remain. Exception details are not published. |
| AdapterError | Inspect actual projection/resource/delivery and adapter errors. A pending completion token remains pending; do not report successful delivery to unblock it. |

## Battle State and Numerical Rules

The engine implements internal Domain RNG, ordinary priest healing arithmetic, turn-order generation
and Manhattan action range, with a dedicated engine unit project and scoped verification entries.
`Gameplay/Sf2/PhysicalStrikeRules`, `BattleRewards` and `Sf2PhysicalAction` own ordinary physical construction,
settlement and atomic state transition. `Sf2PhysicalAction` constructs at most three source-ordered
hits on temporary HP, carries sticky reaction decisions and aggregates one ally award before publication.
Both player commands and `EnemyPhysicalDecision` use the same session-selected physical calculator; `BattleActionCommitter` is
the single publication/queue-consumption mechanism. Automatic advancement catches failures at each
enemy ACTION, preserving earlier commits and retaining the failed enemy's queue entry. The semantic
attack-then-approach strategy scores all reachable physical targets in reverse processing order and uses
shared `PhysicalTargetRules` for signed raw-priority cohorts, class selection and movement ties.
The existing class definition supplies source identity only for admitted named classes; missing
identity rejects a reached critical comparison. Regular movement fixes the class table; content
cannot supply an independent rank. Source thinking, scoring and selection have one production owner.
`AttackThenApproachAi` sequences that decision and its zero-target continuation: unavailable
HEAL1/SUPPORT fail, then MOVE1 succeeds with movement or origin Stay. `Gameplay/Sf2/AiMovementRules` owns source stable cost and radius station selection;
shared `BattleMovement.Walk`/`MoveString` retain the decreasing-cost route primitives used by movement
and source pursuit/standby. The existing movement commit and action publisher apply
the chosen destination once. MOVE1 draws no RNG and leaves last-target memory and resources unchanged;
physical rewards are required only on the reached attack branch. High or incomplete target costs
remain Unsupported, as does wider AI. Private region activation retains the bounded [private-entry contract](../content/profiles-and-trust.md#private-initialized-common-battle).
Shared strike/reward functions remain the calculation owners. Semantic observations carry both actor and target for reversal. Dead combatants retain
identity/HP/kill-and-defeat accounting but have no
battlefield position; occupancy and presentation read that authoritative state.
The [current M1 boundary](../../../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m1-implementation)
adds movement/cancellation, common session/content admission and connected authored battles.
Profile-specific snapshots, fixed import checks, endpoint handlers and old Godot battle dispatch
were retired with the reference runtime at
[M5](../../../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m5-implementation);
The [historical audits](../evidence/retained-comparisons.md#historical-implementation-and-audits) retain their exact old claims. Current responsibility and capability owners govern further work. Public/private trust and the accepted composed 8D/H4
milestone remain distinct from the unsupported broader capabilities in this migration.

Encounter deployments own explicit `BattleFaction` and unique integer `ProcessingOrder`; intrinsic
actor definitions and session starts do not duplicate those roles. The definition sorts deployments
once for stable round RNG, AI candidates and adapter selection. Runtime actors derive faction/order
from their deployment; queue entries identify actors by `ActorRef` with a nullable sentinel. Faction drives
healing/opposition/rewards independently of order. Deployments also own `BattleControl` separately from
`BattleStrategyRef`; intrinsic actor definitions carry neither assignment. Player requires no strategy;
automatic actors select an immutable content key resolved in `SessionRules` before start. Private source orders remain
SourceOrders and use the bounded source standby/activation/set6/set7 continuation. Shared deployment validation at Content
admission and reusable start preserves ally/player and enemy/automatic bounds plus physical/spellbook/
MOV requirements. All encounters, including unreached/dead deployments, bind against the selected composition. The
advancer invokes the resolved decision without policy-name switches or a missing-binding Stay fallback. The native view projects each actor’s current AI memory and immutable source anchor; there is no second control copy.
Actor definitions separately own numerical `Agility` (0–127) and boolean `ExtraRoundAction`.
Live-start capacity and shared generation consume explicit eligibility; only private Content decodes
the raw source byte's low seven bits and high-bit eligibility. The ordinary three draws and optional two draws at the
truncated five-sixths basis retain source arithmetic and signed sentinel ordering. The existing
advancer consumes both entries without an additional scheduler or physical double/counter changes.

`PhysicalCriticalRule` owns the two supported immutable chance/bonus definitions; Content selects
one from explicit semantic fields rather than a packed prowess number. Every actual hit, including
reversed counters, reads its attacker's selected rule. The scalar strike calculator already takes
mathematical operands and remains unchanged, as do its private source mappings. Other
physical fields retain their existing semantic ownership; no generalized profile system is introduced.

`BattleTerrain` holds immutable surface and independent protection; layout glyphs resolve through
explicit local definitions. `BattleTerrainRules` interprets them for the currently admitted
regular/healer/Centaur/hovering movers and target land reduction. Content/start, preview/commit, AI pursuit/scoring
and physical hits use that owner. Temporary opponent blocking changes only a fresh per-cell cost
array; friendly traversal and occupied stopping remain movement policy. Godot projects surface into
existing placeholder colors without supplying gameplay properties.

`WeightedMovement` consumes decoded signed cell costs, preserving its one first-admission/LIFO bucket
algorithm with logical row boundaries; the original flat-storage probe across a row seam is not
reproduced. Private Content maps the selected raw terrain to these semantic surfaces while retaining
the complete source encounter for later consumers.

## Ordinary Medical Herb battle action

The common battle session accepts `SelectItem(slot)`, `SelectTarget(actor)` and `Confirm`
from ordinary action choice. `Cancel` returns to provisional movement. Item reselection
clears the previous target. Invalid slots, empty inventory, unsupported item effects,
dead/opposing/out-of-range targets and failed reward/growth admission publish no partial
movement, HP, inventory or RNG change. Reusing an old command envelope cannot consume twice.
The shared target validator measures from the provisional destination, including self-targets.

`BattleActorState.SourceLoadout` is the live ordered four-slot loadout. Definition loadouts
supply initial content only; explicit `BattleActorStartInput.SourceLoadout` carries later
state. Growth retains that live inventory while changing learned spells, and `BattleOutcome`
passes it into the exploration party on both victory and defeat. Resource resets heal HP/MP
without replenishing items. `item-consumed` records the original item word in `Before` and
the chosen zero-based slot in `After`; the resulting snapshot exposes the arranged inventory.
HP recovery now executes through the [Medical Herb scene](../godot/battle-scenes.md#medical-herb-scenes),
after construction has consumed inventory. EXP/growth and turn release wait for their scene commands.
Application owns single-side target/actor switching; SFX113 and the source NONE/Nothing selection do
not imply fairy or physical recoil draws. There is no separate inventory service or field inventory UI.

### Original source and effect boundary

**Confirmed static:** the pinned SF2DISASM revision
`c834c652b6862bc5679fd7f69a38a7093206efc6` supplies these dependencies beneath `disasm/`:

| Source / symbol | Consumed rule |
| --- | --- |
| `data/stats/items/itemdefs.asm`, item 0 at `table_ItemDefinitions` (`0x16EA6`) | Medical Herb is CONSUMABLE, with no equip effects; use spell HEALIN-1 and item range 0–1. |
| `data/stats/spells/spelldefs.asm`, HEALIN-1 | Base ID 16, MP cost 0, teammate healing, range 0–1, radius 0, power 10. |
| `code/gameflow/battle/battleactions/useitem.asm`, `battlesceneScript_UseItem` (`0xBBB8`) | Select the held item definition, unpack use spell and delegate to its spell effect. |
| `castspell.asm`, `spellEffect_Heal`; `calculatespelldamage.asm`, `AdjustSpellPower` (same battleactions directory) | Recovery is `min(power, missing HP)`. Item actions skip the spell-only promotion multiplier. HEALIN recovery has no variance draw. |
| `earnexp.asm`, `battlesceneScript_CalculateHealingExp`; `giveexpandgold.asm`, `battlesceneScript_GiveExpAndGold` | Ally healer classes contribute `min(25, max(10, floor(25 * recovered / maxHp)))`; other classes contribute zero. Same-side actions skip battle halving. Two range-16 draws add/subtract one on zero, with final minimum one. |
| `breakuseditem.asm`, `battlesceneScript_BreakUsedItem` (`0xBBE6`); `code/common/stats/itemstats.asm`, `RemoveItemBySlot` / `RemoveAndArrangeItems` | Non-equipment consumes without a break roll, shifts subsequent full item words down one slot and appends NOTHING (127). Identity lookup masks the low seven bits; retained words preserve flags. |

The [item-definition](../../../docs/design/contracts/item-definition-data.md),
[spell-definition](../../../docs/design/contracts/spell-definition-data.md),
[spell-resolution](../../../docs/design/contracts/spell-resolution.md) and
[action-construction](../../../docs/design/contracts/battle-action-construction.md) contracts
own the accepted boundaries. **Confirmed native:** the
[original herb observations](../../../docs/research/map3-messenger-acceptance.md#native-herb-observation-and-completed-branch-recovery)
and winning continuation record the reached item/slot/HEALIN/consumption family. Their bounded
HP facts include 6→11, 9→12, 3→12 and 4→11; these are missing-HP clamps, not four different
item powers. `MedicalHerbTests` checks those minimal facts separately from controlled RNG cases.
It does not relabel authored tests as an original trace replay.

Private Content admits only the selected Medical Herb definition into this item family.
Other carried item IDs remain visible but reject as `item-effect`; equipped weapons are
not herbs. The current class model admits PRST healing EXP and the existing non-healer
classes. VICR/MMNK class admission, broader item families, Equip/Give/Drop, field inventory,
AI healing-item choice and save/load remain unsupported. Exact original presentation timing remains Unknown. The existing
physical-only AI policy rejects a nonempty item loadout at start. This does not close the
continuous H4 milestone or establish successful native-original cancellation.
