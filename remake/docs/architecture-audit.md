# Remake Architecture and Verification Audit

Status: **OPEN — findings recorded; engine remediation is not implemented.**

This audit records the user's clarified product direction and the implementation findings that must
inform the next architecture decision. It does not authorize another gameplay slice, adopt a new
framework, or report the continuous milestone complete. Main-gate finishes the currently assigned
slice's acceptance and then stops; this documentation task is separately authorized.

The inspected accepted baseline is commit `24a10b99a30603d35e8aec9ead3276710f957ed7`, tree
`1865117841e26d4698961a581eac22db88f9c5c0`. Findings below refer to that exact object, independently of
later changes on `main`. The review inspected tracked source, decisions, tests, and Git history; it did
not run a new game session or claim new original-ROM observations.

## Current User Direction

The user clarified the intended product on 2026-09-13:

- Build an intuitive modern tactical RPG engine from the reverse-engineered original design.
- Store original maps, battle configurations, and other game content in JSON or another suitable
  configuration format, and load the needed content through the engine's content boundary.
- Use [ADR 0009](../../docs/decisions/0009-first-phase4-playable-slice.md)'s continuous scenario to
  compare modern-engine behavior with the original ROM.
- Extensive 0009-specific verification is welcome, but its reference-route assertions must remain
  separate from engine logic. Reference round numbers, action-history lengths, coordinates, and
  character-specific sequences do not become gameplay preconditions just because a test uses them.
