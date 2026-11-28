# Documentation Roadmap and Governance Boundaries

- Status: **Confirmed repository governance guidance**; this document is not evidence about the
  original game and does not select a remake engine, product, platform, or commercial direction.
- Revised: 2026-11-27 (governance update under `#2382`)
- Scope: organize sourced rules and remake design into readable L0–L3 explanations without blurring
  evidence labels, original-game provenance, accepted remake choices, or unresolved **Unknown**s.

## Design and Support Scope

Use the [design map](design-map.md) to enter at the right depth: L0 vision, L1 overall gameplay,
L2 system design, or L3 mechanism specification. These levels organize reader questions; the
[Three-Layer Boundary](#three-layer-boundary) still governs who may change a claim and what
evidence backs it.

Chinese documentation (`docs/design/zh-CN/`) and Design Wiki publication focus strictly on **L0–L3
player- and designer-facing content**. Keep technical support in the canonical English repository:

| Source content | Disposition |
| --- | --- |
| [Vision](game-vision.md), [Gameplay Overview](synthesis/gameplay-overview.md), and system themes | Publish L0–L2 vision, gameplay loop, player choices, costs, and handoffs in English and Chinese. Keep evidence audits and historical slice comparisons in English repository owners. |
| Chapter and encounter design | Include story flow, player goals, optional/missable branches, encounter rules, and consequences. Worked analyses (such as [Battle01, Taros and Chessboard](synthesis/analyses/battle-design-analysis-pilot.md)) contribute **Inferred** design readings under explicit assumptions. Source matrices, capture comparisons, and reproduction commands stay in English. |
| Mixed behavior contracts (e.g., [map exploration](contracts/world/map-exploration.md), [spell resolution](contracts/battle/spell-resolution.md), [church promotion](../../remake/docs/application/church-promotion-design.md), [support spells](../../remake/docs/domain/support-spell-status-design.md)) | Include engine-neutral player rules: legal choices, formulas, refusal/cancellation, state order, and accepted deviations. Keep ROM layout, source call chains, raw tables/operands, implementation internals, commands, and run logs in English. |
| Raw tables, ROM inventories, and developer lifecycle indexes ([items](contracts/party/item-definition-data.md), [spells](contracts/party/spell-definition-data.md), [campaign flags](../../remake/docs/content/campaign-flags.md)) | English technical support. Cite rule-significant values directly in L2/L3 design prose and link back to the single English owner. |
| Original research, [Map3 readiness](synthesis/milestones/map3-battle01-readiness.md), capability/verification ledgers, tooling, and operations | English repository support. Keep readiness ledgers and acceptance reports out of Chinese design bodies and the Wiki; extract distinct player-facing decisions to their design owner when needed. |
| Navigation | Use L0–L3 entries with Chinese links for design prose and clearly labeled `（英文源）` links back to English evidence, configuration, and implementation owners. |

For a mixed English page, scope the Chinese design excerpt by heading and preserve links to the
exact English owner. Never duplicate raw tables or create a second evidence inventory inside
Chinese design pages or the Wiki.

## Authoring Language Policy

**Confirmed repository policy:** English remains the canonical authoring and review language for
technical authority, original-game evidence, and new design specifications. Chinese design pages
under `docs/design/zh-CN/` provide readable L0–L3 design prose and reviewed excerpts derived from
accepted `main`.

**Confirmed governance revision (2026-11-27):** [`glossary.md`](./glossary.md) governs terminology
and readability across L0–L3 Chinese design prose:

- **Natural L0–L3 design prose, exact technical identifiers:** explain player situations, choices,
  costs, and consequences in natural Chinese rather than disassembly or audit jargon. Keep source
  symbols, macros, handlers, fixture IDs, paths, and numeric literals verbatim under R2.
- **Preserve gameplay-critical distinctions:** do not collapse `round` (整轮回合), `individual
  turn` (轮到某单位行动), and `action` (具体行动), nor blur joined / active / temporary follower
  or carried / equipped states.
- **Accepted `#1468` proper nouns:** Chinese L0–L3 prose may use independently accepted Chinese
  proper nouns from the `#1468` localization corpus (`#1504`/`#1506`, owned by
  [`remake/docs/application/localization-design.md`](../../remake/docs/application/localization-design.md#current-full-text-chinese-corpus)),
  optionally annotating the English name at first occurrence. Do **not** invent ad-hoc Chinese
  proper nouns; leave unreviewed entries in English.
- **Clean evidence placement:** collect repetitive general source/scope caveats into a compact
  **依据与边界** note or table column, while keeping decision-changing conditions, **Inferred**
  interpretations, and **Unknown** questions directly adjacent to the claim they qualify.
- See [`glossary.md#calibration-examples-l0l3-chinese-design-prose`](./glossary.md#calibration-examples-l0l3-chinese-design-prose)
  for the three short calibration examples (turn/action, proper nouns, evidence placement).

`manifests/zh-translation-index.json` tracks the Chinese design pages present under
`docs/design/zh-CN/`. An English addition creates no automatic translation queue. Ongoing Wiki
synchronization and L0–L3 readability rewrites are coordinated under `#2259`, `#2366`, and `#2358`
(first Wiki publication `#1978` is closed); game localization remains under `#1468`.

## Design Information Architecture

**Confirmed repository policy:** `docs/design/` organizes its first level by ownership and evidence
role:

- `contracts/` — evidence-bound subsystem contracts, updated with the research slice when accepted
  findings change.
- `synthesis/` — cross-subsystem and player-facing explanations (`gameplay-overview.md`, `themes/`,
  `analyses/`, `milestones/`, and `chapters/`) consuming accepted evidence from `main`.
- Root governance files (`documentation-roadmap.md`, `glossary.md`, `game-vision.md`,
  `design-map.md`) govern or route across categories.
- `zh-CN/` — L0–L3 Chinese design prose and reviewed excerpts preserving the canonical English
  relative path.

Chapter folders follow the editorial sequence: `grans-opening`, `eastern-parmecia`,
`southern-travel-creed`, `northern-travel-mitula`, `moun-nazca-grans`, and
`final-confrontations-ending`. Keep one owner per cross-cutting topic and link rather than copying.
Do not create empty category scaffolding or wholesale file moves without a concrete navigation
need.

## Three-Layer Boundary

| Layer | Ownership and allowed content | Prohibited shortcut |
| --- | --- | --- |
| A. Original behavior/data contracts | `docs/research/`, `schemas/`, manifests, H2/H3 fixtures, and sourced design contracts record **Confirmed**, **Inferred**, and **Unknown** facts. | A source macro, field, or symbol name does not automatically establish player-visible meaning. Preserve the source label and explain only supported interpretations. |
| B. Reconstructed design explanation | `docs/design/` explains player-facing rules and consequences supported by Layer A, links the owning evidence, and keeps gaps **Unknown**. | Do not promote a static call, address, or plausible reading into confirmed original behavior, natural campaign reach, or original author intent. |
| C. Remake decisions | Modernization, implementation intent, and product choices belong in explicit decisions and remake acceptance owners. | Do not rewrite modernization as original behavior or use a synthesis page to silently pick an engine or product direction. |

**Confirmed repository rule:** Layer B is a traceable interpretation of accepted Layer A evidence,
not a second evidence system. **Inferred** readings remain **Inferred**, and **Unknown** behavior
stays explicit rather than being filled with narrative guesses.

## Pre-Synthesis Evidence Review

**Confirmed repository rule:** every substantive Layer B synthesis change must adversarially review
the Layer A evidence it explains—not merely copy a link. Where relevant to a claimed quantity,
order, or state transition, inspect the owning research prose, contract, fixture payload/ID, and
narrow H2/H3 owner. Test specifically for stale question queues, summaries broader than their
fixture, units reused across lifecycle stages, static call order presented as runtime outcome, and
controlled test seams presented as natural campaign play.

When owners disagree, do not pick the convenient answer or silently edit Layer A: exclude the
disputed claim or keep it **Unknown**, report the mismatch to the owning research lane, and wait
for an accepted owner correction.

## Current Baseline and Near-Term Synthesis

**Current scope agreement — Standard.** L0–L3 design documentation leads readers from player
experience (`design-map.md`, `game-vision.md`, `gameplay-overview.md`) through system themes and
chapter overviews to exact behavior rules, then links out to English research, configuration, and
remake implementation owners.

Previous round-freeze notes (such as historical claims that the five core themes needed no further
rewrite) are **superseded**; full historical round records (`#1280`, `#1047`, `#2129`, `#2228`)
remain in Git and their Issue/PR history rather than being repeated here. Current active design
documentation status:

| Design surface | Current status | Active coordination |
| --- | --- | --- |
| `game-vision.md`, `gameplay-overview.md`, `map-design-principles.md` (EN + `zh-CN`) | Reader-perspective rewrites are synchronized to the live Wiki. | `#2259` remaining Phase C |
| `design-map.md`, `tactical-battle-loop.md`, `progression-and-economy.md` (EN + `zh-CN`) | Accepted bilingual rewrites are synchronized to the live Wiki, including both core L2 themes. | `#2259` remaining Phase C |
| Wiki entry templates (`Home.md`, `_Sidebar.md`, `wiki-mirrors.tsv`) & live publication | Reader routes, compact source footers and six body mappings are published; independent remote readback and repeat-publication no-op passed. Follow the [publication workflow](../operations/wiki-preview.md) for subsequent accepted-source updates. | `#2259` Phases A–B complete |
| `battle-numbers.md` (EN + `zh-CN`) — core combat numbers and growth quick reference | Accepted and merged (`#2395`, repo-only; not yet synchronized to the live Wiki). | `#2259` C1 complete |
| Follow-on L2 design gaps (original-vs-remake modernization overview, UI/UX L2 overview) | Explicitly tracked as open follow-on scopes rather than substituted with engineering pages. | `#2386`, `#2259` remaining follow-on |
| `story-progression.md` and campaign event/chapter overviews | Phases A (`grans-opening/chapter-overview.md`, `#2396`), B1 (`eastern-parmecia/chapter-overview.md`, `#2397`), and B2 (`southern-travel-creed/chapter-overview.md`, `#2398`) accepted and merged (repo-only); Phase B3 (`northern-travel-mitula/chapter-overview.md`, EN + `zh-CN`) authored for Northern Travel through first drought-state Pacalon and admitted `Map71` departure (repo-only; live Wiki untouched); Northern second half (`Battle27`–Mitula/rainy Pacalon), remaining chapters, and cross-chapter recruitment/vehicle indexes remain pending. | `#2366` follow-on & Phase C |
| Modern remake engine architecture & Debug support design entry | Technical docs exist in `remake/docs/`, but designer-facing entry and cross-layer trace are pending. | `#2358` |
| Detailed chapter guides through Battle27 & later story sections | Accepted static research/synthesis within each page's stated boundary; later chapters link directly to English research rather than pending Chinese guides. | Story coverage in `docs/research/story-content-coverage.md` |

Historical reports, failed comparisons, [Map3–Battle01 readiness](synthesis/milestones/map3-battle01-readiness.md#accepted-current-milestone),
[retained comparisons](../../remake/docs/evidence/retained-comparisons.md#accepted-composition-review),
and [capability status](../../remake/docs/capability-status.md) retain their own authority.
Reorganizing or rewriting design prose never upgrades an **Unknown**, turns a failed comparison
into PASS, or expands claimed natural-play or remake coverage. [ADR 0019](../decisions/0019-state-and-content-driven-remake-engine.md)
owns current engine direction.

### Campaign lifecycle indexes

Developer lifecycle lookups under `remake/docs/content/` remain English technical owners linked
from L2/L3 design prose:

- [campaign flags](../../remake/docs/content/campaign-flags.md) — flag writers, readers, lifetimes,
  and receipts;
- [campaign items](../../remake/docs/content/campaign-items.md) — acquisition, custody, effects,
  and persistence;
- [campaign locations](../../remake/docs/content/campaign-locations.md) — setups, services, exits,
  and return/Continue;
- [campaign characters](../../remake/docs/content/campaign-characters.md) — recruitment, following,
  deployment, and persistence;
- [campaign feature trace](../../remake/docs/capability-status.md#campaign-feature-trace) — shared
  remake capability entry.

Design pages cite these owners for technical lookup rather than copying raw flag or item tables.

### Bounded battle-design analysis pilot

The accepted [battle-design analysis pilot](synthesis/analyses/battle-design-analysis-pilot.md)
([中文](zh-CN/synthesis/analyses/battle-design-analysis-pilot.md)) and
[Battle02 first-contact analysis](synthesis/analyses/battle02-first-contact.md)
([中文](zh-CN/synthesis/analyses/battle02-first-contact.md)) demonstrate encounter-level design
readings: problem → rule/placement → player response and tradeoffs. Such readings remain
**Inferred** under explicit preparation assumptions and never claim original author intent,
optimal routes, or difficulty scores.

### Reader structure and readability checklist

Organize L0–L3 design prose around three depths—**Overview** (L0/L1), **Themes and Chapter
Groups** (L2), and **Detailed References** (L3/contracts/research)—and check every revised design
page against these five rules:

1. **Player-first framing:** lead with the player's situation, legal choices, costs, and
   consequences—not disassembly call order, internal guards, or C# class lists.
2. **Natural wording with exact distinctions:** apply [`glossary.md`](./glossary.md) (keeping
   `round` / `individual turn` / `action`, joined / active / follower, and held / equipped
   distinct; preserving R2 identifiers only where needed).
3. **Disciplined evidence placement:** put general baseline/scope caveats in a compact
   **依据与边界** note or table column; keep decision-changing conditions, **Inferred** readings,
   and **Unknown**s immediately next to the affected claim.
4. **No execution-log bloat:** keep per-PR test transcripts, seed logs, and historical acceptance
   receipts in `remake/docs/evidence/` or PR history, never piled onto design prose.
5. **No invented intent or ad-hoc names:** never speculate on original author intent or invent
   Chinese proper nouns outside accepted `#1468` / reviewed glossary entries.

**Verification for documentation-only slices:** run direct prose/scope review, relative-link and
anchor checks, UTF-8 and private-boundary checks, `git diff --check`, applicable `zh-meta` checks,
and the clean-head verification planner / scoped CI. Do **not** run Godot, engine suites, H3, or
normal/full verification solely for documentation changes.

### Bounded translation repair agreement

**Current scope agreement — Standard.** Chinese design work covers L0–L3 prose and reviewed
excerpts listed in `manifests/zh-translation-index.json`. The index and on-disk `docs/design/zh-CN/`
files must match. English-only technical pages and new English design files do not create an
automatic translation queue.

Use `uv run --locked sf2 zh-meta test` for inventory, link, anchor, and drift verification, and
`uv run --locked sf2 zh-meta list` to inspect per-page drift. Whole-file anchors on retained mixed
pages are conservative; keep an older anchor when a source edit has not been semantically reviewed.

For each bounded Chinese design update:

1. **Start from accepted `main`:** confirm the canonical English owner, actual substantive changes,
   and competing work before editing.
2. **Translate and review for readability and accuracy:** apply [`glossary.md`](./glossary.md) (L0–L3
   natural prose, preserved gameplay distinctions, accepted `#1468` names, R1 evidence labels, R2
   identifiers where cited). Full translations preserve structure/fixture sequences; `design-excerpt`
   pages omit technical sections while preserving selected rules and boundaries.
3. **Keep link roles explicit:** route L0–L3 navigation to Chinese counterparts where available and
   label canonical English technical/evidence/implementation links clearly.
4. **Re-anchor only reviewed pages:** update only the reviewed records in
   `manifests/zh-translation-index.json`. When `glossary.md` changes without reviewing all existing
   mirrors, leave `glossaryDrift` visible until each affected L0–L3 mirror is reviewed in its own
   slice.
5. **Verify proportionally:** run direct scope, relative-link/anchor, private-boundary, and
   `zh-meta` checks plus the clean-head verification planner and scoped CI. Independent bilingual
   reader review remains required even when metadata passes.

## Long-Term Directions

Beyond bounded L0–L3 explanations, broader extensions—experiential map redesign, exhaustive roster
optimization, balance curves, or battle simulators—remain **Unknown future directions**, not active
commitments or remake balance targets.

## Reusable Authoring Structure

Organize design documents by coherent subject matter, reader flow, and maintainable ownership. Do
not impose arbitrary line or word caps, split documents solely for length, or strip necessary
explanation to hit a brevity target (the handwritten-source line limit does not apply to design
Markdown). Choose an opening that fits the level: L0 conveys the journey and remake vision; L1
connects preparation, battle, and return; L2 explains system choices and tradeoffs; L3 states exact
conditions, order, exceptions, cancellation, and failure rules.

## External Reference Provenance and Selective Adoption

**Confirmed external-reference provenance:** selected structural prompts (player-verb inventory,
system-dynamics map, game-dissection report, full design brief, reference-game boundary) were
adapted on 2026-08-01 from MIT-licensed `DY-2026/GameDesignOS` at pinned commit
`d01dfebc6eac7a619b9a18f3cbafa51270d1edba` without copying template text or adopting its
nine-directory workspace, commercial framing, or second evidence system.

## Collaboration and Continuing Hygiene

Synthesis edits must not rewrite a Layer A subsystem contract in parallel with its active research
worker. When an accepted finding changes a conclusion, update the owning research note,
fixture/contract, and design explanation together so traceability stays bidirectional.
[`party-roster-state.md`](contracts/party/party-roster-state.md) remains registered in
`src/sf2tool/design_contracts.py` with bidirectional fixture validation.
