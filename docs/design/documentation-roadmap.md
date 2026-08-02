# Documentation Roadmap and Governance Boundaries

- Status: **Confirmed repository governance guidance**; this document is not evidence about the
  original game and does not select a remake engine, product, platform, or commercial direction.
- Record date: 2026-08-01
- Scope: organize sourced contracts into concise player-facing explanations without changing their
  evidence labels; future remake choices still require explicit decisions and H4 acceptance
  boundaries.

## Authoring Language Policy

**Confirmed repository policy:** during the current design-synthesis phase, English is the canonical
authoring and review language for new or materially revised design-synthesis documents. Preserve
source-faithful identifiers, fixture IDs, evidence labels, and code vocabulary rather than inventing
translated equivalents.

Do not create ad hoc bilingual terminology while the glossary is unsettled. Once a project glossary
has been accepted, non-English localization from the canonical English source should happen as a
dedicated batch with terminology consistency, link integrity, evidence-label preservation, and
fixture-trace QA. The English source remains the review baseline unless that localization batch
defines another explicit policy.

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

**Confirmed repository baseline:** existing contracts cover combat, maps, level-up, spells, services,
save/input/window, dialogue, party/roster state, and randomness. They are listed in the
[design index](../README.md#design) and trace back to research and fixture owners. This roadmap does
not merge or replace those contracts.

The following ordering is an **Inferred planning priority**, not a claim that the listed design
conclusions already exist. Each synthesis document must remain incremental until its evidence owners
are stable.

| Order | Candidate document | Scope and prerequisites | Existing contract/evidence links | Non-goal / stop condition |
| ---: | --- | --- | --- | --- |
| 1 | gameplay overview | Explain currently supported player actions, state boundaries, and major subsystem handoffs, beginning only from accepted map, dialogue, roster, service, and input facts. | [map exploration](./map-exploration.md), [dialogue](./dialogue-system.md), [party/roster](./party-roster-state.md), [services](./service-interactions.md), [map-script fixture](../../tests/fixtures/h2/map-script-engine-static-v1.json) | Do not promise a complete campaign flow, interface feel, or narrative experience; these remain **Unknown**. |
| 2 | tactical battle loop | Explain the bounded order from player input/control through battle action, resolution, state replay, and known outcomes while retaining every unresolved branch; requires accepted battle-loop/action/AI research and combat/spell contracts. | [battle-loop research](../research/battle-loop.md), [battle-actions research](../research/battle-actions.md), [combat](./combat-resolution.md), [spell resolution](./spell-resolution.md), [physical-damage fixture](../../tests/fixtures/h3/physical-damage-v1.json) | Do not invent tactics, balance intent, target-selection meaning, or a general simulation from isolated cases. |
| 3 | progression and economy | Connect growth, EXP/gold/item, and service boundaries into a resource-flow explanation only where inputs, outputs, order, and persistence have evidence. | [ally-growth research](../research/ally-growth.md), [common-stats research](../research/common-stats.md), [level-up](./level-up.md), [services](./service-interactions.md), [level-up fixture](../../tests/fixtures/h3/level-up-boundaries-v1.json) | Do not claim an intended difficulty curve, intended prices, an optimal build, or a long-term economy. |
| 4 | story progression | Explain Confirmed state/route/dialogue/roster boundaries as a traceable progression map while retaining normal-story reachability and presentation labels; these least-stable dependencies place it after the preceding documents. | [gameflow research](../research/gameflow-core.md), [common-scripting research](../research/common-scripting.md), [dialogue](./dialogue-system.md), [party/roster](./party-roster-state.md), [dialogue runtime fixture](../../tests/fixtures/h3/map-script-dialogue-v1.json) | Do not reconstruct plot beats, player-choice consequences, or a complete story route from source labels or isolated program references. |

This order establishes reader navigation first, then covers the most bounded tactical loop, connected
resource flows, and finally story explanation that depends on reachability. A document waits when an
active slice is revising its owner contract or when most answers remain **Unknown**.

## Long-Term Directions

The following are **Unknown future directions**, not current commitments. Work may begin only when
entry criteria cite accepted local evidence; none of these directions authorizes a new engine design.

| Direction | Entry criteria and evidence dependencies | Non-goal |
| --- | --- | --- |
| map-design principles | A documented map corpus, route/event/area evidence, and enough reachability and interaction-outcome observations to distinguish layout facts from player-route interpretation. | Do not infer authorial intent or redesign levels from 64x64 layout data alone. |
| player roster choice space | Accepted roster, class/promotion, growth, equipment, battle-party, and persistence/capacity boundaries; unresolved lifecycle limits remain visible. | Do not publish a tier list, “best party” advice, or assumed player preferences. |
| player/enemy numerical curves | Complete source-backed numeric tables plus runtime-confirmed application, caps, and level/encounter context sufficient to name units and boundaries. | Do not set remake balance targets or describe mathematical curves as intended difficulty. |
| battle simulation | Complete and mutually compatible battle-loop/action/AI/pathfinding/state contracts plus a bounded H4 adapter acceptance surface. | Do not select a simulation architecture, claim general predictive accuracy, or use a model to fill unresolved branches. |

## Reusable Authoring Structure

Future `docs/design/` synthesis documents may selectively use the following structure. This describes
document shape, not a parallel workspace or a mandatory full GDD template.

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
   [combat contract](./combat-resolution.md) are the canonical trace; do not copy another evidence
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

**Confirmed repository hygiene closure:** [`party-roster-state.md`](./party-roster-state.md) is now
registered in `src/sf2tool/design_contracts.py` with its H2 map-script and H3 active-party fixtures.
The public tracked-input gate validates document path, fixture path, and fixture ID traceability in
both directions. This closure does not change any original-game finding.
