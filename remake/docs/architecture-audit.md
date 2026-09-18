# Remake Architecture and Verification Audit

Status: **OPEN — bounded common-engine remediation is implemented; A1–A8 are not globally closed.**

This audit records the user's clarified product direction and the implementation findings that must
inform the next architecture decision. It does not authorize another gameplay slice, adopt a new
framework, or report the continuous milestone complete.

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

## Current Remediation Boundary

[ADR0019](../../docs/decisions/0019-state-and-content-driven-remake-engine.md) owns the current
implementation. The findings below describe the exact audited baseline, not acceptance rules for
the common path.

| Finding | Implemented response | Still open |
| --- | --- | --- |
| A1/A7 | Common HEAL/physical/AI/death/growth/outcome legality follows live state and loaded definitions. Connected Battle01 play uses actual commands without receipt or kill-order guards. | Broader actions, status/equipment and other outcomes. |
| A2 | Validated authored and selected private definitions/programs enter production Content. Controlled initial values and provenance remain explicit. | Original natural input producers and broader content families. |
| A3 | Common opening, castle/tower, full before/after-battle and ordinary-defeat programs execute instructions, waits and source return/init. Matching legacy endpoint/outcome/return writers are removed. | Other native/program branches and full original presentation. |
| A4/A5 | Application owns one live session across exploration/battle/outcome/return; Godot projects actual waits and semantic input. | Remaining G3/G4 program/content consumers; the reference host was retired at M5. |
| A6 | Immutable runtime state carries actor progress through return and re-entry. Migrated receipt chains and arrival projections are removed with last callers. | None from the reference runtime (retired at M5); broader state families as capabilities grow. |
| A8 | Typed Content/rule/program failures survive the actual host; negative scratch/wait cases retain exact reasons. | Broader invariant/adapter failure lifecycle and original profiles. |

[M5](../../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m5-implementation) retired the reference runtime, host and legacy tests; the
[reference input inventory](../reference/README.md) names the remaining controlled comparison inputs.
Production projects have no Reference dependency. The selected Battle01 outcome/program group and its actual native observations
do not close original natural continuity, A1–A8 or 8C/H4.

## Findings

P1 findings affect the next engine architecture and should be resolved before resuming the existing
feature-extension strategy. P2 findings affect maintainability and diagnosis. These are architecture
findings; an intentionally bounded earlier implementation can satisfy its local tests and still be an
unsuitable long-term engine boundary.

### A1 — P1: Reference trajectories gate production rules

**Confirmed:** `Battle01PlayerHealing.RequireControl` in
[`Battle01PlayerHealing.cs`](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/8a581a82e297ea2947cc9837e661752163d2806d/remake/reference/Sf2.Remake.Reference/Rules/Battles/Battle01PlayerHealing.cs) requires round 13,
turn offset 4, actor 1, and exactly 102 prior receipts. `RequireSupportOrigin` also requires Sarah's
specific initial HP, MP, EXP, equipment, and spell values. These conditions execute in production.

`Battle01FirstRound.EnterNext` in
[`Battle01FirstRound.cs`](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/8a581a82e297ea2947cc9837e661752163d2806d/remake/reference/Sf2.Remake.Reference/Rules/Battles/Battle01FirstRound.cs) admits a five-survivor
continuation only from the named rounds and, for the post-heal case, five exact unit positions.
`Battle01TurnCompletion.CompleteDefeatedTurn` in
[`Battle01TurnCompletion.cs`](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/8a581a82e297ea2947cc9837e661752163d2806d/remake/reference/Sf2.Remake.Reference/Rules/Battles/Battle01TurnCompletion.cs) only admits
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

**Confirmed:** [`PrivateOriginalBattle01StartupReader.cs`](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/8a581a82e297ea2947cc9837e661752163d2806d/remake/reference/Sf2.Remake.Reference/Readers/PrivateOriginalBattle01StartupReader.cs)
does read JSON, but fixes input digests, three allies, six enemies, specific identities, and selected
map values. [`OriginalMapRuntimeAdmission.cs`](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/8a581a82e297ea2947cc9837e661752163d2806d/remake/reference/Sf2.Remake.Reference/Content/Maps/OriginalMapRuntimeAdmission.cs)
also contains source identities, digests, exact roof/chest/door rows, map IDs, and coordinates.
[`OriginalBattle01ControlledPartyPreset.cs`](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/8a581a82e297ea2947cc9837e661752163d2806d/remake/reference/Sf2.Remake.Reference/Fixtures/Battle01/OriginalBattle01ControlledPartyPreset.cs)
admits named comparison presets with fixed effective stats and seeds.

