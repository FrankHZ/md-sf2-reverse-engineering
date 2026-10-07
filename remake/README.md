# SF2 Remake

The remake uses reverse-engineered behavior and implementation-neutral contracts to build a modern
tactical engine. Godot hosts presentation and input; ordinary gameplay belongs to the C# engine.
Original research, runtime implementation, and reference verification have separate authority.

## Current Status

One common `GameSession` runs configurable authored battle/exploration packages and the connected
private Map 3 through Battle 01 world. Content admits reusable immutable definitions and separate
controlled starts. Domain supplies deterministic movement, admitted actions, progression and rules;
Application owns program execution, waits, automatic turns, staged effects and return. Godot submits
semantic input and presents current state. Production has no Reference or verification dependency.

The accepted finite rule-composition scope lets a project author replace admitted action algorithms,
automatic decisions, progression/outcomes and source-story policy in C#, or author conditional typed
programs, then rebuild/start a new session. Refresh/activation/control, movement-cost tables and
field/scene logical services remain fixed source-specific algorithms. The
[architecture route](./docs/architecture.md) explains these boundaries; the
[capability owner](./docs/capability-status.md) records runnable, Unsupported and Unknown behavior.

The private continuous Map 3 through Battle 01 victory and usable-return milestone is accepted under
keyboard A, the modern deterministic clock and composed 8D/H4 semantics. The
[readiness owner](../docs/design/synthesis/map3-battle01-readiness.md#accepted-current-milestone)
records independent acceptance and unchanged historical reports. Natural original reach, hardware 8C,
full-game parity and public distribution remain outside that acceptance. The
[retained evidence route](./docs/evidence/retained-comparisons.md) preserves failed comparisons,
source/provenance and the retired reference implementation without turning them into current run plans.

## Start Here

| Work | Read |
| --- | --- |
| Architecture and state ownership | [Architecture](./docs/architecture.md), [ADR 0019](../docs/decisions/0019-state-and-content-driven-remake-engine.md), and the consumed behavior contract |
| Rule authoring and algorithms | [Domain gameplay rules](./docs/domain/gameplay-rules.md) |
| Session, exploration and story execution | [Application session and programs](./docs/application/session-and-programs.md) |
| Current capability or controlled reference | [Capability status](./docs/capability-status.md), then the named behavior/evidence owner |
| Content and profile admission | [Runtime profiles and trust](docs/content/profiles-and-trust.md) |
| Build, unit tests, reference or adapter observation | [Development and verification](./docs/development-and-verification.md) |
| Godot input and rendering | [Host/presentation](./docs/godot/presentation.md), [audio](./docs/godot/audio.md), [battle scenes](./docs/godot/battle-scenes.md) |
| Local asset preparation | [Content presentation assets](./docs/content/presentation-assets.md) |
| Historical comparisons and remaining Unknowns | [Retained evidence](./docs/evidence/retained-comparisons.md); current test policy is in the verification guide |

## Runtime Profiles

| Profile | Current input boundary | Claim |
| --- | --- | --- |
| `public-authored` | default local start, or `--authored-package <path>`; validated configurable authored battle | connected semantic battle subset; no original start/fidelity or export claim |
| `private-local-controlled-start` | explicit battle start plus selected private inputs, or `--private-exploration-start` plus prepared connected world | common initialized actions; the connected world executes opening, Battle01 and both supported outcomes/returns. Initial party/accounting/seeds and ordinary-defeat egress remain explicit controlled inputs; no original natural-route or fidelity claim |

Profile selection is explicit and content remains validated. Private startup cannot silently fall back
while reporting private success. Authored battles, the standalone initialized private entry and the
connected private world use the common runtime. The legacy `public-synthetic` and `private-local`
profiles were retired with the reference host at M5.

## Local Presentation Asset Preflight

The separate local product-art repository and its manifest/runtime payloads remain private inputs.
Use the existing `sf2tool.remake_assets` checkout/export command only when the owning asset change or
launch needs it. [Presentation and assets](docs/content/presentation-assets.md#local-product-asset-pack)
owns the admitted pack, current identities, candidate derivation, mounting and distribution boundary.
Reuse accepted inputs and the selected Godot installation; a new topic or verification check does not
require another checkout, extraction, SDK, project copy, or asset export.

## Build and Test

Reuse the owning worktree and its configured Python/NuGet caches. Before any .NET command, select the
existing absolute `DOTNET_BIN` and shared `DOTNET_CLI_HOME`, and force
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. The
[locked workflow](./docs/development-and-verification.md#locked-net-workflow) owns exact launch setup.

Add small unit tests of actual engine behavior. Reference comparisons, probes, fixture drivers,
planners, gates, reports and helpers are verification; do not add tests of those tools. Old tests may
migrate or retire by behavior, without count parity, one replacement per deletion, or a green old
aggregate. Needed adapter checks observe the actual running Godot state/input; screenshots are prohibited.

From the repository root, `uv run sf2 verify engine` runs locked restore/build/unit tests of the
dedicated engine project. `uv run sf2 verify adapter` compiles the actual adapter. The
[verification guide](./docs/development-and-verification.md#github-public) owns current CI scopes and
main-gate's required-check configuration boundary.
Documentation-only work uses direct document checks.

## Repository Layout

```text
remake/
  Sf2.Remake.sln                 production projects and Engine.Tests
  src/Sf2.Remake.Domain/         deterministic rules and state transitions
  src/Sf2.Remake.Application/    GameSession, control flow, content ports and observations
  src/Sf2.Remake.Content/        validated input readers and definition construction
  game/                         Godot project, composition, input and presentation
  tests/Sf2.Remake.Engine.Tests/ actual new-engine behavior unit tests
  tests/Shared/                 private-input test selection attribute
  reference/inputs/             external controlled comparison inputs
  docs/                         current implementation, direction, capability and usage owners
  global.json                   pinned SDK
  toolchain.json                pinned official Godot artifacts
```

## Boundaries

- `GameSession` owns logical gameplay mutation. Domain and Application remain independent of Godot,
  machine paths and byte decoding; Content validates external inputs before constructing definitions.
- Gameplay legality follows live state, data and supported rules. Reference case IDs, receipt counts
  and one walkthrough's rounds or character sequence belong to comparison code, not engine predicates.
- Preserve real source-specific rules and explicit unsupported capabilities. Generalization does not
  mean removing provenance, numeric domains, atomicity or meaningful failure reasons.
- Godot submits semantic input and projects state. Reference verification consumes the engine through
  its ordinary API; it cannot become a production dependency or evidence for original-game claims.
- Private payloads, derived content and runtime outputs remain ignored. Public success grants no
  right to distribute original assets.

## Architecture Decisions

The [architecture guide](./docs/architecture.md) routes ADR 0008's engine choice, ADR 0011's state and
assembly boundaries, ADR 0017's lightweight internals, and ADR 0019's adopted migration direction.
Historical slices and review chronology remain in Git and their evidence owners.
