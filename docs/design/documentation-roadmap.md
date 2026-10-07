# Documentation Roadmap and Governance Boundaries

- Status: **Confirmed repository governance guidance**; this document is not evidence about the
  original game and does not select a remake engine, product, platform, or commercial direction.
- Scope: organize sourced contracts into concise player-facing explanations without changing their
  evidence labels; distinguish original evidence, accepted remake choices and future work at their
  owning decision and acceptance boundaries.

## Authoring Language Policy

**Confirmed repository policy:** during the current design-synthesis phase, English is the canonical
authoring and review language for new or materially revised design-synthesis documents. Preserve
source-faithful identifiers, fixture IDs, evidence labels, and code vocabulary rather than inventing
translated equivalents.

**Confirmed repository decision (2026-08-04):** the project glossary is accepted at
[`glossary.md`](./glossary.md). It is the single binding source for English-to-Chinese terminology
in design-synthesis documents and governs the zh-CN mirror conventions under
`docs/design/zh-CN/`. Do not create ad hoc bilingual terminology; use the glossary's fixed terms and
rules (evidence-label translations, preserved source identifiers, one-term-one-translation, proper
nouns kept in English with suggested annotations, and entry revision process).

Non-English localization from the canonical English source proceeds as a dedicated batch per the
glossary's rules, with terminology consistency, link integrity, evidence-label preservation, and
fixture-trace QA. A zh-CN mirror under `docs/design/zh-CN/` is a derivative; the English source
remains the review baseline unless a localization batch defines another explicit policy.