**Impact:** changing valid content can require changing runtime C# admission logic. The current private
reader is useful for proving a specific source input, but does not provide the user's intended
configuration-driven engine contract.

**Correction boundary:** retain source identity checks in extraction/import and reference verification.
The runtime content contract should validate supported shapes, IDs, references, ranges, and capabilities
without encoding the one acceptance dataset as the definition of valid gameplay. Original private and
public-authored inputs must continue to retain their distinct provenance and distribution boundaries.

### A3 — P1: Some story capabilities commit an endpoint instead of executing a program

**Confirmed:** the palace endpoint writer and complete castle/Astral/tower shortcut handlers are
removed with their final execution callers. The common session executes the complete source palace
body and its native caller, including motion, text, presentation, map state and completion flag.
The [execution owner](./exploration-programs.md#castle-palace-astral-and-tower) records grouped
comparisons, the accepted source identity lifecycle and the remaining original runtime/hardware boundaries. Later Battle01/M4 comparison metadata
does not become executable story logic. The common session also executes complete Battle01 before,
initialization/load/start and first input; its old Map40 pending writer and final movement/presentation
callers are removed. Explicit M4 fixtures retain comparison-only startup/return context. A3 remains
open for other original programs and unsupported branches.

### A4 — P1: Godot owns part of battle orchestration

**Confirmed:** `PrivateBattle01Ui.DispatchNext` in
[`PrivateBattle01Composition.cs`](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/8a581a82e297ea2947cc9837e661752163d2806d/remake/reference/game/src/PrivateBattle01Composition.cs) chooses next-round entry,
dead-slot completion, enemy pursuit/attack/standby, and return to player control. It calls the
Application mutation methods, so this finding is not a claim that Godot directly writes HP or RNG.

**Impact:** an engine-independent host cannot obtain the complete same continuation by submitting only
the player's action without also reproducing the Godot-side scheduling logic. ADR 0011 assigns this
orchestration to Application.

**Correction boundary:** Application should advance the supported flow to its next input, presentation,
or unsupported-capability boundary. Godot should submit semantic input and present observations.

### A5 — P1: The session facade contains incompatible runtime paths

**Confirmed:** [`GameSession.Apply`](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/8a581a82e297ea2947cc9837e661752163d2806d/remake/reference/Sf2.Remake.Reference/Sessions/GameSession.cs) throws when
the session uses the private original-map path. That path has separate movement and battle methods.
Its [constructor](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/8a581a82e297ea2947cc9837e661752163d2806d/remake/reference/Sf2.Remake.Reference/Sessions/Maps/OriginalMapGameSession.cs) sets `_snapshot` to
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

**Confirmed:** [`PrivateOriginalMapSessionSnapshot`](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/8a581a82e297ea2947cc9837e661752163d2806d/remake/reference/Sf2.Remake.Reference/Sessions/Maps/OriginalMapGameSession.cs)
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
[`player attack handler`](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/8a581a82e297ea2947cc9837e661752163d2806d/remake/reference/Sf2.Remake.Reference/Sessions/Battle01/PrivateOriginalBattle01PlayerPhysicalAttack.cs)
selects kill policies using preset names, actor identity, and the live number of deaths.

**Impact:** adding healing requires edits to existing attack and round admission even where those
rules have not acquired new semantics. This is concrete change amplification from scenario coupling,
not merely a concern about file or public-type counts.

**Correction boundary:** rule availability should follow admitted content and implemented capabilities.
Test-case identity must not be the routing key for unrelated production actions.

### A8 — P2: Runtime diagnostics erase useful failure distinctions

**Confirmed:** [`PrivateOriginalBattle01PlayerMovement.cs`](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/8a581a82e297ea2947cc9837e661752163d2806d/remake/reference/Sf2.Remake.Reference/Sessions/Battle01/PrivateOriginalBattle01PlayerMovement.cs),
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

## Original Audit Decision Boundary

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

ADR0019 supplies the adopted engine direction and bounded implementation decisions. The original
fidelity scope remains open. This findings record does not authorize unrelated features, removal of
genuine source guards, changes to evidence labels, or a claim of complete remediation.
