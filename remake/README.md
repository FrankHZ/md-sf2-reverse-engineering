# SF2 Remake

The remake uses reverse-engineered behavior and implementation-neutral contracts to build a modern
tactical engine. Godot hosts presentation and input; ordinary gameplay belongs to the C# engine.
Original research, runtime implementation, and reference verification have separate authority.

## Current Status

One common `GameSession` runs configurable authored battle/exploration packages and the connected
private opening world through Battles01–10 and the [eastern Parmecia handoff](docs/application/new-granseal-east-shrine.md).
Content admits reusable immutable
definitions and separate controlled starts. Domain supplies deterministic movement, admitted actions, progression and rules;
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
[readiness owner](../docs/design/synthesis/milestones/map3-battle01-readiness.md#accepted-current-milestone)
records independent acceptance and unchanged historical reports. Natural original reach, hardware 8C,
full-game parity and public distribution remain outside that acceptance. The
[retained evidence route](./docs/evidence/retained-comparisons.md) preserves failed comparisons,
source/provenance and the retired reference implementation without turning them into current run plans.

The private ordinary opening carries the party through Battles01–07, the Galam escape,
optional Kiwi recruitment, tower aftermath, earthquake town and docks departure. Map73's first-arrival
init reaches usable Map14 control. The [deck, Battle08 and founding route](docs/application/battle08-new-granseal.md)
continues to the New Granseal castle entry. [Peter, Ribble and eastern Parmecia](docs/application/new-granseal-east-shrine.md)
covers two more battles, the cave approach and the separate shrine entrance. The [Dark Caves, Polca and Bedoe route](docs/application/dark-caves-bedoe.md) continues through Battles11–12 to the return confrontation. The [Battle13 and first raft route](docs/application/polca-first-raft.md) continues to Polca’s river landing. Field recipe 4 on gameplay
revision 4 admits eleven members and restores seven-, nine- and ten-member field saves. Earlier
revision1–3 worlds retain their own compatible slots. Explicit Save/Continue supports the existing
opening recipes plus the Battle07 Map58 safe point, year-later castle, Ribble, eastern overworld battle returns and the conditional East Shrine visit. The
[battle persistence boundary](docs/application/battle-save-and-continue.md) also supports explicit
system-menu Save/return or Save and Quit at settled player Movement in Battles01–07, then ordinary
title Continue in a fresh process at that actor's saved turn.
The [campaign agreement](docs/application/pre-voyage-campaign.md) owns preparation, ordinary launch,
continuous input observations, completed failures and remaining acceptance boundaries. Independent
main-gate review and human/aesthetic acceptance are not implied by the local route result.

The [current Godot presentation](./docs/godot/presentation.md) includes continuous exploration/Talk,
a shared 1920×1080 world at 96px per cell, story dialogue, battle movement ranges, HP/MP cards,
battle messages, return feedback and read-only Party Status. These accepted presentation changes retain
their own observation limits; broader #438 UI/UX and final human/aesthetic acceptance remain separate.

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
| Playable UI and opening review | [Pre-voyage scenes and controls](docs/godot/ui-scene-review.md#scenes-in-the-playable-demo), [Opening UI assessment](./docs/evidence/opening-ui-gaps.md) |
| Local asset preparation | [Content presentation assets](./docs/content/presentation-assets.md) |
| Historical comparisons and remaining Unknowns | [Retained evidence](./docs/evidence/retained-comparisons.md); current test policy is in the verification guide |

## Run Locally

Use this repository's existing worktree environment and selected Godot installation. From the
repository root, after the [locked environment setup](./docs/development-and-verification.md#locked-net-workflow):

```powershell
. ./local/private-inputs.ps1
& $env:GODOT_BIN --path remake/game
# Explicit authored diagnostic entry:
& $env:GODOT_BIN --path remake/game -- --authored-package (Resolve-Path -LiteralPath 'remake/content/authored/garden-watch.json').Path
```

These are alternative launches; reuse the owned running instance during development. Private play
requires prepared local inputs; select the
[complete product-art world and opening scene together](docs/content/presentation-assets.md#final-publication-and-ordinary-selection).
For the optional painted Map3 package, follow the
[Map3 HD preparation, matching world/scene selection and Graphics setting](docs/content/asset-inventory.md#rebuild-and-review).
Menu publication alone does not generate that artwork. HD covers Map3's outdoor/interior blocks,
field actors and portraits; other maps retain their installed artwork. Original remains selectable.
The ordinary launch opens the title; New Game needs the six
private battle/static selections, prepared world and optional battle scene content described by the
[New Game input owner](docs/content/profiles-and-trust.md#opening-new-game-admission). Missing content
reports a startup diagnostic. Witch New Game chooses one of two slots, then a protagonist name
(up to seven supported characters; empty keeps the default) and a final Normal Start/save
confirmation. A write succeeds before gameplay replaces the paused session; occupied replacement
is explicit and Cancel retains current progress. At settled field input, New Game… opens the same
flow with Cancel selected. Save… (default Space / Pad LB) opens two local slots at a supported settled
field boundary; use the system Menu during admitted battle Movement. Title Continue restores the
selected journey at its displayed savepoint or battle turn. Configure
absolute `SF2_SAVE_ROOT` outside every checkout and a unique `SF2_SAVE_NAMESPACE` first; there is no
default save folder. See [save/restart usage and limits](docs/verification/host.md#opening-save-and-continue).
Explicit diagnostic play uses a start selection. The
[ordinary-host recipe](./docs/verification/host.md#ordinary-host-startup) and
[Yeel–Battle03 recipe](./docs/verification/field-programs.md#yeel-guide-through-battle03)
own preparation and reproduction; the launch above does not prepare original assets.

In battle, use directions to move within the cyan range. Confirm your stop to open the right-hand
command list. Use Up/Down and Confirm to choose an action. On the board, directions cycle eligible
targets and Confirm executes. Hover previews a target. Clicking that unit executes the action.
Back unwinds the current choice. Cancel from Movement undoes the move and opens free inspection.

In the field, hold a direction to walk and face someone to Talk. At settled field input, Cancel
opens Member / Magic / Item / Search. Use Up/Down to select. E / Pad Start opens Party Status
with the default bindings. Confirm or the Prepare button opens Equip,
Give/exchange, Use, Magic and Drop for the selected member. Selection and Cancel are free;
each change requires its own confirmation. HEAL and ordinary HP recovery items have no field
effect in the original; the menu explains this and keeps their resources. Antidote and DETOX1
use their separately admitted poison-cure rules. On supported early overworld maps, EGRESS returns
to the live refuge, including from enclosed victory positions; confirmation spends up to its MP cost
and keeps gold/progress. See the [field EGRESS boundary](./docs/application/pre-voyage-campaign.md#enclosed-battle02-return-and-field-egress).
Defeated members retain inventory access.
Select a member from the visible roster by clicking a row or using Left/Right. Tab / Pad RB cycles
member, magic and inventory focus. Up/Down selects a member or scrolls the focused detail list.
The [field controls](docs/godot/field-and-shop.md#reading-input-and-return) cover preparation and shop
transactions. For a first session, follow the [short play route](docs/godot/ui-scene-review.md#short-play-route).
For build/test selection continue to [Build and Test](#build-and-test); documentation-only edits do
not require a game launch.

## Runtime Profiles

| Profile | Current input boundary | Claim |
| --- | --- | --- |
| `public-authored` | explicit `--authored-package <path>`; validated configurable authored battle | connected semantic battle subset; no original start/fidelity or export claim |
| `private-local-new-game` | ordinary title → New Game or Continue with admitted private content, no checkpoint | fresh opening party/progress, a confirmed initial save, shared custom protagonist name, supported field savepoints and stable player Movement battle saves through Battle07; [finite campaign scope](docs/application/pre-voyage-campaign.md), no arbitrary interior/animation suspend or full campaign claim |
| `private-local-controlled-start` | explicit battle start plus selected private inputs, or `--private-exploration-start` plus prepared connected world | common initialized actions; selected connected packages cover opening/Battle01, Battle02 outcomes/returns and Yeel–Battle03 play/control at their separate boundaries. Initial party/accounting/seeds and ordinary-defeat egress remain explicit controlled inputs; no original natural-route or fidelity claim |

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