- Stop screenshot-based Godot verification. Observe the actual running instance through an appropriate
  debug/state interface, such as a suitable Godot debugging connection or existing native probe.
  The concrete interface has not yet been selected; see [the validation boundary](#godot-validation-boundary).
- Reuse the same verified Godot installation, owning project, and running debug instance by default.
  A new verification point alone is not a reason to create another engine copy or instance.

This direction preserves original-behavior research as evidence. It does not authorize guessed rules,
silent fallbacks, unverified claims about the ROM, or redistribution of private inputs.

## Findings

P1 findings affect the next engine architecture and should be resolved before resuming the existing
feature-extension strategy. P2 findings affect maintainability and diagnosis. These are architecture
findings; an intentionally bounded earlier implementation can satisfy its local tests and still be an
unsuitable long-term engine boundary.

### A1 — P1: Reference trajectories gate production rules

**Confirmed:** `Battle01PlayerHealing.RequireControl` in
[`Battle01PlayerHealing.cs`](../src/Sf2.Remake.Domain/Battles/Battle01PlayerHealing.cs) requires round 13,
turn offset 4, actor 1, and exactly 102 prior receipts. `RequireSupportOrigin` also requires Sarah's
specific initial HP, MP, EXP, equipment, and spell values. These conditions execute in production.

`Battle01FirstRound.EnterNext` in
[`Battle01FirstRound.cs`](../src/Sf2.Remake.Domain/Battles/Battle01FirstRound.cs) admits a five-survivor
continuation only from the named rounds and, for the post-heal case, five exact unit positions.
`Battle01TurnCompletion.CompleteDefeatedTurn` in
[`Battle01TurnCompletion.cs`](../src/Sf2.Remake.Domain/Battles/Battle01TurnCompletion.cs) only admits
the dead enemy 129 slot at round 12/offset 6 with the specified preceding history. Its player attack
policies separately cap first, second, and Chester's third enemy defeat.

**Impact:** a different interactive path can reach a mechanically similar action and be rejected
because it lacks the recorded trajectory. This conflicts with
[ADR 0010 section 4](../../docs/decisions/0010-map3-battle01-product-acceptance.md#4-player-agency-action-coverage-input-trace-and-rng),
which explicitly separates reproducible reference input from the player's freedom to diverge.

**Correction boundary:** reference trajectories belong in test/replay cases. Production admission
should express the supported rule, current legal state, and required content. Removing a trace guard
must not silently claim support for a still-unimplemented spell, status, accounting effect, or rule.

### A2 — P1: Fixed evidence admission substitutes for configurable content

**Confirmed:** [`PrivateOriginalBattle01StartupReader.cs`](../src/Sf2.Remake.Content/PrivateOriginalBattle01StartupReader.cs)
does read JSON, but fixes input digests, three allies, six enemies, specific identities, and selected
map values. [`OriginalMapRuntimeAdmission.cs`](../src/Sf2.Remake.Application/Content/OriginalMapRuntimeAdmission.cs)
also contains source identities, digests, exact roof/chest/door rows, map IDs, and coordinates.
[`OriginalBattle01ControlledPartyPreset.cs`](../src/Sf2.Remake.Application/Content/OriginalBattle01ControlledPartyPreset.cs)
admits named comparison presets with fixed effective stats and seeds.

**Impact:** changing valid content can require changing runtime C# admission logic. The current private
reader is useful for proving a specific source input, but does not provide the user's intended
configuration-driven engine contract.

**Correction boundary:** retain source identity checks in extraction/import and reference verification.
The runtime content contract should validate supported shapes, IDs, references, ranges, and capabilities
without encoding the one acceptance dataset as the definition of valid gameplay. Original private and
public-authored inputs must continue to retain their distinct provenance and distribution boundaries.

### A3 — P1: Some story capabilities commit an endpoint instead of executing a program

**Confirmed:** `CompletePrivateOriginalMapPalaceFirstVisit` in
[`PrivateOriginalMapPalaceFirstVisit.cs`](../src/Sf2.Remake.Application/Sessions/PrivateOriginalMapPalaceFirstVisit.cs)
publishes a preselected player endpoint and completion receipt in one transition. The associated
[`definition`](../src/Sf2.Remake.Application/Content/OriginalMapPalaceFirstVisitDefinition.cs) names the
source program and its operation count, but the transition does not execute that sequence of movement,
dialogue, and event operations.

**Impact:** this is a controlled result projection, not the typed program execution described by
[ADR 0011](../../docs/decisions/0011-phase4-remake-runtime-architecture.md#5-programs-content-and-data-imports).
The existing documentation correctly limits its claim; it still leaves an engine capability missing.

**Correction boundary:** define and implement the needed event/program operations from accepted
contracts. Reference endpoint checks remain tests. Do not implement a universal scripting framework or
invent missing original semantics merely to replace one projection.

### A4 — P1: Godot owns part of battle orchestration

**Confirmed:** `PrivateBattle01Ui.DispatchNext` in
[`PrivateBattle01Composition.cs`](../game/src/PrivateBattle01Composition.cs) chooses next-round entry,
dead-slot completion, enemy pursuit/attack/standby, and return to player control. It calls the
Application mutation methods, so this finding is not a claim that Godot directly writes HP or RNG.

**Impact:** an engine-independent host cannot obtain the complete same continuation by submitting only
the player's action without also reproducing the Godot-side scheduling logic. ADR 0011 assigns this
orchestration to Application.

**Correction boundary:** Application should advance the supported flow to its next input, presentation,
or unsupported-capability boundary. Godot should submit semantic input and present observations.

### A5 — P1: The session facade contains incompatible runtime paths

**Confirmed:** [`GameSession.Apply`](../src/Sf2.Remake.Application/Sessions/GameSession.cs) throws when
the session uses the private original-map path. That path has separate movement and battle methods.
Its [constructor](../src/Sf2.Remake.Application/Sessions/OriginalMapGameSession.cs) sets `_snapshot` to
null and `_mapContext` to `null!`; the public `Snapshot` property is consequently unavailable.
Exploration, battle, and return observations also require path-specific accessors.

At the audited baseline, 35 source files contribute to the same `GameSession` partial class. The
class owns command routing, admission, lifecycle handlers, shared backing state, and snapshot
construction. Physical file separation has not supplied corresponding internal responsibility
boundaries; the [architecture owner](./architecture.md#application-facade) already identifies that work.

**Impact:** callers must understand which mutually exclusive implementation they received. A shared
class name does not provide one consistent engine command/state contract.

**Correction boundary:** retain one logical mutation facade with explicit focused internal owners.
Choose a coherent state/command contract while keeping content trust profiles separate. Do not force
unrelated inputs through a weaker reader to obtain superficial API uniformity.

### A6 — P2: Scenario-specific snapshot construction amplifies changes

**Confirmed:** [`PrivateOriginalMapSessionSnapshot`](../src/Sf2.Remake.Application/Sessions/OriginalMapGameSession.cs)
has 32 constructor parameters, including Sarah, entity 142, messenger, castle-gate, palace-visit, and
other scenario-specific state/receipts. Ordinary movement reconstructs the snapshot by individually
preserving or clearing those arguments. The constructor also validates relationships among these
specific route states.

**Impact:** new story content expands shared state construction and increases the risk of omitted or
incorrectly reset fields in unrelated transitions. This review does not establish an actual lost-state
incident.

**Correction boundary:** organize live state around the selected engine responsibilities and explicit
transitions. Preserve immutable observations and atomic publication; do not solve the problem with an
untyped property bag or a second writable state owner.

### A7 — P2: A new capability changes unrelated preset allowlists

**Confirmed:** the healing change in Git object `7d51ff6fa` updates existing enemy attack, player attack,
next-round, and dead-turn Application handlers to also accept `SarahHealComparisonId`. The current
[`player attack handler`](../src/Sf2.Remake.Application/Sessions/PrivateOriginalBattle01PlayerPhysicalAttack.cs)
selects kill policies using preset names, actor identity, and the live number of deaths.

**Impact:** adding healing requires edits to existing attack and round admission even where those
rules have not acquired new semantics. This is concrete change amplification from scenario coupling,
not merely a concern about file or public-type counts.

**Correction boundary:** rule availability should follow admitted content and implemented capabilities.
Test-case identity must not be the routing key for unrelated production actions.

### A8 — P2: Runtime diagnostics erase useful failure distinctions

**Confirmed:** [`PrivateOriginalBattle01PlayerMovement.cs`](../src/Sf2.Remake.Application/Sessions/PrivateOriginalBattle01PlayerMovement.cs),
the player attack handler, and other handlers catch `ArgumentException`, keep its `ParamName`, and
replace its message with a generic rejection. The called Domain code uses that exception family for
both unsupported/invalid actions and history/state invariant checks. Runtime rejections also reuse
`OriginalBattle01StartupDiagnostic`.

**Impact:** expected user-command rejection, missing capability, and an internally inconsistent state
can be difficult to distinguish; original diagnostic detail is lost at the boundary.

**Correction boundary:** use a small consistent runtime failure contract, preserve useful bounded
diagnostics, and keep programmer invariant failures distinguishable from expected unsupported input.
Do not add a separate public result hierarchy for every internal helper.

## Interpretation and Retained Value

**Inferred:** repeated extension to the next reference-route refusal encouraged production guards for
individual acceptance states. [ADR 0016](../../docs/decisions/0016-remake-start-evidence-deferral.md)
allows bounded implementation, but does not require these trajectory-specific guards. ADR 0010's
manual-player policy and ADR 0011's content/program/orchestration responsibilities remain relevant.
[ADR 0017](../../docs/decisions/0017-heavy-boundaries-light-internals.md) addresses protocol growth and
internal delegation; applying it only by moving methods into partial files cannot close A1-A7.

Git history identifies the changes, not the model responsible for each design decision. Attribution
to a particular agent/model is **Unknown** from this source review.

Retain the inward assembly dependencies, engine-free numerical rules and state transitions,
deterministic RNG work, immutable observations, stale-request checks, validation-before-publication,
and existing independent source evidence. The code performs real calculations; this audit does not
classify it as a prerecorded movie. Existing passing and failing tests remain evidence of their named
boundaries, not proof that the broader architecture is complete.

## Godot Validation Boundary

The current user instruction is to stop screenshot-based verification, including repeated capture and
visual comparison as a substitute for runtime assertions. Preserve existing captures and results as
historical artifacts; do not rerun them to satisfy this audit. No new screenshot acceptance run is
authorized by this document.

For future authorized verification, first reuse an existing native probe or suitable running-instance
debug/state channel. Observe the real session and scene tree: current actor/phase/state, command
outcome, node properties, visibility, geometry, focus, input handling, runtime errors, and process
completion as required by the specific claim. If the existing interface cannot expose a required
fact, record that gap before choosing the smallest additional observation seam.

Reuse the owning task's verified Godot installation, project workspace, and running debug instance
while they remain suitable. Do not routinely re-extract the engine, copy the project, or launch a new
instance for each check. Restart or create a separate instance only for a concrete reason: an observed
crash or lost connection, a code/import change that actually requires restart, demonstrated state
contamination, or a check specifically about startup, export, or process cleanup. Record the reason
and keep the additional lifetime bounded. When a supported session reset can establish the required
scenario, reuse it. Reuse does not permit another writer to mutate the same
project workspace or commandeer another task's running instance.

Expected facts belong in the test driver. They must not be returned as if observed from the game or
installed as production preconditions. A test that calls Application directly establishes a different
boundary from one that exercises Godot input mapping. Keep those claims explicit.

The exact Godot debug-server/remote-debugger/probe arrangement is **Unknown / not selected**. Check the
accepted engine's actual supported interface before proposing it. This instruction does not create a
new server, network protocol, plugin, dependency, production service, or runtime-validation framework.
The existing bounded launch, process ownership, and cleanup requirements still apply.

Node and session state alone do not prove framebuffer, audio, or hardware-exact parity. Unobserved
fields remain unverified; this audit neither declares the 8C/H4 requirements passed nor silently
waives them.

## Decisions Still Needed Before Engine Changes

- Reconcile the user's modern-engine behavioral comparison goal with
  [ADR 0010's selected 8C profile](../../docs/decisions/0010-map3-battle01-product-acceptance.md#8-visual-and-audio-acceptance-tier)
  and ADR 0011's hardware-observation backend. The documents record prior 8C acceptance; this audit does
  not claim it was unauthorized or automatically rescinded. The desired observable scope must be
  explicit in the follow-up decision.
- Define the smallest runtime content and event/program contracts needed for the continuous milestone,
  including loading boundaries. JSON is an acceptable user direction, not a reason to create a large
  configuration framework or copy the research export format unchanged into the engine.
- Select the first coherent rule/flow migration and its independent tests. Keep the 0009 reference
  scenario as an integration test while checking supported rules across more than one trajectory.

These decisions are pending. Do not interpret this findings record as permission to resume feature
work, remove every guard, rewrite the whole repository, change evidence labels, or report remediation
complete.
