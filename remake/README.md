# SF2 Remake

The remake uses reverse-engineered behavior and implementation-neutral contracts to build a modern
tactical engine. Godot hosts presentation and input; ordinary gameplay belongs to the C# engine.
Original research, runtime implementation, and reference verification have separate authority.

## Current Status

The production Domain/Application/Content path runs configurable project-authored battle packages through
one common session. Format-v5 packages separate immutable definitions/deployments from explicit
per-session resources, counters, seeds and optional controlled placement overrides. Deployments own
explicit faction and stable processing order; queue identity and side checks no longer infer either
from a source slot. Numerical agility and explicit extra-round-action eligibility are separate fields in actor
definitions, consumed by the same turn calculator and queue. Physical definitions select explicit
critical probability/bonus pairs consumed by player, enemy and counter hits through one rule owner. The same admitted
definition can start independent sessions through the common validated entry. This
[content-model modernization](../docs/decisions/0019-state-and-content-driven-remake-engine.md#authored-definitions-and-explicit-session-starts)
precedes private initialized-entry work; further source-semantic mappings remain separate slices.
Godot hosts real movement/action input and projects semantic results. The old
public-synthetic/private-local Map 3 routes are retained in a separate
[transitional reference assembly](./reference/README.md); their fixed admission, endpoint handlers and
adapter scheduling remain explicit legacy limitations in the [audit](./docs/architecture-audit.md).

[ADR 0019](../docs/decisions/0019-state-and-content-driven-remake-engine.md) is the adopted direction
to common commands, live state and configurable typed content, resumable programs, and separate
reference runners. M1 provides typed content admission, provisional movement/cancellation, atomic
HEAL/STAY and automatic next-actor/round progression with carried RNG. The
[M2 physical capability](../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m2-ordinary-physical-implementation)
adds ordinary first/second/counter attacks, EXP/gold, death accounting and continuation. A bounded
configured commandset06/script3 enemy decision scores competing targets, applies source class/movement
ties, and uses the same physical action and publication mechanisms, including ally counter rewards
and carried thinking/main RNG. Required class data is validated at the reached comparison; unsupported
level-up, leader and terminal settlement branches reject atomically. With no attack target, the
empty spell/item branch continues through failed HEAL1/SUPPORT to MOVE1, preserving RNG and resources
and ending that turn even when movement resolves to origin Stay. Incomplete or high target costs
remain Unsupported. The
[current boundary](../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m1-implementation)
records supported behavior, responsibility directories and remaining private/program migration.

The [capability matrix](./docs/capability-status.md) owns runnable support and Unknowns. The
[Map 3 implementation/reference record](./docs/map03-playability-plan.md) owns existing controlled
routes, inputs and their limits; it is not the next-feature queue. ADR 0009's continuous Map 3 through
completed Battle 01 remains the eventual reference milestone. The accepted 8C/H4 target remains
incomplete, including natural continuity and original presentation. Engine migration does not waive it.

## Start Here

| Work | Read |
| --- | --- |
| Engine behavior or architecture | [Architecture](./docs/architecture.md), [ADR 0019](../docs/decisions/0019-state-and-content-driven-remake-engine.md), and the consumed behavior contract |
| Current capability or controlled reference | [Capability status](./docs/capability-status.md), then the named Map 3/reference owner |
| Content and profile admission | [Runtime profiles and trust](./docs/runtime-profiles-and-trust.md) |
| Build, unit tests, reference or adapter observation | [Development and verification](./docs/development-and-verification.md) |
| Godot presentation or local asset work | [Presentation and assets](./docs/presentation-and-assets.md) |
| Old tests during migration | [Test audit scope](./docs/test-suite-audit.md#current-repair-status) and ADR 0019's migration table |

## Runtime Profiles

| Profile | Current input boundary | Claim |
| --- | --- | --- |
| `public-authored` | default local start, or `--authored-package <path>`; validated configurable authored battle | connected semantic battle subset; no original start/fidelity or export claim |
| `public-synthetic` | explicit legacy selection; tracked project-authored fixed package and placeholders | redistribution-safe bounded implementation and export smoke; no original fidelity |
| `private-local` | explicit ignored canonical input; optional selected battle inputs and reviewed local art | bounded controlled original-data consumers and diagnostics; incomplete original fidelity |

Profile selection is explicit and content remains validated. Private startup cannot silently fall back
while reporting private success. Authored battles use the common runtime; original private import and
program support remain in the explicit legacy route until their corresponding migration slices.

## Local Presentation Asset Preflight

The separate local product-art repository and its manifest/runtime payloads remain private inputs.
Use the existing `sf2tool.remake_assets` checkout/export command only when the owning asset change or
launch needs it. [Presentation and assets](./docs/presentation-and-assets.md#local-product-asset-pack)
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
main-gate's required-check configuration boundary. The legacy solution is no longer the engine gate.
Documentation-only work uses direct document checks.

## Repository Layout

```text
remake/
  Sf2.Remake.sln                 current solution, including legacy test projects
  src/Sf2.Remake.Domain/         deterministic rules and state transitions
  src/Sf2.Remake.Application/    GameSession, control flow, content ports and observations
  src/Sf2.Remake.Content/        validated input readers and definition construction
  game/                         Godot project, composition, input and presentation
  tests/Sf2.Remake.Engine.Tests/ actual new-engine behavior unit tests
  tests/                        other legacy/reference consumers
  docs/                         current implementation, direction, capability and usage owners
  global.json                   pinned SDK
  toolchain.json                pinned official Godot artifacts and process bounds
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
