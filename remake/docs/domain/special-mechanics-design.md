# Special Mechanics: Capability Gaps and Staged Extensions

Status: **Proposed engineering design** for the remaining families; selected Battle02 EGRESS/Angel Wing,
Taros physical-effectiveness and authored physical item-drop implementations are tracked by the
[current agreements](../capability-status.md#taros-physical-effectiveness-implementation-agreement).
This document is not blanket implementation authorization or an accepted fidelity profile. [Issue #1339](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1339) owns scheduling;
this document owns the proposed boundaries and decisions. Original facts remain with the linked
English research/contracts/synthesis owners. No original-game evidence is derived from remake code.

## Agreement and reading boundary

**Standard rigor:** six bounded representative families have accepted sources and a concrete current
engine. The intended result is an implementable dependency order and a first player-facing slice,
not an exhaustive full-game gap census. Raise the selected implementation to High when it crosses
party, scheduling and program/persistence owners; that does not authorize all those systems at once.

Scope is Taros, Jaro, Prism Flowers, Fairy Woods, special exits/callers, and raft/Caravan travel, plus
a dependency table for adjacent spells, equipment, economy, services and persistence. Each proposal
must identify Content admission, Domain rules/state, Application publication/waits/failure prefixes,
Godot projection and changed-state counterexamples. Existing finite seams take priority over a new
abstraction. Production, tests, research/contracts, translation indexes and UI/UX are read-only in
this task. Only this document, architecture navigation and relevant capability corrections change.

Acceptance is a complete source/consumer gap map, concrete extension sketches, smaller alternatives,
explicit product decisions and one end-to-end recommendation. This design neither imports later
chapters nor runs acquisition, runtime/full suites, builds, saves, new comparators or implementation
tasks. Direct document/scope checks, the clean-head dependency plan and actual scoped CI validate
this diff. Independent main-gate review precedes any implementation dispatch.

The inspected accepted baseline is `e97bde217537885663e795511688186a642afb3b`. Paths below refer to
that baseline unless this document is deliberately revised with a later accepted implementation.
**Confirmed** original claims below are static at their named owners, unless explicitly described
as runtime observations. **Inferred** means a reasoned engineering consequence, not a new source
fact. **Unknown** includes natural reach, original timing and persistence where the owner leaves
them open; none automatically starts an observation queue. Proposed API names are sketches, not
existing types. Existing accepted modern timing/input choices remain in force.

## Current support and the real gaps

The [capability ledger](../capability-status.md) distinguishes implementation from milestone parity.
The following static code reads and accepted implementation owners control over old local
"integration pending" or "reached level-up unsupported" wording. No tests were rerun for this design.

| Capability | Current evidence and classification |
| --- | --- |
| Physical, HEAL, Herb, Stay; growth and ordinary victory/defeat | **Supported within admitted content.** [Action/progression seams](gameplay-rules.md#implemented-progression-and-outcome-policies), `BattleGrowthTests`, `Battle02OutcomeTests` and the [Battle02 return owner](../application/session-and-programs.md#battle02-outcomes-and-live-facing) include real growth, live party accounting, defeat cost and retry. Full-game item/effect coverage does not follow. |
| Flying and physical sleep | **Supported bounded rules and delivery.** [Sleep/Flying tests](../../tests/Sf2.Remake.Engine.Tests/SleepAndFlyingTests.cs) and the [Yeel/Battle03 owner](../application/session-and-programs.md#yeel-guide-zone-request-and-sleep-continuation) cover movement, sleep construction, skip and recovery. SLEEP spell, Stun and other statuses are separate gaps. |
| Flags, conditional/call programs, followers, region activation and typed waits | **Supported primitives**, not missing infrastructure. [Program runner](../../src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [party flags](../../src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs), [activation](../../src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs). The [authored Jaro composition](../capability-status.md#authored-jaro-conversion-agreement) consumes a finite battle-region scene and two-actor conversion; the [authored hidden-enemy lifecycle](../capability-status.md#hidden-enemy-lifecycle-agreement) consumes modes2/3. |
| Later ordinary map/dialogue bindings using only admitted operations | **Content/delivery gap only for those operations.** Reuse typed programs, selected maps and private preparation; do not infer an entire later encounter is content-only. Private encounter pins still admit the selected 1/2/3 boundary. |
| Taros | **Authored physical-effectiveness rule/content/feedback implemented** at the [bounded owner](gameplay-rules.md#finite-taros-physical-effectiveness). Authored [leader cleanup](gameplay-rules.md#finite-enemy-leader-cleanup) is implemented through explicit source-slot binding and the existing outcome return. Authored physical drops, direct receipt/rare Deals fallback and session custody are implemented at the [finite drop owner](gameplay-rules.md#finite-physical-action-item-drops). Wider private encounter/aftermath remains a gap. The outcome reader still rejects unbound enemy leaders, private leader encounters and nonempty defeated programs. |
| Jaro | **Authored finite composition implemented.** Region programs share StoryState and ProgramRunner; two logical actors retain resources while a visual alias is copied. Explicit source control consumes live bit2 and physical-only ally commandset6. Private Battle32 and victory recruitment remain unbound. |
| Prism Flowers | **Bounded authored implementation.** The [finite laser owner](gameplay-rules.md#finite-prism-laser) implements chosen automatic action, live source-ordered hits, known-no-ally accounting and real authored delivery. Private Battle36, non15 activation, special/edge/debug inputs and original audio/visual timing remain gaps. |
| Fairy Woods | **Authored spawn lifecycle implemented; private trial state/content gap.** Modes2/3 use finite dormant state, source integer regions and staged round appearances. Private Battle44, trial counter/snapshot/record lifecycle remain unbound. |
| EGRESS / Angel Wing / Battle0 caller | **Selected Battle02 retreat implemented; wider caller gap**, not missing ordinary defeat. [State types](../../src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs) now admit the finite Retreat action/result alongside ordinary actions and Victory/Defeat; [return policy](../../src/Sf2.Remake.Application/Runtime/Exploration/IOutcomeReturnPolicy.cs) requires a field anchor. No session-end destination or suspend gameplay exists. |
| Raft / Caravan | **Standalone authored Caravan implemented; raft/full-map travel unsupported.** The [session owner](../application/session-and-programs.md#caravan-boarding-travel-and-landing) covers finite mode1 control and typed events2/4 with live entities and current lists. Acquisition/services/persistence remain separate. |

Accepted merge evidence for the related ledger corrections is
[ordinary policy composition](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/f08bcf9f),
[Battle02 outcomes](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/126644e7),
[Battle03 Flying/sleep and guide](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/fe8ac968)
and [battlefield maps](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/13cce997).
These establish merged implementation, not new original fidelity or human aesthetic acceptance.
Preserve #1047's twelve completed unresolved comparisons: nine old-start full-fade-state cases,
selected seed, first actor and activation rejection. They remain failures at their old boundaries.

## Shared constraints on the proposals

Keep the four assemblies and `GameSession` as the sole snapshot publisher. Immutable Content
describes capabilities and source mappings; Domain computes finite candidates; Application owns
publication, continuations and actual waits. Godot receives semantic projections and returns
matching completions. A view cannot decide faction, damage, spawning, payment or a best record.

Use `SessionRules`' action, decision, progression, outcome and story policies where their authority
fits. Extend a finite result at its real consumer instead of emitting arbitrary state patches,
string-dispatched hooks, a universal ability language or a new event bus. Content validates selected
definition references/resources before play; live legality is rechecked on confirmation. Queries
must not consume RNG or retain speculative future seeds. Keep source-ID mappings at Content/SF2
policy boundaries, not in general dispatch or Godot.

Preparation failure preserves the pre-publication state. A failure after payment, flag write,
movement or a completed reaction retains that prefix and its existing cursor/token; it does not
roll back or rerun the action. Stale completion cannot spend twice or release a different wait.
Expected illegal/unsupported rules and adapter/invariant faults retain their existing categories.
The following sequences name logical commit points, not hardware timing or instructions to add
an independently mutable controller.

## Taros: effectiveness, cleanup, loot and acquisition

The [implementation agreement](../capability-status.md#taros-physical-effectiveness-implementation-agreement)
and [rule owner](gameplay-rules.md#finite-taros-physical-effectiveness) now implement the smaller
authored physical gate, followed by the bounded [leader-cleanup agreement](../capability-status.md#leader-cleanup-implementation-agreement). The table below retains the wider staged design; loot and
chapter binding are still proposed. Source dodge/critical/follow-up/end-EXP ordering and later sleep
are preserved at the implementation owner; ineffective is not a zero-total-RNG or zero-end-EXP action.

**Confirmed static sources:** [initial gate and its lifetime](../../../docs/research/battle18-initialization-and-first-control.md#bowies-equipped-achilles-gate-and-its-lifetime),
[action-drop custody](../../../docs/research/battle18-initialization-and-first-control.md#taros-sword-action-drop-custody),
and [outcome/acquisition](../../../docs/design/synthesis/battle18-outcomes-and-caravan-acquisition.md).
The [tactical explanation](../../../docs/design/synthesis/tactical-battle-loop.md#equipment-can-gate-an-effect-taros)
joins them without making a universal immunity claim.

The effectiveness conjunction is initial construction in Battle18, actor Bowie, ATTACK, target
enemy definition Taros, actually equipped Achilles. The gate is not possession, battle admission,
an enemy slot test or a per-target recalculation. The source damage helper can return ineffective
before HP/reward work. Doubles retain the initial toggle; counter setup clears it without calling
the gate again. Ordinary Taros-origin attacks have their separate counter prohibition.

| Layer | Proposed smallest extension and owning seam |
| --- | --- |
| Content | Admit a finite SF2 encounter effectiveness binding with actor identity, target enemy-definition identity and weapon identity. Keep target definition matching distinct from drop-table combatant matching. Bind leader objective, defeated program, optional drop row and first/repeat aftermath separately; deleting reader guards without consumers is insufficient. |
| Domain | Extend `Sf2PhysicalAction`/`PhysicalStrikeRules` construction with an action-local effectiveness operand; use live `SourceLoadout` equipped bits, not immutable starting equipment. Its lifetime belongs to the prepared script/reactions, not a persistent actor immunity flag. Add an explicit ineffective result consumed by the scene. Later spell/item implementations must respect their own reached source helper, not inherit a global immunity predicate. |
| Application | At existing post-action defeated-hook/death-worklist boundary, execute admitted leader cleanup before recount/outcome; preserve defeat-first priority and do not call the hook a second time after after-turn effects. Reuse ordered death delivery. Model action drops at their construction commit boundary separately from sweep cleanup and after-program execution. |
| Godot | Show ineffective feedback, ordered deaths and successful receipt/Deals feedback from committed facts. Existing scene/field presenters can be extended with finite requests. Caravan visibility/acquisition is a later field projection, not a signal to board. |

Sketch at the existing source policy, **not a new replacement interface**:

```csharp
TarosEffectiveness PrepareInitialEffectiveness(BattleDefinition encounter,
    BattleActorState actor, BattleActionRef action, BattleActorState initialTarget);
// Prepared action owns the value; doubles retain it, counter construction follows its own rule.
LeaderCleanupPlan PrepareDefeatedCleanup(EngineBattleState current);
// Finite ordered ActorRefs; actual killed-list kill bookkeeping, no sweep damage EXP/gold/drop.
```

The implemented [finite physical drop](gameplay-rules.md#finite-physical-action-item-drops) uses
live packed enemy/recipient inventories and immutable session drop-once/Deals custody. Once-bits
remain distinct from story flags. Source order is qualifying action death/held item → range32 draw
for Taros Sword110, Iron Ball102 or Counter Sword68 only → once-bit test
and set → remove enemy slot → living recipient receipt or rare Deals fallback. An already-held bit
does not skip the preceding draw. Prepare these no-wait helper operations as one finite candidate
with ordered facts; do not add a transaction runner between each write. Existing construction
publication/cursor prevents later delivery from repeating the draw, removal or receipt. A full
inventory sends rare items to Deals and discards non-rare items, without another ally search.
RARE metadata is independent of the three chance identities. Only direct receipt emits the source
dropped/received message pair; fallback is silent. Return/reentry carries live party inventory and
once/Deals custody while fresh enemies regain their initial items. The authored implementation does
not establish private Battle18 or natural acquisition. A leader sweep awards no action drops.

First victory's after-program can use existing flag/conditional machinery for Caravan acquisition
and Rohde-following removal once its actual scene operations are admitted. Repeat victory skips
the scene and does not repair acquisition; defeat/retreat do not acquire it. These are content
bindings after rule/scene support, not a reason to build transport before the combat gate.

Counterexamples for implementation: carried-but-unequipped Achilles; another actor with Achilles;
different target definition in the same slot; changed battle; second/counter construction; both
leader and enemy dead; Taros dead without an action drop; full/dead recipient; held once-bit with
the same draw; first versus repeat completion. Original natural trajectories remain **Unknown**.
Effectiveness, leader cleanup and finite physical receipt/custody now have authored implementations.
Complete Taros chapter support still requires its private encounter/after-program bindings and
independent acceptance; no generic damage-hook platform is proposed.

## Jaro: region programs and two logical actors sharing a visual

**Confirmed static sources:** [region wrapper](../../../docs/research/battle32-activation-jaro-and-first-turn-dispatch.md#region-hook-admission-and-complete-jaro-scene),
[conversion and aliasing](../../../docs/research/battle32-activation-jaro-and-first-turn-dispatch.md#logical-conversion-membership-and-physical-aliases),
[retained enemy slot](../../../docs/research/battle32-initialization-and-opening.md#jaro-joined-list-empty-list-and-skipped-old-slot)
and [victory membership](../../../docs/research/battle32-outcomes-and-moun-entry.md#physical-population-aliases-and-logical-jaro-state).

| State | Owner and required distinction |
| --- | --- |
| Enemy143 and ally23 identity/resources | Keep separate `ActorRef`s. Conversion removes enemy HP/placement and transfers coordinates to ally23; ally HP, stats, loadout and status remain its own. Do not implement this source case as an enemy object changing its faction with copied vitals. |
| Joined/active membership | Existing story flags remain authority; existing `MapPartyMembership` produces lists. The source can expose a prefix whose list was rebuilt before a later active-flag write; rebuild only at the named operation if preserving that behavior. Explicit Jaro activation can exceed twelve; do not silently clamp/evict a member. |
| Control and strategy | Faction stays with the two deployments for this case. For admitted SF2 actors derive effective control from the existing live activation AI bit at the source control seam; do not add a second mutable control flag. Authored deployment control retains its current meaning. Retain immutable strategy binding; admit ally automatic only with a supported ally decision rule. |
| Physical alias | Reuse field logical-to-physical lookup concepts for a battle visual binding. Two logical actors may refer to one visual slot; presentation ownership is not actor identity. Copy the alias without allocating/healing a new actor; do not serialize Godot node IDs as gameplay data. |
| Queue | Region conversion precedes round generation. Generate from the resulting live ally placement/HP/agility; do not substitute an enemy queue slot or grant an immediate manual turn. Existing queued-dead handling remains distinct from future control changes. |

Implemented finite sequence in the existing round continuation:

```text
activation -> first eligible region row -> commit played flag F386
 -> run typed program, retaining ActiveBattle and PC/callers/waits
 -> conversion writes / membership / alias -> later joinForceAI operation
 -> spawning phase -> generate queue from live state -> eligible actor control
```

`BattleRoundContinuation` now retains a Program stage before reset/appearance and queue generation.
`ProgramRunner` retains `ActiveBattle` and shared PC/callers/waits; GameSession and BattleAdvancer
preserve accepted story writes instead of restoring incoming StoryState. A whitelist admits only
battle-safe typed operations; unsupported field operations keep the reached prefix.

Content declares ordered region/program/played-flag rows, explicit source-slot actor bindings and
retained visual aliases. `ConvertBattleActor` and `JoinBattleForceAi` preserve the source-ordered
membership/list/activation writes. `SourceAllyControl` reads live bit2 for the explicitly admitted
ally deployment. `SourceAllyAi` implements finite empty-loadout commandset6, with ally-specific target
selection and no enemy preprocessing/LastTarget write. The [implementation owner](gameplay-rules.md#jaro-conversion-and-source-ally-commandset6)
records pinned source clarification and Unsupported boundaries. Godot consumes current bindings and
typed text/camera waits without making nodes or friendly appearance authoritative.

Dead ally23 must stay dead and unscheduled. Already-joined23 can leave143 uninitialized by the
fresh initializer; the source scene still reads retained143 without a membership guard. A modern
explicit start must either supply the retained slot/alias or reject that unsupported input, not
invent fresh143. Malformed physical lookup has no accepted safe emulation contract: recommend
explicit Unsupported at the reached operation, preserving F386, rather than inventing a sprite.
An interrupted scene after F386 does not rerun from its start; same-session continuation resumes
its PC, while a new wrapper invocation sees the held flag and skips. Death, full active party,
both regions true, absent region, already joined and each interruption prefix are required cases.

First victory recruitment and AI-bit clear/active removal are separate story operations; a region
conversion is not required for that scene, and repeat victory may skip it. The smaller alternative
is ordinary joined/follower content with no battle conversion. That is useful later chapter content
but fails this mechanism's result. Two actors plus a finite round continuation avoids a general
faction-changing entity system and reduces alias/debug/recovery complexity. Preserve the source
capacity exception in this named operation; a new universal party-cap policy is a product decision.

## Prism Flowers: decision ray versus construction hit list

**Confirmed static source:** [laser owner](../../../docs/research/battle36-prism-flower-laser.md),
including its [current-consumer audit](../../../docs/research/battle36-prism-flower-laser.md#actual-current-consumers-and-missing-binding),
and [dispatch admission](../../../docs/research/battle36-activation-and-first-turn-dispatch.md#first-individual-turn-control-and-the-flower-kind-branch).
Natural fired turns and completed original animation remain **Unknown**.

Content supplies a finite line-attacker strategy/action and cardinal laser direction mapped from
battle/ordinal, separately from display facing. It must admit the actual automatic loadout/mover and
required presentation resources; a LASER spell label or removal of a physical-only guard is not support.
Keep the private 48-by-48 tactical storage boundary distinct from the rendered crop.

The [authored implementation agreement](../capability-status.md#prism-laser-implementation-agreement) now implements this finite extension through `BattleAutomaticAction.ChosenAction`, `PrismLaser` and the existing scene/field consumers. The following sketch retains the wider design boundary:

```csharp
// Add ChosenAction to the existing automatic result; Target becomes focus, not the damage list.
BattleActionRef ChosenAction; // existing actions plus a bounded PrismLaser kind
LaserIntent SelectRay(EngineBattleState state, ActorRef flower, CardinalDirection direction);
LaserResolution PrepareRay(EngineBattleState live, LaserIntent intent);
```

The selected decision retains activation/control prerequisites, scans eligible living positioned
non-neutral allies on the fixed ray and selects the first focus or STAY. It does not turn, move,
scan other directions or treat walls/ordinary spell range as blockers. At construction, rebuild
occupancy using both factions, sort by the admitted source combatant order, then perform base16
and the two variance draws per ordinary valid target before clamping HP. No ATT/DEF, land,
ordinary resistance or MP charge. Use existing staged HP/reward/death and live RNG owners, with
a laser-specific finite validator: same-faction targets and zero reactions are legal here, but
not silently allowed for ordinary physical attacks. Preserve neutral and coordinate filters.

Application carries the decision through movement completion without replacing it with Physical.
At preparation it rebuilds targets from current state; target departure is not a reason to rerun AI
or turn a committed shot into STAY. An empty list still retains source message/animation requests.
Godot needs semantic ray origin/direction, focus, ordered hit/reaction list and explicit laser
message/effect delivery. Existing source message fallback273 and physical damage presentation
cannot stand in for the required laser message319/animation. Missing delivery holds the token;
later target/RNG work is not silently skipped.

Counterexamples: ally off-ray/on-ray, unchanged facing but different laser direction, enemy behind
the trigger, target death/movement between decision and construction, all targets gone, non15
inactive triggers, sleeping/stunned execution and enemy128/Executioner still alive after Flowers die.
Leader victory is the independent Taros-style cleanup dependency, not the laser's damage algorithm.
Extending physical range alone is smaller in line count but changes targeting, RNG and friendly
fire, making debugging and later spell reuse harder. A finite action plus existing decision/reaction
seams is the recommended option; no general AoE platform is needed before the laser can work.

## Fairy Woods: spawning and result state have different lifetimes

The finite authored spawn/reset/round portion below is implemented under the
[hidden-enemy agreement](../capability-status.md#hidden-enemy-lifecycle-agreement); the
[rule owner](gameplay-rules.md#hidden-enemy-reset-and-source-regions) specifies actual integer geometry
and reset semantics. Trial clock/results, private Battle44 content and original presentation remain
proposed/unsupported. The lifetime table records the accepted design boundary, not a new work queue.

**Confirmed static source:** [trial owner](../../../docs/research/battle44-fairy-woods-trial-and-records.md),
especially [spawn rules](../../../docs/research/battle44-fairy-woods-trial-and-records.md#regions-checked-bits-hidden-appearance-and-reappearance),
[timer](../../../docs/research/battle44-fairy-woods-trial-and-records.md#timer-lifetime-snapshot-record-and-save-boundary)
and [result/retry](../../../docs/research/battle44-fairy-woods-trial-and-records.md#victory-result-record-comparison-and-retry).
This is Map24/Battle44, not Fairy Cave. Bubbling Ooze's leader cleanup can terminate before every
hidden enemy appears; the invitation's wording is not an all-fifteen-kills predicate.

| Lifetime | Proposed authority and transition |
| --- | --- |
| STARTING / HIDDEN / literal3 definition | Content admits only implemented modes, source starting position and region references. Preserve source triangle/quad form and integer region predicate; current four-vertex geometric admission alone is insufficient for the trial's type3 row. |
| Dormant versus loaded definition | Add finite runtime spawn initialization state to `BattleActorState`; its effective maxHP resolves to zero while uninitialized, otherwise existing definition/progress. Do not mutate the shared actor definition or duplicate ordinary maxHP authority. Initialize this state explicitly for fresh/controlled entries. |
| Reset attempt | Domain checks source bound and all coordinate occupancy, including dead/self occupants. Success restores source stats/loadout/placement/orders and retains the passed activation word. Refusal sets max/currentHP0 and absent placement while retaining activation/held regions; it is not an exception or successful appearance. |
| Round prefix | Application runs activation → region program → ordered reset candidates → actual spawn presentation → generation. The complete reset list publishes before the first appearance; retain list/cursor/wait so resumption cannot reset twice or generate early. The finite spawn continuation and authored Jaro program stage are implemented; private/trial consumers remain unbound. |
| Fresh retry | Reinitialize encounter actors/regions separately from session best record. No immediate regeneration inside a death reaction and no persisted queue inferred from a new attempt. |

HIDDEN requires dead plus held region and maxHP0. Successful appearance followed by ordinary death
retains nonzero maximum HP, preventing another appearance in that unchanged trial. Literal3 has
no maxHP0 gate and can reappear at a later round prefix. A failed reset leaves maxHP0, so a later
eligible attempt can succeed. A monotonic "has ever spawned" bit is insufficient for all supplied
mode/reset states; the proposed initialization state represents the live source condition.

Godot projects appearing actor, camera, appearance text and actual matching completion from the
existing effect/wait pattern. It does not respawn a hidden node by local timers. Occupied start cell,
dead/self occupancy, failed-then-successful reset, HIDDEN success/death, literal3 death/next-round,
mode changes in explicit inputs, and fresh retry are the behavioral counterexamples.

The clock is a separate design choice. Source counter seconds/frame remainder, callback-copied
snapshot, visible digits, RAM best and serialized save field are different values. Source victory
uses the last timer callback copy, potentially updated during close; a strict unsigned improvement
updates RAM best, with no SaveGame in that result branch. Defeat/retreat do not record a completion.

**Proposed recommendation, requiring product acceptance:** score modern deterministic simulation
time from first usable trial control to accepted terminal resolution, freeze that result explicitly,
and pause when the existing host suspension stops simulation. Store run counter/result in the
trial session state and best in session-level story state; Godot renders their snapshots. This is
logical work time, **not real player decision speed**. Define exactly which admitted battle/program
ticks count before implementation; wall-clock waiting or hidden callbacks cannot supply missing
ticks. Start a retry with fresh run state, preserving only RAM best. No durable-best claim until
save/load has its own accepted writer/readback.

The smaller compatibility alternative reuses current logical-service opportunities to model the
source timer callback/snapshot lifecycle, including the close-wait and sentinel, without hardware
timing. It costs more lifecycle debugging but preserves source score semantics. The recommended
explicit terminal snapshot is easier to explain and test but changes scores; do not adopt it by
calling it a bug fix. If a real-time speed challenge is desired, that is a new product/clock decision,
not permission to bypass the accepted deterministic clock. Timer hidden while counter advances,
equal/slower record, no callback copy, close-wait update and unsaved RAM record must remain distinct.
Spawning can be implemented independently while this scoring choice stays Proposed.

## Special exits: payment, battle return and session end

**Implemented subset:** selected Battle02 EGRESS and Angel Wing use finite spell-MP/item-slot payments
and the shared paid retreat continuation. Wing stays an ordinary Item selection, requires self but
neither MP nor EGRESS, removes the selected live slot before item feedback and retains that paid
inventory through return/healing/walking retry. See the [current Wing agreement](../capability-status.md#battle02-angel-wing-implementation-agreement)
and [source/payment owner](../application/session-and-programs.md#paid-retreat-and-field-return).
The following extension sketch remains the wider-map/terminal-caller design; it grants no all-map,
Battle0, save/suspend or acquisition support.

**Confirmed static sources:** [Egress/Angel Wing control contract](../../../docs/design/contracts/battle-functions-control-flow.md#egress-and-angel-wing),
[actual admission/payment/caller](../../../docs/research/battle00-bonus-battle-and-reset.md#outcomes-escape-and-the-actual-reset-return)
and [ordinary outcome differences](../../../docs/design/synthesis/tactical-battle-loop.md#victory-defeat-and-admitted-retreat-leave-different-state).
EGRESS1 has cost8, range0/radius0 and teammate targeting. Its selected live actor must actually
reach actionable control, know the spell, confirm an eligible target and afford the cost. Angel
Wing requires a usable held slot and confirmed target. The special Egress path bypasses ordinary
CastSpell effect processing; do not import an unproven Silence gate from spell metadata.

| Boundary | Proposed extension |
| --- | --- |
| Content/action | Add a finite retreat spell/item definition and action offer to the existing semantic action pipeline, retaining selection/target/payment distinctions. Do not reinterpret every non-HEAL spell as retreat. Query and confirmation use the same live prerequisite rules. |
| Domain | Extend finite action preparation with a `PreparedRetreat` result carrying exactly MP cost or selected item consumption, no ordinary damage/award. Validation prohibits unrelated mutations/RNG. Add Retreat as a distinct battle exit result, never as Defeat or an overloaded zero. |
| Application | After successful confirmation, commit payment once, run retreat text/effect/close waits, update repeat/unlocked state, resolve live egress and dispatch the caller return. Bypass ordinary scene construction, defeated-hook/after-turn suffix, defeat HP/gold and victory new completion/aftermath. Keep the retreat stage in `ActiveBattle` until return admission succeeds. |
| Return/caller | Extend `IOutcomeReturnPolicy`'s finite result with destination alternatives, separating outcome costs from the caller's destination. Existing field return remains one alternative; Battle0 needs a session-end alternative. Never force a dummy map anchor for a non-field caller. |
| Godot | Project enabled/disabled retreat choice, self target, paid cost, text/effect completion and return/end state. Do not unload the battle immediately when a menu option is selected. |

```csharp
// Illustrative finite alternatives at existing action/return boundaries.
PreparedRetreat PrepareRetreat(EngineBattleState current, ActorRef actor, BattleActionRef action);
abstract record BattleReturnDestination;
record ReturnToField(FieldReturnAnchor Anchor, ProgramLocation Program) : BattleReturnDestination;
record EndSession(SessionEndReason Reason) : BattleReturnDestination;
```

Reuse current return programs/map load/field readiness only after the retreat-specific prefix.
Current `OutcomeReturnRules` validates HP/MP-only recovery and field anchors; ordinary outcome
selection admits only Victory/Defeat. The paid retreat continuation explicitly selects its own return.
A future destination must preserve those guards rather than overloading an ordinary outcome. Source egress uses
live context and may reset raft state; a first slice must declare its admitted flag/map subset and
retain explicit Unsupported for unimplemented branches. Payment/close/return failures retain the
already-paid prefix and pending continuation, never refund or pay again on duplicate input.

Battle0 is a caller policy: EndGame consumes every actual victory, defeat or admitted retreat return,
ignoring computed map parameters. Zeon has no leader sweep there. Suspend refusal leaves battle
control; it is neither successful retreat nor a reset. **Proposed modern recommendation:** publish
a terminal session reason after actual outcome/retreat services complete, dispose battle projection,
and offer an explicit return to the application title/new-session entry. Do not emulate CPU stack,
interrupt reset or silently save. Exact title interaction and whether unsaved progress is discarded
require product acceptance; post-reset original menu/storage behavior remains **Unknown**. No
finished-state publication may precede a still-pending service. Ordinary suspend/save stays separate.

Counterexamples: cancel/no target/insufficient MP, stale selection or completion, actor asleep,
slot changed before confirmation, successful payment followed by delivery failure, completed flag
held versus clear, different live egress map, all three Battle0 returns and refused Suspend. The
smaller ordinary-retreat slice needs no terminal session union or save subsystem and is recommended
first. Faking retreat as defeat saves little implementation work but couples costs and destinations
incorrectly; a general outcome workflow framework adds no value over these finite alternatives.

## Transport: possession, mode, position and service access

**Confirmed static sources:** [map transport synthesis](../../../docs/design/synthesis/map-design-principles.md#transport-acquisition-boarding-travel-and-landing),
[raft boarding/selected exits](../../../docs/research/polca-raft-boarding-through-south-parmecia-entry.md),
and [Caravan boarding/travel/landing](../../../docs/research/caravan-boarding-and-southwest-parmecia-travel.md).
Full raft event5 consumption, naturally traversed routes and durable transport persistence remain
**Unknown** at these owners. Read the numeric consumers: mode1/opcode7 is Caravan and mode2/opcode8
is raft despite the misleading source opcode names.

The implemented finite Caravan subset reuses entity motion, physical/logical bindings, working layout,
typed story/actions and existing waits. `FieldTravelMode` distinguishes walking from Caravan;
`CurrentEntities` retains the explicit current physical list. Acquisition flags are still story state.
The [content owner](../content/profiles-and-trust.md#authored-caravan-world) admits one controlled
standalone world; it does not reconstruct Map68 or fresh population.

| State / phase | Implemented boundary |
| --- | --- |
| Acquired | F65 enables the walking marker producer; it does not imply mode1 or fresh population. C-button services remain separate. |
| Parked Caravan | Live physical entity position/facing are authoritative. Numeric events2/4 do not write the raft's saved map/X/Y. No persistent Caravan VehiclePlacement is inferred. |
| Boarding | After obstruction and before raw blocking, the0x3800 producer requests event2. The program uses logical FOLLOWER_A separately from raw0/1 and the current-list suffix; no raft alignment recheck or resource reset. Mode1 follows actual waits. |
| Travelling | Mode1/opcode7 uses unshrunk bounds and current-only physical obstruction. Marker travel has no walking zone/counter/sound tail. Door/warp/copy combinations fail explicitly in this admission. |
| Landing | Event4 clones/restores/grows live actors, waits only the last current suffix actor, installs flag-selected follower scripts by active-row ordinal, then commits mode0. It neither rebuilds population/list nor triggers a zone. Partial failures retain mode1 and the committed scene. |
| Reentry and services | Map replacement, persistence, fresh acquisition and services are Unsupported in this standalone composition. Future work requires its own source/acceptance boundary; parked persistence is not assumed. |

Changed-state coverage includes acquired clear/set, nonaligned Caravan, logical/raw-slot disagreement,
current list length/order/content, zero-speed/pending failure, current versus destination occupancy,
walking versus travel bounds/counter/zone behavior, live HP-bound appearance and flag-selected action
reinstall. Actual Godot uses authored markers and modern tick/draw delivery. It makes no original
sprite, sound or timing claim. Full class-to-mapsprite behavior is an admitted class-specific appearance
binding; save/load and original natural reach remain Unknown. Raft mode2/event5 is deferred, not a
mirrored implementation. The broader transport extension remains Proposed beyond this finite subset.

## Dependencies and proposed order

Selected Battle02 EGRESS and Angel Wing use the finite payment/continuation and current agreements
linked above; other-map bindings and terminal callers remain proposed. The remaining sequence is
advice, not a ticket queue or a claim that every later binding is ready. No
unmerged work is a dependency. PR692's UI files remain separately owned; future semantic menu work
must consume its accepted state or coordinate shared paths before writing. Do not make that layout
proposal an architectural prerequisite for any rule.

| Family / owner | Classification and smallest useful follow-up | Ordering reason |
| --- | --- | --- |
| Ordinary retreat | Rule + finite action/return extension; selected Battle02 binding. [Exit design](#special-exits-payment-battle-return-and-session-end). | **Implemented for Battle02 spell/item retreat.** Existing world, costs/returns and semantic action UI bound the work; wider maps/callers remain unsupported. |
| Item custody/full-pack fallback | Packed live loadout and finite authored physical drop/once/Deals custody are implemented; wider acquisition and service receipts still need their own policies. [Economy receipt routes](../../../docs/design/synthesis/progression-and-economy.md#sources-receipts-and-the-capacity-decision), [Caravan/Deals contract](../../../docs/design/contracts/caravan-and-deals-state.md). | Before private Taros, shop/depot and mandatory receipts. The implemented battle receipt does not supply field Search restoration or story discard loops; these policies are not interchangeable. |
| Equipment curse/break/use | ATT-only equipped contribution, Herb consumption and selected Wing retreat supported; equip/remove/curse refusal, break state, wider use effects and refresh need rules/action/service binding. [Equipment](../../../docs/design/synthesis/progression-and-economy.md#carried-gear-becomes-an-equipped-contribution), [field effects](../../../docs/research/field-item-effects.md). | Taros gate can consume explicit live equipped input first; a player equip interaction is a separate coherent outcome, needed before a full obtain/equip/fight route. Reuse loadout/refresh; do not invent an equipment framework. |
| Leader cleanup / Taros | Authored effectiveness, cleanup and physical drops are implemented; private encounter/aftermath gaps remain. [Taros design](#taros-effectiveness-cleanup-loot-and-acquisition). | Complete Taros chapter still waits for private encounter and actual aftermath bindings. Cleanup is reusable by Prism/Fairy objectives, not by Battle0. |
| Attack/area spells, AURA, DETOX, BOOST/SLOW, MUDDLE and wider statuses | HEAL/physical sleep supported; other effects lack admitted action rules, multi-target/status publication and presentation. [Spell contract](../../../docs/design/contracts/spell-resolution.md). | Add one concrete action family through existing preparation/reaction/after-turn owners. AURA multi-target recovery and DETOX status clearing are distinct; buffs require refresh/counter expiry; MUDDLE needs control/target semantics. Prism's finite multi-target work can inform these but is not a mandatory universal spell foundation. |
| Prism laser | Finite authored decision/action, source-slot binding and visual delivery are implemented; private chapter/special-input gaps remain. [Laser design](#prism-flowers-decision-ray-versus-construction-hit-list). | Independent of broad spell/status completion. Complete Battle36 also requires its ordinary enemy capabilities, leader cleanup and story/resources; laser support alone is not chapter acceptance. |
| Region programs / Jaro / ally AI | Implemented authored resumable round prefix, live control and visual aliases; [Jaro design](#jaro-region-programs-and-two-logical-actors-sharing-a-visual). | Private Battle32 and full victory recruitment remain separate consumers. Reuse party flags; no requirement to first build Caravan services. Ally AI is a new rule, not a faction toggle on physical enemy AI. |
| Hidden spawn and trial record | Shared round continuation plus finite reset lifecycle; trial clock/result choice separate. [Fairy design](#fairy-woods-spawning-and-result-state-have-different-lifetimes). | Reuse accepted round-prefix work when available; otherwise introduce only its spawn consumer. Source geometry differences must be resolved before claiming Battle44 activation. Record persistence waits for saving; spawn work need not wait for scoring choice. |
| Caravan travel / raft | Standalone authored Caravan state/controller and programs implemented; raft and full-map consumers deferred; [transport design](#transport-possession-mode-position-and-service-access). | Independent of full battle import when given explicit initial acquisition. Caravan's complete source consumer is the better first travel slice; raft landing awaits its named gap decision, not a guessed clone. |
| Caravan party/depot/item/purge | Joined/active flags and slots exist; member selection/relief, 64-slot depot, exchange and service continuation absent. [Service contract](../../../docs/design/contracts/service-interactions.md#caravan-source-boundary), [custody](../../../docs/design/synthesis/progression-and-economy.md#custody-storage-and-active-membership). | After one inventory receipt/store rule, implement one service result. Boarding is not required to design the transaction, but natural C-menu availability needs separate field binding. Preserve twelve-member ordinary guard and leader/curse refusals. |
| Church raise/cure/promote, shop and Mithril | Source contracts exist; production service transactions/UI are not supplied by ordinary outcome healing/growth. [Services](../../../docs/design/contracts/service-interactions.md), [promotion](../../../docs/design/synthesis/progression-and-economy.md#church-recovery-promotion-and-saving), [Mithril orders](../../../docs/design/synthesis/progression-and-economy.md#mithril-orders-placement-readiness-and-collection). | Reuse gold, loadout, class refresh and typed waits; add paid selection/cancel/commit for one service at a time. Promotion needs class/item eligibility and post-change resources; Mithril needs order/readiness/collection state and RNG, not just a shop stock row. |
| Save/load/suspend | **Unsupported** cross-session persistence; [save contract](../../../docs/design/contracts/save-system.md). No field return, RAM best or egress selection proves storage. | Separate High-rigor acceptance for versioned state, write failure/recovery and real reload. Needed for durable records/resume, not first retreat or in-session transport. Battle0 refusal remains possible without implementing suspend. |
| Battle0 terminal caller | New finite session-end destination and adopted UX decision. [Special exits](#special-exits-payment-battle-return-and-session-end). | After ordinary retreat/return distinction. Full hidden encounter also needs its combat/content families; terminal routing alone cannot admit the full battle or assert new-game-plus. |

## Recommended first implementation: retreat and playable retry

**Implemented bounded outcome:** in the existing selected Battle02 world, a player controlling a living actionable ally
can confirm self with learned EGRESS1 and pay8 MP, or select a held Angel Wing and consume exactly
that slot without MP/learning, complete the retreat presentation, reach usable field control at the admitted egress destination,
then walk back into Battle02 with fresh enemies in the same session. Payment must occur even though
the subsequent ordinary supplied-map exploration entry heals eligible living/immortal allies;
that later caller recovery is separate from defeat's leader revival/gold halving and victory's
new completion. This is a bounded ordinary retreat, not Taros/Battle0, all savepoints or full EGRESS
coverage. The [common post-battle caller inspection](../../../docs/research/battle18-outcomes-and-caravan-acquisition.md#victory-map12-control-or-warningretreat)
names the exploration-entry healing boundary; its [alternate-return inspection](../../../docs/research/battle18-outcomes-and-caravan-acquisition.md#defeategress-supplied-savepoint-not-a-fixed-hassan-return)
establishes that direct outcomes take that caller. Map-specific content still needs its own admission.

Use **High rigor for that implementation** because payment, staged waits, live party return and
retry cross action/story/map owners. Its agreement belongs with the implementation owner when
authorized. Dependencies are accepted Battle02 entry/return, typed waits, live party carry and the
source Egress admission/return facts above. No content acquisition or save system is prerequisite.
Keep `Heal` reusable for the distinct map-entry recovery, with tests of eligible living/immortal
versus ordinary dead members; never reach it by routing Retreat through the current defeat `Finish`.

| Observation point | Required resource/state interpretation |
| --- | --- |
| After payment, before retreat delivery | Exactly one8-MP deduction for EGRESS, or removal/compaction of the selected Wing slot for item retreat; no defeat gold/leader revival, damage, reward or new completion. This is the payment assertion, not a final-field MP assertion. |
| Actual battle return | Paid state and retreat-specific unlock handling have completed; destination parameters alone do not prove usable field control. Do not execute victory/defeat suffixes or consume a new battle turn. |
| Exploration rebuild and usable field | The supplied-map caller applies its eligible-party healing/refresh and map setup/init. Attribute changed HP/MP/status to that stage, retaining dead-member exceptions and prior gold/inventory/flags as required. No claim that paid MP remains deducted after recovery. |
| Fresh Battle02 reentry | The [fresh-entry contract](../../../docs/design/contracts/battle-control-lifecycle.md#new-battle) heals eligible allies and initializes enemies again; [current initializer](../../src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) already implements that boundary. Preserve identity, growth/loadout/accounting and proper dead/active placement, not an artificial unchanged HP/MP requirement. |

Before implementing the selected route, join its actual egress map/flags, return program and
exploration recovery call against these common owners. That is a named static/code integration
check, not a new runtime acquisition prerequisite; any unsupported map/status branch stays explicit.

Candidate paths to review for ownership (not authorization to edit them now):

- `Domain/Battles/State/EngineBattleState.cs`, `Battles/Rules/IBattleActionRule.cs`,
  `BattleActionResolution.cs`, and a small `Gameplay/Sf2` retreat rule for action identity, target
  admission and finite paid result. Extend learned-spell representation only enough to express
  EGRESS beside existing healing definitions; preserve both live/source spellbook consistency.
- `Application/Runtime/SessionRules.cs`, `Battles/BattleCommandDispatcher.cs`, action continuation,
  `Exploration/BattleOutcome.cs`, `IOutcomeReturnPolicy.cs` and `Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs`
  for an explicit retreat stage and field return. Existing policy methods that branch "not victory"
  must not treat retreat as defeat. Extract touched responsibilities from oversized files under
  the repository source-size rule instead of growing a new central switch.
- Existing Content encounter/action readers and the selected preparation binding in
  `src/sf2tool/remake_exploration_battles.py`, only after checking their actual spell/loadout owner.
  Admit one evidenced egress branch; keep unsupported raft/special-map flags closed. Public authored
  inputs may exercise the same rule without private provenance bypass.
- Current semantic battle menu/scene and exploration view consumers for real completion/field
  readiness, using the accepted UI owner at implementation time. No menu redesign, new media
  platform or implicit asset fallback; use approved authored feedback if private resources are not
  already available, with that presentation deviation declared before acceptance.

The player scenario may start from an explicitly controlled admitted Battle02 input with EGRESS1
and suitable MP. That proves the new behavior, not natural learning or full opening chronology.
After start, use ordinary commands only: no flag/resource injection, snapshot replacement or reseed.
Do not infer support just because a modified initial fixture lists the spell.

Small engine behavior tests must cover MP7 rejection/MP8 success, cancellation and missing target,
an unlearned spell, sleeping control, changed live envelope/MP before confirmation, duplicate/wrong
completion, and a presentation/return failure after payment retaining the prefix exactly once.
Compare gold, other members' HP/status/inventory, main/thinking seeds, unlock/completion and party
identity at payment, return and new entry, attributing later healing to its actual caller. Include
held versus clear completion and a second admitted egress selection
to expose fixed-map assumptions. A same-session command test must reenter with fresh enemies and
the party produced by the paid retreat plus ordinary map-entry recovery. Unit tests of a new
probe/planner are not part of this slice.

Native acceptance uses the owned Godot instance's real semantic input/state/projection/errors:
disabled or rejected insufficient-MP choice, successful payment and actual feedback completion,
usable field movement, ordinary walking reentry and the next usable battle turn. Observe focus/
suspension during a pending wait and ensure no second payment. Use changed valid actor resources,
not a fixed receipt count or round. No screenshots, emulator capture, whole-route H4 or old aggregate
rerun. Run affected behavior tests, adapter build and dependency-selected checks; use a direct source
comparison only where the implementation makes a new claim needing it.

Stop when that result works within the declared admitted route, unsupported egress branches remain
explicit, doc/code boundaries agree and independent review accepts the frozen candidate.
All-map egress, battle4 special loss, Battle0 ending, save/suspend, full transport and later chapters
remain deferred. A discovery requiring a new campaign/persistence platform or additional media
acquisition changes the scope and must be resolved before expanding; it is not hidden prerequisite
work for this first slice.

## Decisions that remain Proposed

| Decision | Recommendation / impact | What can proceed without it |
| --- | --- | --- |
| Fairy Woods score semantics | Prefer explicit deterministic result snapshot; acknowledge logical-work scoring rather than real-time speed and source callback score. Requires product acceptance of the deviation and exact tick/start/stop policy. | Spawn/reset/region design and ordinary in-session result storage boundaries. |
| Battle0 modern endpoint | Terminal session with explicit title/new-session transition, no hardware reset or implicit save. UX and unsaved-state handling require acceptance; original post-reset retention stays Unknown. | Ordinary retreat and finite outcome/caller separation. |
| Jaro malformed retained alias / ordinary capacity | Reject unsupported invalid aliases with preserved prefix; retain the named source over-capacity operation rather than silently normalize it. Any broader safety/deviation policy must be explicit. | Two-actor conversion, live control and normal valid alias/queue cases. |
| First retreat presentation | Reuse available accepted semantic consumers/resources; any authored substitute needs a declared presentation boundary. No UI/art decision is selected by this design. | Action/payment/return rules and candidate path review. |

None of these proposals reopen the accepted #514 source documentation, replace the modern clock,
reclassify historical comparison failures or authorize implementation. Revisit this owner when a
selected mechanism is accepted so its gap classification reflects actual consumers rather than a
second accumulating milestone history.
