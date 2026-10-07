# ADR 0010: Map 3 to Battle 01 Product Acceptance Profile

- Status: **Accepted**
- Proposal date: 2026-08-14
- Decision date: 2026-08-19; acceptance amendment: 2026-09-19
- Scope: product choices for the first Phase 4 playable milestone
- Accepted profile: `1A + 2A + 3A + 4A + 5B + 6A + 7C + 8D + 9A + 10A`
- User acceptance: **Recorded profile, with the current keyboard-scope correction below controlling 9A**

## Context

[ADR 0008](./0008-godot-csharp-cli-first-remake-tooling.md) accepts Godot 4.7.2 .NET/C# as a
CLI-first prospective implementation baseline. [ADR 0009](./0009-first-phase4-playable-slice.md)
selects one continuous playable milestone from Map 3 through completion of Battle 01 and requires
Research and Design gap closure plus a separate user start action. Neither ADR selects the player
experience, assets, route, acceptance endpoint, or intentional deviations.

The [Map 3 to Battle 01 Readiness Ledger](../design/synthesis/map3-battle01-readiness.md) records those
unresolved product-choice slots and remains **NOT READY** for Phase 4. Accepted contracts close many
local seams, including [Battle Functions Control Flow](../design/contracts/battle-functions-control-flow.md),
but accepted `main` still does not establish the exact admitted state, natural route, complete
natural battle trace, after-battle program effects, or final observable state.

This ADR preserves the bounded alternatives and the Design lane's original recommendation for
decision history. The current keyboard-scope correction controls conflicting 9A wording. A historical
`SELECTED` label or an implemented feature does not establish independent user authorization for
every device, setting or full-route acceptance variant. Other selected profile choices remain in force.

## Decision Boundary

This decision selects product scope classes. It MUST NOT invent original-game facts that
Research has not accepted. Exact scenario values marked `Research-owned exact value required` remain
open blanks for the later continuous-scenario contract and its eventual milestone acceptance.

This decision does not:

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

The matrix is retained as decision history. Only rows marked `SELECTED`, together with the conflict
resolution stated for 3A/7C/8D, are accepted project state.

### 1. Admitted start

| Option | Product definition | Evidence and scope consequence |
| --- | --- | --- |
| **1A — SELECTED** | Start from a **controlled admitted snapshot** whose first observable state is player-controllable Map 3. | Selects the start-mode class only. It is a product admission seam, not a canonical original New/load state. Exact values and provenance remain Research-owned. |
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
| **2A — SELECTED** | Implement the smallest Research-proven natural route from the admitted snapshot to Battle 01. Include only interactions, dialogue, menus, transitions, and backtracking proven mandatory. | Optional NPCs, field-menu pages, item/status/options flows, and unrelated exploration are excluded from this milestone unless Research proves the selected route needs them. |
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
| **3A — SELECTED** | Preserve the Research-proven natural battle admission and before/start-cutscene state/request chronology. Its originally proposed structural-placeholder presentation subclause is superseded by selected 7C/8D for the private local milestone. | Requires natural route, cutscene-effect, and the selected 8D presentation evidence. |
| 3B | Preserve natural battle admission but replace cutscenes with a summary card or skip. | Must be a named intentional deviation with explicit state-equivalence checks; it is not the recommended profile. |
| 3C | Require bounded original rendered cutscene fidelity. | Reopens targeted presentation evidence and private/licensing boundaries. |

A Debug Battle Test path, direct `BattleLoop` injection, or transition that shows only a summary and
does not execute the accepted natural seam cannot satisfy option 3A. Exact before/start program
effects, transition chronology, and the first battle-ready state remain
`Research-owned exact value required`.

The natural-admission and chronology portion of 3A remains selected. Its placeholder presentation
portion is not selected: 7C/8D requires private original assets and the reached presentation semantics,
without reproducing the original renderer or hardware clock. The former 8C-triggered reopening of
[ADR 0005](./0005-remake-value-driven-driver-freeze.md) condition 3 is no longer a milestone
requirement. Concrete gameplay ambiguities still use the existing evidence/admission rules.

