# ADR 0010: Proposed Map 3 to Battle 01 Product Acceptance Profile

- Status: **Proposed**
- Proposal date: 2026-08-14
- Scope: product choices for the first Phase 4 playable milestone
- User acceptance: **Required; no recommendation in this document is accepted project state**

## Context

[ADR 0008](./0008-godot-csharp-cli-first-remake-tooling.md) accepts Godot 4.7.1 .NET/C# as a
CLI-first prospective implementation baseline. [ADR 0009](./0009-first-phase4-playable-slice.md)
selects one continuous playable milestone from Map 3 through completion of Battle 01 and requires
Research and Design gap closure plus a separate user start action. Neither ADR selects the player
experience, assets, route, acceptance endpoint, or intentional deviations.

The [Map 3 to Battle 01 Readiness Ledger](../design/synthesis/map3-battle01-readiness.md) records those
unresolved product-choice slots and remains **NOT READY** for Phase 4. Accepted contracts close many
local seams, including [Battle Functions Control Flow](../design/contracts/battle-functions-control-flow.md),
but accepted `main` still does not establish the exact admitted state, natural route, complete
natural battle trace, after-battle program effects, or final observable state.

This ADR presents bounded alternatives and a recommended profile for explicit user choice. It does
not silently select the recommendation. Until the status changes to **Accepted** after a user choice,
every `RECOMMENDED` label below means only “the Design lane's proposed default.”

## Decision Boundary

This proposal MAY select product scope classes now. It MUST NOT invent original-game facts that
Research has not accepted. Exact scenario values marked `Research-owned exact value required` remain
blocking blanks for the later continuous-scenario contract.

This proposal does not:

- report the readiness ledger as ready;
- define or register original-game evidence;
- create a research-index association or executable design-contract fixture registration;
- authorize a debug-battle shortcut, direct battle injection, summary-only transition, or automated
  demo as the continuous playable milestone;
- install Godot, create `remake/`, run an MCP bakeoff, adopt an MCP adapter, or start Phase 4;
- publish original dialogue, graphics, music, sound, maps, captures, ROM data, or other private
  payloads.

Ordinary player control must begin in the admitted Map 3 state and continue through a
Research-proven natural route, natural Battle 01 admission, playable battle completion, the natural
after-battle program, and the selected observable endpoint. Controlled setup may establish the
admitted start, but it may not replace the route or battle with helper calls after control begins.

## Accepted Inputs and Their Limits

| Accepted input | What it supplies | What it does not decide here |
| --- | --- | --- |
| [ADR 0008](./0008-godot-csharp-cli-first-remake-tooling.md) | prospective Godot/.NET/CLI boundary | product experience, assets, MCP adoption, or Phase 4 start |
| [ADR 0009](./0009-first-phase4-playable-slice.md) | continuous Map 3-through-Battle 01-completion extent | exact start, route, endpoint, save, UI, assets, or parity tier |
| [Readiness Ledger](../design/synthesis/map3-battle01-readiness.md) | dependency classes, open gaps, H4 layers, and closure order | original evidence, product answers, or readiness approval |
| [Story Progression](../design/synthesis/story-progression.md) and [Gameplay Overview](../design/synthesis/gameplay-overview.md) | bounded handoffs and cross-system vocabulary | a natural campaign chronology or exact scenario route |
| [Battle Functions Control Flow](../design/contracts/battle-functions-control-flow.md) and [Tactical Battle Loop](../design/synthesis/tactical-battle-loop.md) | local player/battle routes and composition boundaries | a complete natural Battle 01 playthrough or product UI |
| [Save System](../design/contracts/save-system.md), [Input System](../design/contracts/input-system.md), and presentation contracts | bounded local service, input, loader, and request seams | milestone save scope, device mapping, accessible UX, or rendered parity tier |

## Choice Matrix

No row in this matrix is accepted by its presence. The user may accept the recommended option,
select another listed option, or request a bounded revision before this ADR becomes **Accepted**.

### 1. Admitted start

| Option | Product definition | Evidence and scope consequence |
| --- | --- | --- |
| **1A — RECOMMENDED** | Start from a **controlled admitted snapshot** whose first observable state is player-controllable Map 3. | Selects the start-mode class only. It is a product admission seam, not a canonical original New/load state. Exact values and provenance remain Research-owned. |
| 1B | Show the visible New flow, including every required naming/configuration/menu handoff, before Map 3. | Requires additional natural-flow, UI, text, presentation, and state evidence plus conditional contracts. |
| 1C | Start by loading a saved game through a visible load flow. | Requires accepted persistence, load UI, failure, and complete scenario-field survival boundaries. |