**Confirmed current sequencing decision:** Chinese synchronization under
[the design-documentation epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514)
is deferred until the selected theme's English source is stable. Stability means its substantive
English revisions have been independently accepted and merged, and no planned rewrite remains for
that theme in this round. Main-gate records that readiness before dispatching a separately scoped
translation batch. [#434](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/434) remains
deferred; completing the baseline calibration alone does not dispatch it or establish stability for
all themes. Chinese translation and continuing synchronization remain eventual epic deliverables.

During this English round, preserve existing Chinese mirrors, glossary and translation metadata,
including known drift and retained diagnostic failures. Do not refresh source anchors to conceal
drift or claim global translation PASS. A later translation batch must translate and review the
substantive content before using the existing source-anchoring checks.

## Design Information Architecture

**Confirmed repository policy:** `docs/design/` uses ownership and evidence role, rather than game
feature alone, as its first-level information architecture:

- `contracts/` contains evidence-bound subsystem contracts. These remain owned by the research slice
  when accepted findings change.
- `synthesis/` contains cross-subsystem or player-facing explanations that consume accepted evidence
  from `main` and remain owned by the design-synthesis lane.
- repository-governance documents, including this roadmap and `glossary.md`, remain at the design
  root because they govern both categories.
- `zh-CN/` preserves the canonical English relative hierarchy: for example,
  `contracts/save-system.md` maps to `zh-CN/contracts/save-system.md`.

This split is an ownership boundary, not a claim about original-game architecture and not a future
engine module layout. Add deeper subdirectories only when a concrete slice establishes a durable
navigation or ownership need; do not create empty category scaffolding.

## Three-Layer Boundary

| Layer | Ownership and allowed content | Prohibited shortcut |
| --- | --- | --- |
| A. Original behavior/data contracts | `docs/research/`, `schemas/`, manifests, H2/H3 fixtures, and sourced design contracts record **Confirmed**, **Inferred**, and **Unknown** facts. | A source macro, field, or symbol name does not automatically establish player-visible meaning. Preserve the source label and explain only interpretations supported by evidence. |
| B. Reconstructed design explanation | `docs/design/` may explain player-facing consequences supported by Layer A, link the local research/fixture owner, and mark gaps as **Unknown**. | Do not promote a static call, address, or plausible reading into original behavior, a campaign conclusion, or a player-experience fact. |
| C. Future remake decisions | Modernization, implementation intent, and product choices belong in explicit decisions and separate expected-deviation/H4 acceptance boundaries. | Do not rewrite modernization as original behavior or use a synthesis document to choose an engine or product direction. |

**Confirmed repository rule:** Layer B is a traceable interpretation of accepted Layer A evidence,
not a second evidence system. **Inferred** interpretations retain that label, and **Unknown** behavior
remains a question rather than being completed as narrative.

## Pre-Synthesis Evidence Review

**Confirmed repository rule:** every Layer B synthesis slice must adversarially review the Layer A
evidence it will explain. A link to an accepted document is necessary but not sufficient. The review
must inspect, where present, the owning research prose, evidence-bound design contract, executable
fixture payload and exact fixture ID, schema/verifier or focused test, and the narrow H2/H3 command
that owns the claimed quantity, unit, order, or state transition.

The review must specifically test for stale question queues, summary prose that is broader than its
fixture, units reused across different lifecycle stages, source-static call order described as a
runtime outcome, and controlled validation seams presented as natural campaign behavior. Record the
surfaces checked and the disposition in the synthesis document or its review record.

When owners disagree, Layer B must not select the convenient answer or silently repair Layer A.
Exclude the disputed conclusion or retain it as **Unknown**, report the exact mismatch to the owning
research lane, and wait for an accepted owner correction before expanding the synthesis. A stale
queue or over-broad summary may be nonblocking only when the executable owner and the stricter claim
boundary agree; the synthesis must use that stricter boundary and keep the discrepancy visible to
reviewers.

## Current Baseline and Near-Term Synthesis

**Current scope agreement — Standard.** Several document owners and later design batches depend on
a consistent entry baseline. The reader outcome is a traceable explanation of how play moves through
exploration, interaction/story, battle admission, actions/results and return, with original evidence,
accepted remake choices, historical results and remaining Unknowns clearly distinguished. Follow the
[Product Constraint Workflow](../operations/github-project-governance.md#product-constraint-workflow);
Issues/Project coordinate execution, while this roadmap owns the current agreement.

The finite English entry-plus-overview round has two accepted outcomes:

1. [Baseline calibration #667](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/667):
   align this roadmap and the English Design index with the accepted private milestone; establish
   scope, sequencing and translation deferral. Evidence contracts, readiness, implementation owners,
   Chinese files, glossary, translation index and agent guidance remain read-only for this outcome.
2. [Overview revision #669](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/669): the
   user-accepted [Gameplay Overview](synthesis/gameplay-overview.md) provides a readable explanation of
   connected gameplay directions, using the accepted exploration → interaction/story → battle
   admission → actions/results → return slice as a concrete demonstration. It preserves original
   facts, accepted remake choices and uncertainty, and links numerical/evidence tables rather than
   duplicating them. Main-gate completed technical/editorial review, and the user personally accepted
   the English overview and authorized its merge. #669 is complete; subsequent research/documentation
   tickets still require separate planning and dispatch.

With the overview accepted, main-gate may separately plan research/documentation tickets by direction.
Tactical-loop, progression/resource and story/persistence revisions are subsequent candidates,
requiring separate scope and dispatch; they are not automatic work in this round. Further map and
roster explanations remain subject to the entry criteria below.
[#638](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/638) remains user-paused.
[#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438) and
[#617](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/617) retain separate product and
architecture outcomes. This round selects no new art/UI/UX direction and adds no original research,
engine implementation, capture, schema, evidence ledger or validation machinery.

The planning allowance is **4 agent-hours cumulatively** for entry calibration, overview and their
reviews, provisional rather than a delivery guarantee. Baseline calibration and overview each have
a provisional **90 minutes execution plus 30 minutes independent review**, including corrections;
record actual active effort and uncertainty in their handoffs. Each uses at most **5 MiB
of local text-only scratch** and no new runtime data. Stop affected expansion and report for replanning
if broader contract changes, evidence disagreements, new product choices, new validation machinery
or a budget overrun become necessary; a new slice does not reset the cumulative allowance.

Baseline acceptance directly compares proposed prose with the
[accepted milestone](synthesis/map3-battle01-readiness.md#accepted-current-milestone) and
[verification composition owner](../../remake/docs/development-and-verification.md#accepted-composition-review).
Read the complete diff, check changed links/anchors, run `git diff --check` and `git diff --name-only`,
and confirm exact ownership and the private-content boundary. This documentation-only outcome runs
no emulator, Python/.NET/Godot/H3 suites, comparison rebuilds, translation checker or new tests.
Freeze the committed/pushed Draft PR with actual CI state for independent main-gate review; passing
these direct checks does not independently accept the milestone or close the whole documentation epic.

Overview acceptance also reads the full resulting explanation as a reader and checks changed original
claims against their contract/research, pertinent fixture payload/identifier and verifier semantics.
Check changed links/anchors and any diagram's syntax and transition meaning, exact two-file ownership
and private boundary directly. No runtime/native/translation suites or new verification machinery
are added. The #669 independent-review and user-acceptance gates are satisfied. For a separately scoped
material revision, freeze its Draft PR for independent review and any required user acceptance;
passing direct checks or CI alone does not satisfy a user gate.

**Confirmed repository baseline:** existing contracts cover combat, maps, level-up, spells, services,
save/input/window, dialogue, party/roster state, and randomness. They are listed in the
[design index](../README.md#design) and trace back to research and fixture owners. This roadmap does
not merge or replace those contracts.

The following bounded explanations already exist. The ordering is an **Inferred reading priority**,
not a completion claim for their wider subjects. Extensions still require stable accepted evidence
owners and must preserve each document's Unknowns.

| Order | Existing document | Scope and prerequisites | Existing contract/evidence links | Non-goal / stop condition |
| ---: | --- | --- | --- | --- |
| 1 | [Gameplay Overview](synthesis/gameplay-overview.md) | Explain currently supported player actions, state boundaries, and major subsystem handoffs, beginning only from accepted map, dialogue, roster, service, and input facts. | [map exploration](contracts/map-exploration.md), [dialogue](contracts/dialogue-system.md), [party/roster](contracts/party-roster-state.md), [services](contracts/service-interactions.md), [map-script fixture](../../tests/fixtures/h2/map-script-engine-static-v1.json) | Do not promise a complete campaign flow, interface feel, or narrative experience; these remain **Unknown**. |
| 2 | [Tactical Battle Loop](synthesis/tactical-battle-loop.md) | Explain the bounded order from player input/control through battle action, resolution, state replay, and known outcomes while retaining every unresolved branch; requires accepted battle-loop/action/AI research and combat/spell contracts. | [battle-loop research](../research/battle-loop.md), [battle-actions research](../research/battle-actions.md), [combat](contracts/combat-resolution.md), [spell resolution](contracts/spell-resolution.md), [physical-damage fixture](../../tests/fixtures/h3/physical-damage-v1.json) | Do not invent tactics, balance intent, target-selection meaning, or a general simulation from isolated cases. |
| 3 | [Progression and Economy](synthesis/progression-and-economy.md) | Connect growth, EXP/gold/item, and service boundaries into a resource-flow explanation only where inputs, outputs, order, and persistence have evidence. | [ally-growth research](../research/ally-growth.md), [common-stats research](../research/common-stats.md), [level-up](contracts/level-up.md), [services](contracts/service-interactions.md), [level-up fixture](../../tests/fixtures/h3/level-up-boundaries-v1.json) | Do not claim an intended difficulty curve, intended prices, an optimal build, or a long-term economy. |
| 4 | [Story Progression](synthesis/story-progression.md) | Explain Confirmed state/route/dialogue/roster boundaries as a traceable progression map while retaining normal-story reachability and presentation labels; these least-stable dependencies place it after the preceding documents. | [gameflow research](../research/gameflow-core.md), [common-scripting research](../research/common-scripting.md), [dialogue](contracts/dialogue-system.md), [party/roster](contracts/party-roster-state.md), [dialogue runtime fixture](../../tests/fixtures/h3/map-script-dialogue-v1.json) | Do not reconstruct plot beats, player-choice consequences, or a complete story route from source labels or isolated program references. |

This order establishes reader navigation first, then covers the most bounded tactical loop, connected
resource flows, and finally story explanation that depends on reachability. An extension waits when
an active slice is revising its owner contract or when its required answers remain **Unknown**.

[Map Design Principles](synthesis/map-design-principles.md) also exists as a bounded structural
explanation; it does not establish player-route quality or authorial intent. The
[Map 3 to Battle 01 readiness ledger](synthesis/map3-battle01-readiness.md#accepted-current-milestone)
owns the **accepted current private Map 3 → Battle 01 victory → usable 5B return milestone**.
Acceptance composes existing executed continuous-route/return evidence and independently reviewed
source/consumer proofs under default keyboard A, the modern deterministic clock and the 7C/8D/10A
boundary. C is supplemental; gamepad B/D are excluded. The
[verification composition owner](../../remake/docs/development-and-verification.md#accepted-composition-review)
retains exact dependencies and reproduction routes. This accepts gameplay and presentation semantics,
including actual consumers, completion and input readiness, without asserting hardware equality,
full-game parity or public-distribution rights.

Original observations retain their own [bounded frontier](synthesis/map3-battle01-readiness.md#accepted-original-frontier).
Historical A10/default/matrix reports remain Unavailable with `milestonePass=false`; scoped outputs
also remain false. No fresh full executable modern report or corrected whole-A trajectory was
produced. Historical seed-latch, HEAL timing, provenance and review failures remain preserved.
Terminal internal completion remains **Inferred**, its delay **Unknown**, and omitted historical
turn-generation operands and original timing retain their owner-defined limits. These scientific
limits do not reopen accepted compositions or create an automatic runtime queue; further evidence
work requires a concrete new defect.

The [Phase 4 bootstrap plan](synthesis/phase4-bootstrap-plan.md) remains a historical pre-start
proposal. Current engine direction and implemented/unsupported capabilities belong to
[ADR 0019](../decisions/0019-state-and-content-driven-remake-engine.md) and the
[remake capability ledger](../../remake/docs/capability-status.md); implementation progress is not
original-game evidence.

## Long-Term Directions

The following broader explanations or extensions remain **Unknown future directions**, not current
commitments. Work may begin only when entry criteria cite accepted local evidence; none authorizes a
new engine design or treats remake functionality as original-game evidence.

| Direction | Entry criteria and evidence dependencies | Non-goal |
| --- | --- | --- |
| experiential extension of map-design principles | The existing structural synthesis is the baseline; additional reachability and interaction-outcome observations must distinguish layout facts from player-route interpretation. | Do not infer authorial intent or redesign levels from 64x64 layout data alone. |
| player roster choice space | Accepted roster, class/promotion, growth, equipment, battle-party, and persistence/capacity boundaries; unresolved lifecycle limits remain visible. | Do not publish a tier list, “best party” advice, or assumed player preferences. |
| player/enemy numerical curves | Complete source-backed numeric tables plus runtime-confirmed application, caps, and level/encounter context sufficient to name units and boundaries. | Do not set remake balance targets or describe mathematical curves as intended difficulty. |
| battle simulation | Complete and mutually compatible battle-loop/action/AI/pathfinding/state contracts plus a bounded H4 adapter acceptance surface. | Do not select a simulation architecture, claim general predictive accuracy, or use a model to fill unresolved branches. |

## Reusable Authoring Structure

Future `docs/design/` synthesis documents may selectively use the following structure. This describes
document shape, not a parallel workspace or a mandatory full GDD template.

**Confirmed authoring guidance:** organize design documents by coherent subject matter, reader flow
and maintainable ownership. Include the explanation needed to make the bounded subject understandable;
do not impose line, word or page caps, split documents solely for length, or remove necessary
explanation to meet a brevity target. The handwritten-source line limit does not apply to design
Markdown. Effort budgets bound work expansion, not document length.

1. **Audience and judgment boundary.** Identify the reader—researcher, fidelity implementer, or
   player-facing explainer—and the supported and unsupported judgments. Original-game claims retain
   **Confirmed**, **Inferred**, or **Unknown** at the source-owner layer.
2. **Player verbs and action-goal alignment.** Begin with evidenced inputs, state changes, and
   outcomes. Keep original source labels separate from neutral player-action phrases. A player goal
   or meaning without local evidence is **Inferred** or **Unknown**.
3. **Loops, state flow, and system dynamics.** Diagram only ordered transitions, resources, and
   feedback relationships with evidence owners. Retain unobserved branches and do not present a
   control-flow graph as engine architecture.
4. **Evidence matrix.** Every substantive entry includes its label, bounded claim, source/research
   owner, contract, fixture ID/path when applicable, and remaining question. Local links such as
   [runtime RNG and battle math](../research/runtime-rng-and-battle-math.md),
   [combat fixture](../../tests/fixtures/h3/physical-damage-v1.json), and
   [combat contract](contracts/combat-resolution.md) are the canonical trace; do not copy another evidence
   ledger.
5. **Original fidelity and modernization.** State the original-fidelity rule first, then mark a
   deliberate deviation as a future decision with a separate expected-deviation fixture. In the
   absence of a decision, do not imply modernization.
6. **H4 acceptance, expansion, and stop conditions.** List adapter-visible parity facts, fixture
   consumers, and the evidence required for expansion. Stop when a gap is a runtime, reachability,
   presentation, or product question rather than silently expanding the contract.

## External Reference Provenance and Selective Adoption

**Confirmed external-reference provenance:** the
[DY-2026/GameDesignOS README](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/README.md),
[contract catalog](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/contracts/README.md), and
[player-promise contract schema](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/contracts/player-promise-contract.schema.json)
were accessed on 2026-08-01 at pinned `main` commit
`d01dfebc6eac7a619b9a18f3cbafa51270d1edba`; the repository uses the
[MIT license](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/LICENSE).
The reproduction command `git ls-remote https://github.com/DY-2026/GameDesignOS.git` observed that
commit at `refs/heads/main`, and requests for each listed pinned raw document/template returned HTTP
200.

The following structural prompts were selectively adopted:
[player-verb inventory](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/player-verb-inventory.md),
[system-dynamics map](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/system-dynamics-map.md),
[game-dissection report](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-experience-analyzer/templates/game-dissection-report.md),
[full design brief](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/full-design-brief.md), and
[reference-game boundary](https://github.com/DY-2026/GameDesignOS/blob/d01dfebc6eac7a619b9a18f3cbafa51270d1edba/game-concept-architect/templates/reference-game-boundary.md):
reader/action scope, visible uncertainty, loop mapping, evidence links, scope gates, and validation
conditions. They were adapted to this repository's evidence labels and H4 boundary without copying
template text.

This project explicitly rejects the external project's nine-directory workspace, commercial
pitch/market assumptions, and second evidence/decision system. This repository already owns
`docs/research/`, `docs/design/`, `docs/decisions/`, `schemas/`, `manifests/research-index`, H2/H3
fixtures, and the H4 acceptance boundary. The external reference contributes only selective
authoring perspective; it is not a project dependency or a new source of truth.

## Collaboration and Continuing Hygiene

**Confirmed collaboration rule:** synthesis documents may be added over accepted evidence while
reverse engineering continues, but they must not rewrite a subsystem contract in parallel with its
active worker. When a future finding changes a conclusion, update the owning research note,
fixture/contract, and design explanation together so the trace remains bidirectional.

**Confirmed repository hygiene closure:** [`party-roster-state.md`](contracts/party-roster-state.md) is now
registered in `src/sf2tool/design_contracts.py` with its H2 map-script and H3 active-party fixtures.
The public tracked-input gate validates document path, fixture path, and fixture ID traceability in
both directions. This closure does not change any original-game finding.