### 4. Player agency, action coverage, input trace, and RNG

| Option | Product definition | Evidence and scope consequence |
| --- | --- | --- |
| **4A — SELECTED** | The player controls exploration and every ally turn. Implement every action family used by one later-accepted winning reference trace, plus movement, target selection, confirm, and one cancel/reselect path. | Keeps the first milestone playable and evidence-bounded. The exact reached action set remains Research-owned. |
| 4B | Require the full Attack/Magic/Item/Search-or-Stay and all menu/cancel surfaces whether or not the accepted trace uses them. | Adds conditional action, menu, item, spell, and presentation closures beyond the smallest scenario. |
| 4C | Automate the battle or play a noninteractive demo. | Rejected for this milestone because ADR 0009 requires one continuous playable scenario. |

The accepted H4 reference-path policy uses one declared fixed seed and one recorded sequence of **logical**
inputs and action choices. That policy makes acceptance reproducible; it does not require interactive
players to follow the script or prevent other playthroughs from diverging. Original viable seed, RNG
draws/effects, reached branches and winning trace remain `Research-owned exact value required`.
The modern trace follows the finite-music amendment and requires its own independent review. Physical device
events, frame-exact repeat and original clock alignment are not comparison requirements. Timing that
changes a gameplay result still requires an explicit behavioral contract; it cannot be waived as a
rendering difference.

#### Evidenced gameplay waits (accepted Option A)

The user selected evidenced gameplay Wait semantics for 4A/9A. The shared main RNG and matched-state
rule comparisons remain required; whole-history equality is scoped by the modern music amendment
below. The logical input stream may include genuine player waiting at an eligible consumer only when original evidence
identifies its caller/service, enable state, phase and ordering against other active services.
Neutral controller frames, audio sample duration and a desired seed do not by themselves establish
a Wait. Do not infer a count by working backwards from the expected RNG state.

Distinguish three kinds of progression:

- Source-defined mandatory logical work follows the current state/content and command, including
  required movement, polling preambles, counters and cue work. It is not additional player input.
- Player waiting at an eligible consumer permits evidenced logical service opportunities without
  selecting an action. Interactive players may choose different waits and obtain different results.
- Host display/audio delivery latency adds no logical opportunity or accumulated tick debt. It must
  not erase required source logical work during playback. Finite audio releases its dependent wait
  only after both the admitted logical end and actual player completion are satisfied.