For 1A, all of the following remain `Research-owned exact value required`: map identity confirmation,
position, facing, party and active roster, combatant stats/status, items/equipment, spells/MP, gold,
story and battle flags, difficulty, RNG state, elapsed-time state, map setup/event/program cursors, and
any other field later proven relevant to the route. The milestone name fixes Map 3 as the admitted map
class; it does not fill the snapshot.

### 2. Mandatory route and optional Map 3 scope

| Option | Product definition | Evidence and scope consequence |
| --- | --- | --- |
| **2A — RECOMMENDED** | Implement the smallest Research-proven natural route from the admitted snapshot to Battle 01. Include only interactions, dialogue, menus, transitions, and backtracking proven mandatory. | Optional NPCs, field-menu pages, item/status/options flows, and unrelated exploration are excluded from this milestone unless Research proves the selected route needs them. |
| 2B | Add a named bounded set of optional exploration or dialogue to the mandatory route. | Every optional step needs accepted route/effect evidence and explicit H4 coverage. |
| 2C | Offer broad/free Map 3 exploration and general menu coverage. | Expands the milestone into map-content, field-menu, dialogue, persistence, and presentation work not closed by the current ledger. |

Option 2A requires ordinary player movement and interaction along the accepted route. Excluding an
optional path is a product scope decision, not evidence that the original path is absent or
unreachable. The exact ordered route, required interactions, dialogue/event programs, flag effects,
menu calls, transition points, and permitted backtracking remain
`Research-owned exact value required`.

### 3. Natural battle admission and cutscene presentation

| Option | Product definition | Evidence and scope consequence |
| --- | --- | --- |
| **3A — RECOMMENDED** | Preserve the Research-proven natural battle admission and before/start-cutscene state/request chronology, presented with project-owned structural placeholders. | Requires natural route and cutscene-effect evidence, but not original pixels, frame timing, or copyrighted payloads. |
| 3B | Preserve natural battle admission but replace cutscenes with a summary card or skip. | Must be a named intentional deviation with explicit state-equivalence checks; it is not the recommended profile. |
| 3C | Require bounded original rendered cutscene fidelity. | Reopens targeted presentation evidence and private/licensing boundaries. |

A Debug Battle Test path, direct `BattleLoop` injection, or transition that shows only a summary and
does not execute the accepted natural seam cannot satisfy option 3A. Exact before/start program
effects, transition chronology, and the first battle-ready state remain
`Research-owned exact value required`.

The structural-placeholder tier is a new proposed product choice.
[ADR 0005](./0005-remake-value-driven-driver-freeze.md) freezes low-value driver/hardware expansion
by default, but it does not choose this tier and MUST NOT be cited as if it had already accepted
placeholder presentation.

### 4. Player agency, action coverage, input trace, and RNG

| Option | Product definition | Evidence and scope consequence |
| --- | --- | --- |
| **4A — RECOMMENDED** | The player controls exploration and every ally turn. Implement every action family used by one later-accepted winning reference trace, plus movement, target selection, confirm, and one cancel/reselect path. | Keeps the first milestone playable and evidence-bounded. The exact reached action set remains Research-owned. |
| 4B | Require the full Attack/Magic/Item/Search-or-Stay and all menu/cancel surfaces whether or not the accepted trace uses them. | Adds conditional action, menu, item, spell, and presentation closures beyond the smallest scenario. |
| 4C | Automate the battle or play a noninteractive demo. | Rejected for this milestone because ADR 0009 requires one continuous playable scenario. |

The proposed H4 reference path uses one declared fixed seed and one recorded sequence of **logical**
inputs and action choices. That policy makes acceptance reproducible; it does not require interactive
players to follow the script or prevent other playthroughs from diverging. Physical device events,
frame-exact repeat, original input cadence, natural RNG timing, the viable seed value, the reached
player/AI/navigation/action/resolution/status branches, and the complete winning trace remain
`Research-owned exact value required`.

### 5. Observable completion endpoint

| Option | Product definition | Evidence and scope consequence |
| --- | --- | --- |
| 5A | End when the battle controller returns victory result `D4 = 1`. | Explicitly insufficient: it does not prove the after-battle program, return route, or stable observable state. |
| **5B — RECOMMENDED** | End at the first stable player-controllable state after natural victory mutation, after-battle program execution, and return handoff complete. | Selects the endpoint shape while leaving exact values to Research. |
| 5C | Continue to a later summary, save, title, or other presentation screen. | Adds new route, UI, persistence, and endpoint evidence not required by the smallest milestone. |

Option 5B's later scenario contract must observe the accepted victory result; completion/unlocked-flag
mutations; after-battle program completion; absence of a pending modal, script, transfer, or battle
controller; input readiness; and the exact scenario-relevant map, location, facing, party, roster,
flags, inventory, stats, spells, gold, and related state. The exact returned map/location and every
state value remain `Research-owned exact value required`. `D4 = 1` or controller return alone never
passes the endpoint gate.

