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
translation batch. [#434](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/434) tracks its
separately scoped synchronization from an independently assessed stable map-exploration source;
completing the baseline calibration alone does not establish stability for all themes. Chinese
translation and continuing synchronization remain eventual epic deliverables.

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

**Current scope agreement — Standard.** This English design round consumes accepted data
under [Epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514). Its outcome is a
readable account of the game's connected rules, world progression and player choices that supports
later design and implementation decisions. Standard rigor fits known evidence owners with
cross-system dependencies: declare each theme's claims and acceptance, independently review them,
and reassess when a new claim or ownership conflict changes the scope. This round does not select
a new engine, rebalance the game or expand the playable milestone.

### Accepted starting point

The [Gameplay Overview](synthesis/gameplay-overview.md) remains the reader entry. Existing
[Tactical Battle Loop](synthesis/tactical-battle-loop.md),
[Progression and Economy](synthesis/progression-and-economy.md),
[Story Progression](synthesis/story-progression.md) and
[Map Design Principles](synthesis/map-design-principles.md) remain the thematic owners. Their
accepted revisions provide the connected explanations selected below.
Existing detailed story guides remain available through Story Progression and the
[design index](../README.md#design), including the
[Battle27 activation guide](synthesis/battle27-activation-and-first-turn-dispatch.md).

The [story-content coverage owner](../research/story-content-coverage.md#four-different-frontiers)
separates original static research, original observations, English synthesis and remake production.
Accepted static chapter connections extend through the ending, credits and Battle0 reset, with
selected optional encounters and branches. This supports a qualified campaign-level explanation,
not a naturally played walkthrough, every NPC/chest/service visit, every legal combat action or
an exhaustive content census. Later research has not automatically become accepted synthesis.
The coverage owner's [remaining dispositions](../research/story-content-coverage.md#remaining-labels-and-next-content-task)
select no automatic collection queue.

### Selected English source set and reconciliation

The bounded English source set is exactly these five existing synthesis files. Each now has its
substantive explanation; **no further substantive rewrite is planned for any of them in this round**.
This disposition concerns the selected themes, not all design contracts or detailed chapter guides.

| Selected source | Current reader responsibility | Substantive rewrite remaining |
| --- | --- | --- |
| [gameplay-overview.md](synthesis/gameplay-overview.md) | Connected loop, theme entry and original/observed/remake/production boundaries. | None planned. |
| [story-progression.md](synthesis/story-progression.md) | Six chapter groups through ending/reset, selected optional branches and the Taros/Caravan dependency example. | None planned. |
| [tactical-battle-loop.md](synthesis/tactical-battle-loop.md) | Common choices, resolution and returns, with five encounter comparisons and their exceptions. | None planned. |
| [progression-and-economy.md](synthesis/progression-and-economy.md) | Membership, class, equipment and growth joined to receipt, capacity, spending and recovery. | None planned. |
| [map-design-principles.md](synthesis/map-design-principles.md) | Attempted interaction, dialogue continuation, transport, live/rebuilt state and persistence boundaries. | None planned. |

**Integrated candidate disposition:** prepared for independent main-gate reader acceptance, not
self-accepted. The review route is overview → appropriate theme → chapter/example → named evidence,
with thematic and chapter links back to useful reading entry points. Its connected example follows
[Ribble/Achilles → Taros → Caravan](synthesis/story-progression.md#worked-example-taros-and-the-caravan):
capacity/refusal, custody versus equipment, admission versus effect, first/repeat and non-victory
returns, acquisition versus boarding, and separate service access. Complementary Creed/Jaro,
reward/smith and Polca no-scroll/scroll branches check membership/control, resource and lifecycle
seams. These are selected static connections and bounded observations, not one naturally played
campaign or an all-content audit. Detailed review routes, exact candidate base/head/tree and actual
check results belong to [Issue #1280](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1280)
and its PR.

Stability becomes effective only after main-gate independently accepts and merges the integrated
candidate and confirms this no-planned-rewrite disposition. Main-gate records the exact accepted
merged source revision in that handoff before a separate translation dispatch. This is eligibility
for those five sources, not translated content, global translation PASS or completion of bilingual
Epic #514. [#434](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/434) selects the distinct
[map-exploration contract](contracts/map-exploration.md); its source readiness must be assessed
separately and is not conferred by this synthesis set. Existing Chinese mirrors and source anchors,
including drift and failures, remain unchanged.

The retained questions are precise limits on the explanation, not planned rewrites or automatic
research tasks:

| Retained question | Owning boundary |
| --- | --- |
| Natural campaign reach, every legal action/interaction and unobserved caller/presentation completion. | [Story coverage's four frontiers](../research/story-content-coverage.md#four-different-frontiers) and the selected chapter/encounter owners. Static connections do not extend the original observed route. |
| Complete promotion mutation, all-roster choices and campaign balance/scarcity. | [Party/resource judgment boundary](synthesis/progression-and-economy.md#judgment-boundary), [promotion](synthesis/progression-and-economy.md#definitions-current-class-and-promotion) and their named contracts. No optimal-build or universal curve claim is made. |
| Full transport/return lifecycles and durable save/suspend survival. | [Map return/persistence](synthesis/map-design-principles.md#return-and-persistence), [resource persistence](synthesis/progression-and-economy.md#persistence-boundary) and [save contract](contracts/save-system.md). Save-domain inclusion, save return and durable storage remain separate. |
| Broader playable content, hardware fidelity and future UI/UX or save choices. | [Capability owner](../../remake/docs/capability-status.md), [accepted milestone](synthesis/map3-battle01-readiness.md#accepted-current-milestone) and [accepted/future choices](synthesis/gameplay-overview.md#made-remake-choices-and-remaining-product-choices). English coverage grants no production admission. |

Historical original/modern failures, #1047's twelve completed unresolved comparisons and prior
actual diagnostics, corrections and CI remain with the linked evidence and Issue/PR owners.
No historical suite is rerun or relabeled to establish this source disposition.

The [Map3–Battle01 readiness owner](synthesis/map3-battle01-readiness.md#accepted-current-milestone)
and [retained comparison owner](../../remake/docs/evidence/retained-comparisons.md#accepted-composition-review)
retain the accepted composition and completed failures. Original observations retain their
[separate boundary](synthesis/map3-battle01-readiness.md#accepted-original-frontier).
Current production support belongs to the
[capability ledger](../../remake/docs/capability-status.md), its linked consumers and the
[coverage distinction](../research/story-content-coverage.md#four-different-frontiers).
None of those boundaries advances when prose is reorganized. Historical reports, failed
comparisons and Unknowns remain with their owners; this plan neither replaces them with PASS
nor restarts them. The [Phase 4 bootstrap plan](synthesis/phase4-bootstrap-plan.md) remains historical;
[ADR 0019](../decisions/0019-state-and-content-driven-remake-engine.md) owns current engine direction.

### Reader structure and ownership

Use three reading depths within the existing architecture:

1. **Overview:** explain the exploration, encounter, battle, reward/recovery and world-return loop,
   and point to the relevant thematic explanation. Keep original knowledge distinct from playable
   remake support and accepted deviations.
2. **Themes and chapter groups:** explain rules and dependencies in ordinary gameplay terms.
   A reader should understand what can be attempted, its conditions, changes, failure/refusal and
   return behavior, and the remaining uncertainty without reading assembly or raw fixture rows.
3. **Detailed references:** link existing segment guides, contracts and research for exact flags,
   source order, parameters and proof. Keep each fact's canonical owner; do not copy full source
   tables, repeat a technical audit in every chapter or create a second evidence inventory.

Chapter organization follows meaningful world changes and content dependencies, rather than one
page per battle number or investigation task. The initial editorial grouping is **Inferred**:
Grans opening and departure; New Granseal and eastern Parmecia; southern travel, Caravan and Creed;
northern travel, Pacalon, Tristan and Mitula; Moun/Nazca and the return to Grans; final confrontations
and ending. Optional recruitment, transport, trials and revisits appear beside their actual gates,
not as mandatory numbered steps. These are reading groups, not assertions about natural chronology
or the original authors' intended chapters.

Reuse existing pages first. Add a thematic page only when its responsibility has no adequate owner;
its Issue must name the file and the boundaries it takes from existing pages. Retain stable links or
repair their actual callers when reorganizing. Do not create empty directories, a new catalog
framework or wholesale file moves. Length follows content and readability, not a cap.

### Delivery order and acceptance

These five acceptance boundaries govern the selected source set, not one task per row or a new
writing queue. Each executable Issue owns one coherent reader outcome and exact paths; independent
review judges reader usefulness as well as evidence interpretation.

| Milestone | Reader question and deliverable | Owners and dependencies | Acceptance boundary |
| --- | --- | --- | --- |
| 1. Connected overview and campaign structure | How do the systems fit together, what changes across the world, and where do choices branch? Refresh the overview and turn Story Progression into a chapter-oriented route with links to existing detail. Extend the later chapter explanation from accepted research. | Existing overview/story owners; [chapter coverage](../research/story-content-coverage.md), [gameflow](../research/gameflow-core.md), [dialogue](contracts/dialogue-system.md), [roster](contracts/party-roster-state.md). | A reader can follow qualified entry, prerequisite, encounter/outcome and return connections through the selected chapter spine to the ending, find optional branches and distinguish static connections from observed play. Every selected group is explained or explicitly bounded; chronological numbering is not proof. |
| 2. Tactical rules and encounter composition | What can a combatant do, how is a turn resolved, what activates enemies, and what changes between encounters? Revise Tactical Battle Loop and organize evidence-backed encounter differences with representative examples. | [Battle control](contracts/battle-control-lifecycle.md), [actions](contracts/battle-action-construction.md), [navigation](contracts/battlefield-navigation.md), [AI](contracts/battle-ai-decision.md), [combat](contracts/combat-resolution.md), [spells](contracts/spell-resolution.md), selected encounter research linked by coverage. | Explain legal actions, movement/target conditions, resolution, enemy activation/control, outcome and retry/return boundaries. Separate common rules from boss, neutral, repeat and special-encounter exceptions; an initial formation or first dispatch does not prove a played battle or every AI decision. No simulator, difficulty rating or balance proposal. |
| 3. Party development and resources | How do recruitment, active party, class/promotion, equipment and growth interact, and how do items, magic, money and services constrain choices? Give party development a clear owner and revise Progression and Economy around those relationships. | [Roster](contracts/party-roster-state.md), [growth](contracts/level-up.md), [ally definitions](contracts/ally-definition-data.md), [item definitions](contracts/item-definition-data.md), [spellbook](contracts/spellbook-state.md), [services](contracts/service-interactions.md), [Caravan/deals](contracts/caravan-and-deals-state.md), accepted recruitment and service research. | Keep joining, following, active membership and AI distinct; connect growth/equipment to actual rule application. Explain resource sources, transfers, sinks, capacity, refusal/failure and recovery with actual service boundaries. Reference parameters at their owners; do not invent optimal builds, campaign balance curves or complete service/runtime coverage. |
| 4. Exploration, interaction and continuity | How do movement, interaction, map transitions, dialogue, transport and world changes compose, and which state survives each kind of return? Update Map Design Principles and the relevant story/overview sections, separating save/suspend from ordinary map return. | [Map exploration](contracts/map-exploration.md), [dialogue](contracts/dialogue-system.md), [map-entry state](contracts/map-entry-routing-state.md), [save](contracts/save-system.md), [input](contracts/input-system.md), accepted scene/service/transport owners. | Explain conditional interactions, current versus rebuilt entities, story gates, return/re-entry and persistence limits. Summarize accepted feedback/input/audio choices by linking their owners; no new UI layout, art/accessibility choice, save design or hardware-parity target. |
| 5. Integrated reader acceptance | Can a reader move from overview to a theme, through a chapter example, and back to its evidence without contradictions? Reconcile cross-links and repeated explanations; record the selected English themes that are stable and any deferred questions. | All accepted theme revisions; roadmap and existing navigation; independent main-gate review. | Review at least one connected route plus an optional/refusal or failure branch across themes. Verify consistent terminology, state distinctions and original/remake boundaries, not just links. Unexplained claimed transitions return to their owner. English completion does not complete the bilingual Epic. |

Integrated review uses the existing reading structure and worked cross-system example. Its accepted
detail remains in the
[sword acquisition guide](synthesis/ribble-tree-opening-and-achilles-sword.md),
[Taros admission guide](synthesis/southwest-parmecia-through-taros-battle18-entry.md),
[Battle18 initialization/first-control research](../research/battle18-initialization-and-first-control.md),
[Caravan acquisition guide](synthesis/battle18-outcomes-and-caravan-acquisition.md) and
[Caravan travel guide](synthesis/caravan-boarding-and-southwest-parmecia-travel.md).
The example connects an item/encounter condition, battle and outcome boundaries, acquisition and a
later traversal option, preserving refusal/failure and missing-prerequisite branches. An outcome
not established remains a precise gap; the example asserts no naturally uninterrupted itinerary.
Detailed reference owners remain read-only unless a concrete correction is separately scoped.
Main-gate serializes roadmap/index and other shared edits. Integrated acceptance checks these
connections and repeated terminology across themes; it is not another general rewrite or research
campaign. A new contradiction returns to its owner before the affected claim expands.

### Evidence, effort and stopping rules

Apply the pre-synthesis evidence review to the claims actually selected. Reuse accepted exact
objects, detailed reviews and public facts; inspect the relevant source/fixture boundaries when a
quantity, state transition or interpretation needs checking. Do not mechanically replay every
prior extraction, geometric calculation or experiment just because a thematic guide cites it.
Conflicting owners require an explicit correction or exclusion of the disputed claim. An unresolved
fact can remain **Unknown** while independent explanation proceeds.

For each substantive rule, preserve its conditions, observable or source-defined consequence,
important exception and linked owner. Label interpretations **Inferred**; static proof stays static.
New remake choices require their own product/decision owner. A synthesis gap becomes a research
Issue only when it blocks a named explanation or implementation decision and accepted owners cannot
answer it. Such an Issue names the smallest missing condition and required evidence; it is not an
instruction to collect all remaining data. Existing detailed failures and #1047's completed
unresolved comparisons remain preserved, not relabeled as interruptions or rerun to clear prose.

This is a documentation program using existing text and public facts. Work uses one
active theme writer, independent review and sequential integration, with small Markdown diffs and
local review notes. No new raw acquisition, private payload copy, generated data corpus, database,
comparison framework or maintained checker is needed. The uncertain cost is evidence reconciliation
and readable synthesis, not processing volume. Use actual authoring, review and correction effort
when sizing any later expansion. If a theme needs the same fact copied, a task cannot define its
reader question, or evidence repair starts dominating explanation, reassess ownership and scope before scaling. Necessary design
and review are not sacrificed to an arbitrary cost or document-length target.

Each slice requires direct prose/scope, relative-link/anchor, UTF-8/private-boundary and
`git diff --check` inspection, plus the configured committed planner and actual scoped CI under the
documentation-only policy. Do not launch blanket normal/full Python, .NET, Godot, H3/H4 or translation
suites for this round. Independent review checks evidence interpretation and reader usefulness;
a mechanical PASS alone is insufficient. Leave a clean committed/pushed Draft PR for independent
acceptance. No historical report, private input or retained failure is deleted by this plan.

This roadmap owns the current agreement and source disposition; Issues own task coordination.
Any later English expansion uses a separately scoped Issue. Chinese mirrors, glossary and translation
anchors remain untouched until a selected English theme is accepted and has no planned substantive
rewrite in this round. Record that stable source before dispatching its translation batch;
[#434](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/434)
tracks its separately scoped accepted-source translation.
[#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438) and its UI/UX
tasks keep product ownership; [#638](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/638)
remains paused. This round does not authorize production admission, new art, save/load implementation,
full-game parity or a general comparator.

## Long-Term Directions

Beyond the bounded theme explanations above, the following extensions remain **Unknown future directions**, not current
commitments. Work may begin only when entry criteria cite accepted local evidence; none authorizes a
new engine design or treats remake functionality as original-game evidence.

| Direction | Entry criteria and evidence dependencies | Non-goal |
| --- | --- | --- |
| experiential extension of map-design principles | The existing structural synthesis is the baseline; additional reachability and interaction-outcome observations must distinguish layout facts from player-route interpretation. | Do not infer authorial intent or redesign levels from 64x64 layout data alone. |
| exhaustive roster choice space across campaign states | The current round explains accepted recruitment/development rules. A wider comparison needs compatible capacity, lifecycle and encounter context beyond those selected accounts. | No tier list, “best party” advice, assumed preferences or complete campaign optimization. |
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