Service order is established per mode and caller, not by a universal global ordering. Under 9A,
compare the same semantic Wait/acknowledgement stream when evaluating supplemental settings. A reveal-only Confirm and
pure delivery delay do not add gameplay opportunities. The [continuous-scenario contract](../design/contracts/map3-battle01-continuous-scenario.md#evidenced-gameplay-waits)
owns admission and command-readiness details. Interactive wait rate and concrete scheduling remain
implementation decisions requiring a bounded contract; this choice does not establish them.

This is accepted product policy, not evidence of implementation conformance or original runtime
reach. Missing caller/phase/audio-end bindings remain Unknown. The existing 260-step plan is not
automatically a conforming Wait trace, and this decision neither promises seed `0x6DC1` nor changes
an observed FAIL into PASS. No split RNG, reseed, outcome waiver or production reference replay is
authorized by this policy.

#### Accepted modern finite-music clock

The user selected a modern deterministic clock for finite music. The binding
[music-wait policy](../design/contracts/music-wait-service.md#accepted-modern-finite-music-policy)
starts generation progress at the semantic music request and advances one step per existing bound
common logical service, independent of entity enablement. Sound progress precedes gate/context
service; the first helper service arms without rescanning, and the helper completes whole groups
of three. The initial private MUSIC_JOIN19 profile ends at modern step505. Actual matching finite
playback completion is a second gate: early delivery skips no work, late delivery adds no work/debt.
The existing whole-PCM completion followed by previous-track restart remains the playback policy.
This explicitly replaces the requirement for an original sound-to-VInt clock binding on this profile.
It establishes no original elapsed time, timer ratio, driver phase or natural service count.

This intentional deviation can change NPC states, shared RNG history, encounter scores/order,
subsequent decisions and final battle resources. The old original whole-history seed/order/winning
trace is retained as a historical diagnostic, not an unchanged golden required of this modern clock.
Matched-state RNG arithmetic, local battle rules, mandatory route/story effects, manual agency,
actual private content consumption and the current keyboard scope remain required. A new modern continuous
winning trace needs independent review; this decision is neither that acceptance nor an H4 PASS.
Preserve completed failures and Unknowns, including HEAL and next-actor discrepancies without
attributing them to this clock absent causal evidence. No production reseed is authorized.
Controlled original seed injection is a separate comparison technique, never natural-continuity
evidence or a production behavior; its feasibility does not authorize a new capture here.

### 5. Observable completion endpoint

| Option | Product definition | Evidence and scope consequence |
| --- | --- | --- |
| 5A | End when the battle controller returns victory result `D4 = 1`. | Explicitly insufficient: it does not prove the after-battle program, return route, or stable observable state. |
| **5B — SELECTED** | End at the first stable player-controllable state after natural victory mutation, after-battle program execution, and return handoff complete. | Selects the endpoint shape while leaving exact values to Research. |
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
| **6A — SELECTED** | No user-facing save, load, checkpoint, or battle suspend in this milestone. Restart returns to the controlled admitted snapshot. | Can be selected now without persistence research. Harness reset/setup is not a save feature. |
| 6B | Session-only checkpoint and resume. | Requires exact checkpoint fields, lifecycle, UI/configuration, and observable restoration checks. |
| 6C | Durable cross-process save/load or battle suspend. | Requires complete scenario-field persistence, failure, storage, and visible-flow closure. |

Under selected 6A, later save support remains a separate milestone.

### 7. Assets, text, licensing, and private inputs

| Option | Product definition | Evidence and scope consequence |
| --- | --- | --- |
| 7A — HISTORICAL RECOMMENDATION, NOT SELECTED | Use project-authored placeholder graphics, text, music, and sound with tracked authorship/provenance and explicit redistribution terms. | Avoids any tracked dependency on private originals while retaining semantic event/resource identities. |
| 7B | Use separately licensed third-party replacements. | Each asset requires an accepted provenance/license record before use or distribution. |
| **7C — SELECTED WITH PRIVATE-LOCAL BOUNDARY** | Use extracted original assets, dialogue, graphics, music, and sound only in the private local milestone/profile. | Original payloads remain ignored local inputs. They are never tracked, uploaded, redistributed, embedded in a public release, or required by public CI. |

Selected 7C authorizes only a private local product profile. It does not grant or imply copyright or
redistribution rights. Original payloads and reference captures remain ignored local inputs with
recorded private provenance; they are never tracked, uploaded, redistributed, embedded in a public
release, or required by public CI. A distributable or public build remains blocked until separately
licensed rights or project-owned/licensed replacements are accepted. Public H4 reporting may expose
only licensing-safe metadata, checks, hashes, and results that do not reconstruct original payloads.

### 8. Visual and audio acceptance tier

| Option | Product definition | Evidence and scope consequence |
| --- | --- | --- |
| 8A — HISTORICAL RECOMMENDATION, NOT SELECTED | Functional state/structure tier: accepted scene/resource/request identities and order, deterministic project-owned layout/screenshot regression, and replacement cue presence. | Does not require original pixel, palette, animation-frame, waveform, chip, DMA, VInt, or timing parity. |
| 8B | Add bounded screenshot, palette, or animation comparison to the original. | Requires targeted Research, private comparison inputs, tolerances, and licensing-safe public results. |
| 8C — SUPERSEDED | Former frame/audio/hardware-exact profile. | Historical evidence gaps remain recorded; this tier no longer gates the milestone. |
| **8D — SELECTED** | Gameplay and presentation semantics: accepted state/results, scene/resource/cue identities, event order, completion and input readiness. | Requires evidence-bound behavioral comparisons and actual remake state/input/presentation observations; no hardware-exact capture or emulation backend. |

### Current acceptance amendment

The user explicitly removed hardware-level precision from this milestone on 2026-09-19. 8D replaces
8C; it does not silently redefine the old tier or select historical 8A's screenshot workflow. This
section controls conflicting 8C/H4 completion requirements in earlier ADRs, research audits,
bootstrap plans, architecture documents, README/status summaries and verification notes. Their bounded
results, missing capabilities and original-game Unknowns retain their original evidence meaning.

The comparison must preserve gameplay rules and results, scenario state, natural route/admission,
turn/action/RNG effects, victory/after-program flow and the controllable 5B endpoint. Presentation must
deliver the reached scene, dialogue, animation and audio cue identities in the accepted semantic order,
with correct completion, acknowledgement, blocking and input-ready behavior. 7C remains the private
original-content requirement; an authored replacement cue does not automatically satisfy it.

Pixel/palette equality, original frame cadence or animation durations, waveform/chip equivalence,
cycle timestamps, VInt/DMA/CRAM/VDP chronology and analog/hardware behavior are **outside this
milestone's acceptance domain**. Do not demand exact capture APIs, a hardware emulation backend or
per-difference waivers for those excluded domains. A timing difference that changes game state,
event order, player choices or completion is still in scope. 9A accessibility modes retain separately
reported state-equivalence and acknowledgement checks under 10A. Future hardware-fidelity work needs
its own explicit product decision; an unresolved hardware research question does not create one.

H4 uses accepted implementation-neutral contracts/fixtures and their original source/ROM provenance.
It checks actual remake state transitions, logical inputs, presentation requests and host completion
observations, including a continuous playable start-to-5B run. Request emission alone cannot prove
that a required cue was consumed or that the player regained control. Use existing debug/state/input
observation mechanisms; screenshots are not Godot acceptance. Verification remains outside production
engine logic, and production behavior must not depend on the Map3 reference trace.

Reuse accepted static facts and bounded original observations. Natural caller/route, winning battle
and return-state gaps remain open where those facts do not establish the claim. Research must name
the missing behavior, required fields/checkpoints and why existing evidence is insufficient; only
then may a separately admitted bounded original observation be required under ADRs 0014/0016.
Static topology alone must not be promoted to observed continuity, and remake output must not become
the original reference. Design owns the exact comparison fields, expected evidence and failure rules.

The old original-reference replay path is **disabled**. Recovering its ledger, validating its runtime
capability, implementing its scenario transport, and adding full 8C capture interfaces are not
prerequisites for this milestone or for H4. Preserve its timeout FAIL, cleanup failure, missing actual
ledger/receipts and launch prohibition. This amendment does not launch, revive, rename or reset that
path or its budget. Main-gate owns its disposition in the milestone Issue, with a separate Issue only
if a concrete removal or replacement implementation needs one. Any genuinely necessary new original
observation needs an independently accepted purpose, evidence method and lineage/budget decision under
ADR 0015; a nominal new task or runner is not a reset.

This scope change reports neither evidence closure nor H4 PASS. Natural continuity, required private
content/provenance, continuous contract/definitions and actual applicable H4 execution still need
independent acceptance.

### 9. Accessibility and platform input mapping

#### Current keyboard scope

The user's 2026-10-04 scope correction removes gamepad B/D from current #534/#437 execution and
mandatory acceptance. The required continuous baseline is **A, default keyboard**. The existing C
capture is a reusable keyboard diagnostic/performance fixture; its existence does not authorize
remapping, swapped Confirm/Cancel, reduced flash or a specific adjustable text speed as additional
full-route gates. This correction controls older four-variant requirements in contracts and runbooks.

| Surface | Requirement basis | Current acceptance role |
| --- | --- | --- |
| Default keyboard A | Current user scope, recorded through main-gate in #534 | Required baseline; all applicable route, rule, resource and consumer obligations still apply |
| Gamepad B/D | User explicitly rejected its assumed inclusion | Excluded from current execution and milestone totals; retain historical reports and existing support |
| C keyboard remapping | Former broad 9A text only; no independently established user selection | Supplemental diagnostic only |
| C swapped Confirm/Cancel | Former broad 9A text only; no independently established user selection | Supplemental diagnostic only |
| C reduced flash | Former broad 9A text only; no independently established user selection | Supplemental diagnostic only |
| C adjustable text / 20 characters per second | Former 9A mentioned adjustable text; it did not establish this specific full-route requirement | Supplemental diagnostic only |
| Fast-text speech Option A | Explicit [#517 user decision](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/517#issuecomment-5953279661) | Remains required at its real behavior boundary: omit skipped speech, preserve existing tails and normal source-specific confirmation |

The default keyboard mapping uses arrows or WASD, Enter/Z for Confirm and Escape/X for Cancel.
Existing remapping, standard-gamepad, swapped-button, reduced-flash and text settings are implemented
capabilities with retained evidence; this scope correction does not remove them. A new required device
or settings cohort needs an explicit product decision and a scale/resource plan.

The comparator declares its fixed current keyboard scope rather than inferring requirements from
provided reports. Missing, malformed or failed A cannot pass. A's remaining required H4 children
still block the milestone even when A equals itself. B/D reports cannot populate current counts or
remaining obligations. Optional C comparisons report their actual FAIL/Unknown and raw differences
separately; current-scope results never claim all-settings equivalence.

The [evidenced gameplay wait policy](#evidenced-gameplay-waits-accepted-option-a) remains unchanged.
Reveal-only Confirm and display duration add no gameplay waits. The separate #517 choice below is
not waived by removing an unsupported full-route settings requirement.

### 10. Intentional-deviation ledger

| Option | Product definition | Evidence and scope consequence |
| --- | --- | --- |
| **10A — SELECTED** | Maintain an explicit expected-deviation ledger as an independent H4 layer. | Each deviation names its owner, rationale, affected observable layer, and expected result. |
| 10B | Allow implementation notes or test exclusions to imply deviations. | Rejected because silence would weaken original-fact and product-choice separation. |

The accepted initial deviation inventory includes:

- controlled admitted snapshot instead of a visible New/load flow;
- optional Map 3 interactions and menus excluded unless the accepted route requires them;
- modern remappable logical input and the accessibility configuration surface;
- no user-facing save/load/checkpoint/suspend in the milestone;
- fixed H4 seed and logical reference trace while interactive play may diverge;
- the modern finite-music clock and declared downstream history differences (4A/9A, layers2–5/7/9/10); and
- engine-native safe behavior outside admitted fixture domains, never mislabeled as original behavior.

Each accepted deviation must appear in the future continuous H4 report even when its expected result
passes. Silence, a missing fixture, or an unavailable private input does not authorize a deviation.

### Accepted fast-text speech omission

The user selected this 9A/10A accessibility deviation on 2026-10-02
([decision record](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/517#issuecomment-5953279661)).
The product owner is the user; main-gate records acceptance, the audio adapter owner maintains the
behavior, and the continuous-scenario owner reports its independent layer-10 result. The rationale
is to support fast reading without replaying speech for skipped character reveals or delaying input
for that omitted speech.

The affected observables are layer-8 speech/confirmation playback, layer-9 text delivery and input,
and layer-10 deviation reporting. Instant text and reveal-all input skip per-character speech for
the characters whose reveal was omitted. Reveal itself preserves already-playing sound tails;
later legitimate cue replacement, including source-defined confirmation replacement, still applies.
Normal source-specific confirmation cues remain required; this does not add cue67 to every Ack.
Reveal-all alone emits no acknowledgement, confirmation cue, service tick, RNG advance or speech wait.
Acknowledgement need not wait for a speech tail to finish.

Report this omission explicitly even when it conforms to the accepted expected behavior. It does
not waive same-semantic-Wait/Ack state equivalence, normal-reveal speech, actual completion,
private-content provenance or unrelated 7C/8D obligations. Product acceptance is not observation
coverage or full H4 PASS; the [continuous contract](../design/contracts/map3-battle01-continuous-scenario.md)
and [audio owner](../../remake/docs/godot/audio.md#accepted-fast-text-speech-policy)
retain the actual evidence boundaries.

Private-only original-asset handling and the prohibition on public distribution are product and
copyright boundaries, not deviations from original fidelity.

## Accepted Profile

The user accepted `1A + 2A + 3A + 4A + 5B + 6A + 7C + 8D + 9A + 10A`.
The Design lane's original 7A/8A recommendation was not selected. The 7C private-local boundary and
8D semantic requirements govern presentation wherever they conflict with 3A's originally proposed
placeholder subclause.

## Selected Choices and Remaining Research

The accepted profile fixes these policy classes without claiming original facts:

- controlled-snapshot versus New/load start class;
- minimum mandatory-route rule and default exclusion of optional content;
- natural transition chronology with the selected private original-fidelity presentation target;
- manual agency breadth and deterministic H4 seed/trace policy;
- stable post-after-program endpoint shape;
- save exclusion or inclusion class;
- private-local original-asset policy with no public redistribution;
- gameplay and presentation-semantic parity tier, excluding hardware exactness;
- modern logical input/accessibility requirements; and
- explicit deviation-ledger policy.

Selecting a class does not fill its Research-owned fields or authorize Phase 4.

## Exact Research-Dependent Blanks for Eventual Milestone Acceptance

The later continuous-scenario contract needs the following evidence. The readiness ledger owns which
bounded fields are already closed; this list does not reopen them:

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
7. 8D reached presentation identities/order, completion and input-ready behavior; private content
   and any required reference-observation provenance; deterministic logical comparison conditions
   and licensing-safe public reports.

Research may split these into coherent evidence owners. This ADR MUST NOT name speculative fixture
IDs or treat an unmerged observation as accepted. After the required owners merge, Design may propose
`docs/design/contracts/map3-battle01-continuous-scenario.md` with its exact fixture and association set
derived from accepted evidence.

## Eventual Milestone Acceptance Consequences

Although accepted, this ADR does not make the scenario ready. The readiness ledger remains
**NOT READY** until Research closures, the continuous-scenario contract, any route-required
conditional contracts, the private-local asset inventory/provenance and no-public-distribution
boundary, complete 8D H4 acceptance definitions, main-gate readiness review, and the separate user
Phase 4 start action are complete for the eventual continuous milestone. Under
[ADR 0016](./0016-remake-start-evidence-deferral.md), these open rows do not block a separately
user-authorized bounded implementation start by default.

Before the continuous milestone is accepted, the continuous H4 acceptance contract must specify
executable check definitions for:

1. admitted-state identity and provenance;
2. logical input and natural exploration route;
3. map/setup/event/program/dialogue/roster/flag transitions;
4. natural Battle 01 admission and encounter state;
5. turn, movement, target, player/AI action, RNG, resolution, replay, and after-turn traces;
6. natural victory and after-battle program/handoff;
7. the product-selected stable endpoint;
8. private-local asset identity/provenance, reached scene/dialogue/animation/audio semantics and
   completion/input-ready observations under 8D, plus save-exclusion and accessibility assertions;
9. every expected deviation as a separately reported layer.

Those definitions must be accepted before the continuous milestone is accepted. Implementing the
adapter and obtaining H4 PASS remain Phase 4 and milestone-acceptance work after the separate user
start action.

## Decision Integration

This acceptance amendment changes product policy and current readiness routing, including the
readiness ledger's Chinese mirror metadata. It changes no executable fixture, research-index
association, runtime implementation or verification gate.

No accepted choice in this ADR starts Godot work, adopts MCP, creates remake code, or authorizes
Phase 4. A separate user authorization under ADR 0016 is required for a bounded implementation start.