### 6. Save and resume scope

| Option | Product definition | Evidence and scope consequence |
| --- | --- | --- |
| **6A — RECOMMENDED** | No user-facing save, load, checkpoint, or battle suspend in this milestone. Restart returns to the controlled admitted snapshot. | Can be selected now without persistence research. Harness reset/setup is not a save feature. |
| 6B | Session-only checkpoint and resume. | Requires exact checkpoint fields, lifecycle, UI/configuration, and observable restoration checks. |
| 6C | Durable cross-process save/load or battle suspend. | Requires complete scenario-field persistence, failure, storage, and visible-flow closure. |

Later save support remains a separate milestone if 6A is accepted.

### 7. Assets, text, licensing, and private inputs

| Option | Product definition | Evidence and scope consequence |
| --- | --- | --- |
| **7A — RECOMMENDED** | Use project-authored placeholder graphics, text, music, and sound with tracked authorship/provenance and explicit redistribution terms. | Avoids any tracked dependency on private originals while retaining semantic event/resource identities. |
| 7B | Use separately licensed third-party replacements. | Each asset requires an accepted provenance/license record before use or distribution. |
| 7C | Use extracted original assets, dialogue, or audio in the public milestone. | Rejected under the repository's copyright/private-input boundary. |

Required dialogue uses project-authored semantic placeholder text keyed to accepted event identities,
not copied or lightly transformed original prose. Private original payloads and emulator captures may
support lawful local comparison only; they are never tracked, redistributed, or required by public
CI/H4. The exact replacement inventory, authorship/license identifiers, and file-level provenance
remain a product-owned closure before readiness.

### 8. Visual and audio acceptance tier

| Option | Product definition | Evidence and scope consequence |
| --- | --- | --- |
| **8A — RECOMMENDED** | Functional state/structure tier: accepted scene/resource/request identities and order, deterministic project-owned layout/screenshot regression, and replacement cue presence. | Does not require original pixel, palette, animation-frame, waveform, chip, DMA, VInt, or timing parity. |
| 8B | Add bounded screenshot, palette, or animation comparison to the original. | Requires targeted Research, private comparison inputs, tolerances, and licensing-safe public results. |
| 8C | Require frame/audio/hardware-exact parity. | Conflicts with the smallest value-driven milestone unless separately justified and accepted. |

Under 8A, screenshots compare the remake's own project-authored presentation to a tracked
project-owned baseline; they are not original-game screenshots. Audio acceptance observes replacement
cue identity and accepted request order, not waveform or timing. Options 8B/8C are not implicitly
excluded forever, but selecting either reopens the corresponding Research and acceptance boundary.

### 9. Accessibility and platform input mapping

| Option | Product definition | Evidence and scope consequence |
| --- | --- | --- |
| **9A — RECOMMENDED** | Use remappable logical actions for keyboard and standard gamepad, configurable confirm/cancel convention, reduced-flash mode, and instant/adjustable text progression. | Modern product interface; automated checks observe logical behavior rather than original device scancodes or frame cadence. |
| 9B | Fix a Genesis-style mapping and repeat model. | Requires unnecessary hardware/timing fidelity and reduces accessibility. |
| 9C | Support one fixed keyboard mapping only. | Smaller adapter surface but not the recommended desktop product baseline. |

The proposed default mapping is arrow keys or WASD plus Enter/Z for Confirm and Escape/X for Cancel;
gamepad uses D-pad or left stick plus south-button Confirm and east-button Cancel. Every binding is
remappable, including confirm/cancel convention. This is proposed product metadata, not original
behavior.

If 9A is accepted, the configuration interface MAY be an external versioned settings file or launch
configuration rather than an in-game Options screen. It MUST expose binding identity, confirm/cancel
convention, reduced-flash state, and text-progression mode. The future automated acceptance surface
MUST check:

1. keyboard and gamepad bindings produce the same logical route decision;
2. swapped Confirm/Cancel bindings preserve their configured semantic roles;
3. reduced-flash mode reaches the same gameplay completion event without the suppressed flash cue;
4. instant or adjusted text progression preserves acknowledgement and route results; and
5. device mapping and repeat timing are absent from original-fidelity assertions.

The exact settings serialization and UI are implementation choices for Phase 4, but omission of the
configuration surface or these observable checks is not implementation discretion if 9A is accepted.

### 10. Intentional-deviation ledger

| Option | Product definition | Evidence and scope consequence |
| --- | --- | --- |
| **10A — RECOMMENDED** | Maintain an explicit expected-deviation ledger as an independent H4 layer. | Each deviation names its owner, rationale, affected observable layer, and expected result. |
| 10B | Allow implementation notes or test exclusions to imply deviations. | Rejected because silence would weaken original-fact and product-choice separation. |

If the recommended profile is accepted, the initial deviation inventory includes:

- controlled admitted snapshot instead of a visible New/load flow;
- optional Map 3 interactions and menus excluded unless the accepted route requires them;
- project-authored/licensed placeholder graphics, text, music, and sound instead of private originals;
- structural/state presentation rather than original pixel/frame/waveform/hardware timing parity;
- modern remappable logical input and the accessibility configuration surface;
- no user-facing save/load/checkpoint/suspend in the milestone;
- fixed H4 seed and logical reference trace while interactive play may diverge; and
- engine-native safe behavior outside admitted fixture domains, never mislabeled as original behavior.

Each accepted deviation must appear in the future continuous H4 report even when its expected result
passes. Silence, a missing fixture, or an unavailable private input does not authorize a deviation.

## Proposed Recommended Profile

For explicit user review, the Design lane recommends the combined profile
`1A + 2A + 3A + 4A + 5B + 6A + 7A + 8A + 9A + 10A`.

This line is not a decision. It becomes accepted project state only if the user explicitly selects it
and this ADR's status is changed to **Accepted**. A different selection must update the consequences,
Research dependencies, H4 layers, and deviation inventory before acceptance.

## Choices That Can Be Fixed Before Research Completes

The user can select these policy classes now without claiming original facts:

- controlled-snapshot versus New/load start class;
- minimum mandatory-route rule and default exclusion of optional content;
- natural transition with structural placeholders versus a named cutscene deviation or original
  presentation target;
- manual agency breadth and deterministic H4 seed/trace policy;
- stable post-after-program endpoint shape;
- save exclusion or inclusion class;
- project-authored/licensed asset policy and private-input boundary;
- structural versus original visual/audio parity tier;
- modern logical input/accessibility requirements; and
- explicit deviation-ledger policy.

Selecting a class does not fill its Research-owned fields.

## Exact Research-Dependent Blanks

All of the following block the later continuous-scenario contract and remain open on accepted `main`:

1. the controlled admitted snapshot's exact values and provenance;
2. the exact natural Map 3 route, ordered player inputs, mandatory interactions, dialogue/event/menu
   calls, flags, state effects, transitions, and permitted backtracking;
3. natural Battle 01 admission, before/start cutscene chronology and effects, and first battle-ready
   state;
4. one complete playable multi-round victory trace, including every reached player, AI, navigation,
   action, resolution, replay, reward, and status branch;
5. the viable declared H4 seed and complete logical input/action trace under the accepted admission
   state;
6. natural victory, the after-battle program and its effects, return routing, and exact endpoint
   map/location/state values; and
7. original rendered/audio evidence and tolerances only if the user selects tier 8B or 8C instead of
   the recommended 8A.

Research may split these into coherent evidence owners. This ADR MUST NOT name speculative fixture
IDs or treat an unmerged observation as accepted. After the required owners merge, Design may propose
`docs/design/contracts/map3-battle01-continuous-scenario.md` with its exact fixture and association set
derived from accepted evidence.

## Pre-Phase-4 Acceptance Consequences

Even after user acceptance, this ADR will not make the scenario ready. The readiness ledger remains
**NOT READY** until Research closures, the continuous-scenario contract, any route-required
conditional contracts, licensed/project-owned asset inventory, complete H4 acceptance definitions,
main-gate readiness review, and the separate user Phase 4 start action are complete.

Before Phase 4, the continuous H4 acceptance contract must specify executable check definitions for:

1. admitted-state identity and provenance;
2. logical input and natural exploration route;
3. map/setup/event/program/dialogue/roster/flag transitions;
4. natural Battle 01 admission and encounter state;
5. turn, movement, target, player/AI action, RNG, resolution, replay, and after-turn traces;
6. natural victory and after-battle program/handoff;
7. the product-selected stable endpoint;
8. selected asset, structural presentation, audio-cue, save-exclusion, and accessibility assertions;
9. every expected deviation as a separately reported layer.

Those definitions must be accepted before Phase 4. Implementing the adapter and obtaining H4 PASS
remain Phase 4 work after the separate user start action.

## Proposed Ownership and Integration

The first Draft phase owns only this document and keeps Status **Proposed**. It has no executable
fixture, research-index association, translation-manifest entry, or registry dependency.

Only after explicit user choice may a later accepted update:

1. change this ADR to **Accepted** with the exact selected options;
2. minimally update the readiness ledger to link the chosen product profile while retaining every
   Research/scenario/H4 gap and its overall **NOT READY** status; and
3. add this ADR to the decision index in `docs/README.md`.

No accepted choice in this ADR starts Godot work, adopts MCP, creates remake code, or authorizes
Phase 4.
