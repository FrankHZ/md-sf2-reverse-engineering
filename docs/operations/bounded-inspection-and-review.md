# Bounded Data Work, Inspection, and Review Runbook

Use this runbook when planning large acquisition or processing, inspecting a large artifact or topic
diff, preparing a root or main-gate handoff, or independently reviewing a candidate. It uses existing
Git, GitHub, and `sf2` outputs; it does not introduce telemetry, a benchmark, a new acceptance gate,
or a substitute for owning evidence.

## Plan before Scaling

Before a new or materially expanded bulk run, record a concise engineering plan in the owning
Issue or design surface. Durable requirements and acceptance boundaries belong in their tracked
owners. Scale the rigor to the work: a small bounded task needs a few concrete estimates, not a new
framework, manifest, monitoring service or human approval ceremony. Use existing dispatch and review.

1. **Question and granularity.** Name the claim, selected scope, required occurrences and context.
   Choose the minimum sufficient events, changes, counters or samples. Justify each additional
   per-frame, per-object or per-variant dimension; a convenient callback is not a data-volume plan.
2. **Cardinality and complexity.** Estimate records as duration × rate × subjects × variants where
   applicable, then bytes per record and input/intermediate/output sizes. Include distinct keys and
   skew, nested scans, sorting, repeated serialization/copies and per-row database work. A keyed
   requirement/use join can still cost `sum(R_k * U_k)`: an index reduces lookup cost, not the number
   of pairs. Streaming, SQLite and bounded queues alone establish neither affordable total time nor
   storage. Prefer scope selection before import/materialization and reuse existing reductions.
3. **Budgets and execution.** Estimate compute, elapsed time, peak memory, I/O, logical and physical
   storage, network and API/model costs where relevant; mark non-applicable costs. Include sources,
   transactions/journals, sort/spill buffers, report companions and publication copies. Plan stages,
   batch sizes and concurrency against shared capacity, retaining explicit headroom. State assumptions,
   uncertainty and thresholds for pausing expansion and replanning before resources are exhausted.
4. **Lifecycle.** Identify immutable raw evidence, reusable descriptors, derived reports and
   recomputable scratch, with exact owners and locations. Store facts once where practical and use
   references or streamed reductions. Define the new run's scratch disposal boundary in advance;
   this grants no cleanup authority over existing evidence, inputs, failure records or other runs.
   A published result must retain its declared dependencies and remain readable after permitted
   scratch disposal.
5. **Calibrate and stage.** Prefer existing relevant measurements; if key estimates are unknown,
   use a representative small pilot covering skew and the costly stages, including publication.
   Extrapolate with uncertainty and validate the main assumptions before full scale. A short native
   frame-time measurement does not validate offline full-route join or storage cost. Do not run the
   full expensive job merely to learn a cardinality available from counts or a small sample.
6. **Respond to measurements.** Record bounded stage progress through existing output. When actual
   growth or throughput materially differs, preserve diagnostics and revise the algorithm, scope,
   staging or concurrency. Do not blindly wait, raise limits or restart. Compression can protect
   physical headroom but does not fix logical amplification. Report any missing required evidence
   explicitly; a budget or a smaller selection never turns an incomplete claim into PASS. Existing
   process-control, input-preservation and cleanup authority still applies.

### Sampling, Events, and Writing

These are defaults for future capture design, not permission to thin existing evidence or change
accepted predicates. The [remake observation owner](../../remake/docs/development-and-verification.md#observation-and-comparison-planning)
applies them to Godot and offline comparison.

- Start ordinary operational telemetry (CPU, memory, progress) and coarse periodic state visibility
  around 1Hz; select another rate when the question requires it. This is not a limit on semantic events.
- Record necessary input, transition, actual consumption, completion and failure events at their
  originating boundaries with occurrence/order identity. A one-second point sample cannot replace
  a short-lived event, and a mount or request is not proof of actual use.
- Reuse stable descriptors. Summarize unchanged repeated uses with counts or delimited spans only
  when the predicate permits it and interval completeness is established. Retain actual first use,
  scope/lifetime, relevant changes, contradictions and auditable original locations. A high-frequency
  draw callback is still high-volume even when called event-driven.
- Collect per-frame detail only for a named claim over a bounded interval with record/byte estimates
  and an endpoint. Per-frame timing counters or accumulators can support the required tail/percentile
  measurement without a full-state JSON snapshot per frame; preserve the chosen metric's accuracy.
- Separate observation frequency from serialization and flush frequency. Batching one second of
  writes must preserve every necessary event within that second. Bound buffers and total output,
  and retain overflow, writer-error and terminal-integrity behavior.

## Source Size and Responsibility

New or extracted handwritten source must stay below 1,000 physical lines; aim for 300–800 where
that responsibility warrants it and plan a split before the limit. Small entry points and specific
value contracts can be shorter. Count physical lines, not statements: do not compress code, remove
useful comments, move a giant function intact, split by numbered parts, or create a catch-all helper
to satisfy the limit. Explicit generated/vendor code is excluded; naming handwritten code generated
is not an exemption.

An existing oversized file must not gain responsibilities or net growth. Extract the touched
responsibility in a bounded review; unrelated legacy files remain a migration backlog, not a global
prerequisite. Review dependency direction, state/resource ownership, real import/CLI consumers and
semantic differences before moving code. Keep compatibility only for actual consumers; avoid reverse
imports, circular dependencies, wildcard forwarding and mutable shared globals. Shared matching must
have demonstrably identical subset, ordering, partial and missing-value rules.

The initial enforcement is a direct changed-source review, alongside existing lint; no new CI
framework is required. Before staging, include newly added files in the count. On the staged diff:

```powershell
$changedSource = git diff --cached --name-only --diff-filter=AM -- '*.py' '*.cs'
foreach ($path in $changedSource) {
    [pscustomobject]@{ Path = $path; Lines = (Get-Content -LiteralPath $path).Count }
}
git diff --cached --numstat
```

The reviewer checks each new/extracted file against the limit and compares existing oversized files
to the accepted base for net growth and responsibility changes. Record the exact count/diff with the
handoff, not a growing history in agent guidance. Preserve complete old/new selected results,
including check order, occurrences, Unknowns and missing-versus-contradiction precedence; a matching
PASS alone is insufficient. Budget full-result output before batching controls. When repeated full
results exceed a per-command output budget, use smaller sequential batches and lossless local
snapshots, retaining completed results and failures.

### H4 Physical Module Route

The maintained CLI remains `python -m sf2tool.remake_h4_comparison`; its `physical` mode and
`compare_modern` use the same `physical_consumer_binding`. The monolith still owns transport,
resource lifetime, other families, report publication and CLI. Subsequent #638 slices must decompose
those owners independently; this representative family does not complete that migration.

| Owner under `src/sf2tool/remake_h4/` | Responsibility and interface |
| --- | --- |
| `physical_source.py` | `source_operands(source_root, item_ids)` verifies/reads pinned tables; `source_action(source, actors, attacker, target, seed)` calculates original draws/strikes. Existing AI, scene and reward consumers retain the old module's explicit aliases. |
| `physical_checks.py` | Physical-only subset/list/ordered matching, missing sentinel and `PhysicalChecks(session)` ordered check log/clock joins. No other family's matcher is consolidated here. |
| `physical_evidence.py` | `select_evidence(actual, context, checks)` consumes channel iterables and returns selected channel maps, events and event-to-result indexes; checks input/result/projection causality. |
| `physical_occurrences.py` | `compare_occurrences(selected, events, event_rows, census, source, checks)` checks scene preparation, predicted rules, persistent effects and reactions; returns occurrence summaries, Unknowns and covered effect sequences. |
| `physical_binding.py` | `physical_consumer_binding(actual, context, source_root)` orchestrates selection, source admission, census and occurrence checks and assembles the existing report. |

The dependency direction is binding → evidence/occurrences → checks/source → existing foundational
RNG/path/reference owners. No extracted module imports the monolith. Channel selection retains only
the declared rows while the caller's stream is open; new modules neither open nor close its SQLite
context. Source file reads and subprocess lifetimes remain local to operand loading. Reports contain
ordinary detached values and preserve ordering. Direct observations import their actual owner;
private closure-extraction or monkeypatch techniques do not require compatibility machinery. The
[physical verification entry](../../remake/docs/development-and-verification.md#scoped-physical-consumer-comparison)
owns compact controls and unchanged CLI usage; the
[scenario contract](../design/contracts/map3-battle01-continuous-scenario.md#selected-physical-rule-and-consumer-binding)
owns accepted semantics and evidence limits.

### H4 AI and Seed Module Route

The existing `ai` CLI and modern AI child use `ai_binding.ai_consumer_binding`; the existing
comparison module retains that import and the direct `_ai_source_rules`/`_ai_source_decision`
observation aliases. Its `admission_seed_binding` alias still serves field-service, modern and the
scoped admission-seed CLI. Opening controls remain a separate family.

| Owner under `src/sf2tool/remake_h4/` | Responsibility and interface |
| --- | --- |
| `ai_source.py` | `source_rules(source_root)` augments the existing physical source tables with AI declarations; `source_decision(source, actors, who, seed)` calculates original effects and logical paths. |
| `ai_checks.py` | AI-specific subset/subsequence/missing matching and `AiChecks(session)` ordered check log/clock comparisons. A shorter retained list may be a subsequence; this is different from physical pairwise matching. |
| `ai_evidence.py` | `select_evidence(actual, context, checks, producer)` returns rows/events/owners/census; `check_state_continuity(...)` binds source declarations and thinking/memory/region last writers across retained gaps. |
| `ai_decisions.py` | `compare_decisions(rows, events, owners, census, source, checks)` checks actual caller operands, source effects and their movement/action delivery; returns occurrence summaries and Unknowns. |
| `ai_binding.py` | Composes selection, source checks, decisions, accepted seed proof and consumer dependencies, then preserves ordered check compression and historical/current report roles. |
| `admission_seed.py` | `admission_seed_binding(actual, context, source_root)` owns the existing original-write/historical-delivery/accepted-execution seed composition, with its own matching semantics. |

AI binding depends on evidence/decisions/checks/source and the independent seed module. AI source
reuses physical source read-only; both families consume existing foundational parsers/RNG/path tools.
Seed imports no comparison family or monolith. Channel transport stays owned by the caller, and
consumer/source dependency reads and accepted TRX/adapter receipts retain their existing lifetimes.
The private historical seed latch remains FAIL even when the current composed mechanism passes.
Direct observations use the new source/check owners; changing an old alias is not forwarded as a
mutable override. Use the [AI](../../remake/docs/development-and-verification.md#scoped-ai-consumer-comparison)
and [seed](../../remake/docs/development-and-verification.md#scoped-admission-seed-comparison)
verification routes. Other families and transport/report/CLI decomposition remain future bounded
slices; neither this extraction nor a matching historical result closes those responsibilities.

### H4 Reward Module Route

The existing `reward` CLI and modern child retain `reward_consumer_binding`, now imported from
`reward_binding.py`. All owners below live in `src/sf2tool/remake_h4/` and keep reward matching local.

| Module | Responsibility / handoff |
| --- | --- |
| `reward_checks.py` | Reward value/list/count/clock contracts and the ordered `RewardChecks` log. |
| `reward_evidence.py` | `select_evidence` returns selected channels, events, owning indexes and census after input/result/projection checks. |
| `reward_source.py` | `load_source` returns pinned operand tables, initial mutable party progress and Unknowns; partial source failure retains available operands for independent receipt checks. |
| `reward_scenes.py` | `compare_scenes` derives EXP/gold/kill obligations from matched scene operands, with occurrence summaries and Unknowns; physical rules are reused read-only. |
| `reward_ledger.py` | `reduce_rewards` updates the one progress dictionary in event order and returns EXP/kill/gold balances plus Unknowns. |
| `reward_outcome.py` | `compare_outcome` reads those balances/progress and checks first Party.Progress, healing, gold/JOIN/flags through return. |
| `reward_binding.py` | Calls those owners in check order and preserves nonpass-first report compression and applicability. |

Operand mappings contain source tables/initial expectations only; progress is initialized by source
admission, mutated by the ledger, then read by outcome checks. Scene obligations and final balances
carry only their named reward/resource values. No module owns the caller's channel transport or
imports the monolith. Use the [reward verification route](../../remake/docs/development-and-verification.md#scoped-reward-and-outcome-comparison)
with the joined input including return; the [contract](../design/contracts/map3-battle01-continuous-scenario.md#selected-reward-growth-and-outcome-consumer-binding)
retains the reached-cohort EXP limit and missing/contradiction precedence.

### H4 HEAL Module Route

The scoped `heal` CLI and modern child share `heal_binding.heal_consumer_binding`; the old import
remains available. Owners under `src/sf2tool/remake_h4/` preserve HEAL-specific missing-value semantics.

| Module | Responsibility / handoff |
| --- | --- |
| `heal_checks.py` | Partial value/list/event matching and the single ordered `HealChecks` log. Numeric operands exclude booleans; explicit `None` remains unavailable. |
| `heal_source.py` | Pinned motion/cast/idle tables and `fairy_source_step`, the independent setup/update/controller calculation. |
| `heal_evidence.py` | Indexed channel/occurrence selection, event envelopes and `SceneProjections` post-Present identity joins; no transport ownership. |
| `heal_preparation.py` | Source scalar obligations, spell/target selection, effect records and prepared Submit; returns scalars, resource tuples, effects and events. |
| `heal_resources.py` | MP→HP→EXP phase order, live deferral and retention; reads preparation outputs. |
| `heal_opportunities.py` | Locally owns previous scene/seed while reducing services and delivery; returns phase work, final scene and occurrence counters. |
| `heal_work.py` | Source phase/caller obligations, fairy cleanup and scene completion. |
| `heal_binding.py` | Composes those checks in their original order and returns the detached report. |

The [contract](../design/contracts/map3-battle01-continuous-scenario.md#selected-heal-rule-and-consumer-binding)
and [verification route](../../remake/docs/development-and-verification.md#scoped-heal-consumer-comparison)
retain logical-clock limits, complete comparison controls and the caller-owned selected-stream boundary.

### H4 Battle-Scene Module Route

The old `battle_scene_consumer_binding` entry passes the existing `read` callable explicitly to
`scene_binding.battle_scene_consumer_binding(actual, context, source_root, read_document)`.
The scoped CLI and modern child use that wrapper; direct observations inject the same document
reader or a bounded candidate loader. All owners below live in `src/sf2tool/remake_h4/`.

| Module | Responsibility / handoff |
| --- | --- |
| `scene_checks.py` | Family partial matching, clocks and one ordered aggregate check log. |
| `scene_source.py` | Pinned animation/text bytes and read-only accepted material selectors. |
| `scene_dependencies.py` | Capped repository-relative document loading, context/provenance admission and keyed union of fresh dependency projections. |
| `scene_evidence.py` | Bounded JSONL reader and phase/event/projection/census joins. |
| `scene_construction.py` | Independent source strikes, critical text operands and constructed phase obligations. |
| `scene_rendering.py` | Mounted actors, animation, fairy/reaction and text; explicit `AnimationProgress` belongs to one phase. |
| `scene_fielddeath.py` | Source death batches and field-actor resource/visibility observations. |
| `scene_phases.py` | Owns each phase's progress and causal input, projection and completion checks. |
| `scene_terminal.py` | Admitted terminal release, attach, return and field-control composition. |
| `scene_binding.py` | Calls those owners in check order and returns the detached aggregate report. |

Loading retains the existing reader's behavior and size/error boundaries. The caller owns any
selected streams; these modules neither close them nor reopen the full capture. No owner imports
the monolith. See the [contract](../design/contracts/map3-battle01-continuous-scenario.md#selected-battle-scene-command-and-consumer-binding)
and [verification route](../../remake/docs/development-and-verification.md#scoped-battle-scene-consumer-comparison).

### H4 Field-Service Module Route

The existing `field_service_binding` and `_field_case_input` wrappers pass their real `read`
callable to `field_binding.field_service_binding(actual, context, source_root, read_document)`
and `field_evidence.case_input(case, read_document)`. Scoped and modern callers retain those
entries; direct source/case observation imports remain aliases. All modules live in
`src/sf2tool/remake_h4/` and none imports the monolith.

| Module | Responsibility / handoff |
| --- | --- |
| `field_values.py` | Strict value equality and partial predecessor joining; contradictions dominate missing evidence. |
| `field_source.py` | Seeded RNG, portrait counters, structural action programs and NPC motion; owns motion/counter/service constants. |
| `field_evidence.py` | One capped native case, repository-relative private paths and explicit document reader. |
| `field_admission.py` | Terminal receipt, absolute clocks, bounded process and declared initial actor state. |
| `field_wait.py` | One case's Wait press/release/cancellation owner and repeated-service readiness. |
| `field_state.py` | Declared events, typed operands, result identity and predecessor joins. |
| `field_services.py` | Ordered draws, service evolution and the admitted unregistered entry batch. |
| `field_projection.py` | Portrait lifecycle and actual input/Draw-to-resource joins. |
| `field_case.py` | Per-case ordered checks, coverage and detached report; retains the row exception boundary. |
| `field_binding.py` | Independent context, accepted dependencies, case admission and family aggregation. |

Case checks and Wait state are created for each invocation. The evidence loader opens and closes
bounded text files locally; injected document readers retain their own lifetime policy. Existing
in-memory cases remain caller-owned. Neither path extends the observed boundary or reads a raw
archive. See the [contract](../design/contracts/map3-battle01-continuous-scenario.md#selected-field-service-rule-and-consumer-binding)
and [verification route](../../remake/docs/development-and-verification.md#scoped-field-service-comparison).

### H4 Mutable-Map Module Route

`map_consumer_binding` remains the same callable at the old import and in
`map_binding`; scoped map/resource and modern callers use that alias. `_map_source_regions`
and `_map_draw_cells` remain direct observation aliases. These owners in
`src/sf2tool/remake_h4/` consume selected objects and the explicit source root:

| Module | Responsibility / handoff |
| --- | --- |
| `map_source.py` | Named source tables/layout/blocks, retained mutators and school population. |
| `map_history.py` | Historical session, complete selected ranges/producers and source-class coverage. |
| `map_cohort.py` | Source-derived region obligations for the admitted delivery roles. |
| `map_delivery.py` | Controlled input/ready-state chain and bounded transfer lineage. |
| `map_layout.py` | Available use facts, reconstructed working/saved words and snapshot execution identity. |
| `map_projection.py` | Legacy/current viewport planes and independent actor census. |
| `map_geometry.py` | Existing float32 arithmetic and positive-area clipping. |
| `map_actors.py` | Renderer-derived actor admission and source sprite ink for one draw inventory. |
| `map_draw.py` | Expected plane/mask cells and actual coordinate/resource multiset. |
| `map_witness.py` | Ordered region comparisons, per-binding identities and repeated-read continuity. |
| `map_binding.py` | Source admission, aggregate check log and composition of those owners. |

`SpriteInk` retains only the existing per-draw source table and alpha-run cache; each draw gets
a fresh instance. Selected streams remain caller-owned, and reports are detached from them.
No module imports the monolith or changes material/texture prerequisites. Actor missingness
does not bypass independent plane geometry. See the [delivery contract](../design/contracts/map3-battle01-continuous-scenario.md#composed-mutable-map-delivery)
and [verification route](../../remake/docs/development-and-verification.md#composed-mutable-map-verification).

### H4 W1 Module Route

`w1_consumer_binding` is the same callable at the old import and in `w1_binding`;
the scoped CLI and modern comparison use that alias. The three `_W1_*` source-cohort
constants remain available there. Owners live in `src/sf2tool/remake_h4/`:

| Module | Responsibility / handoff |
| --- | --- |
| `w1_checks.py` | W1's partial matcher, absence marker and ordered check log for one binding. |
| `w1_selection.py` | Independent cohort and indexed selected input/result/state operands. |
| `w1_envelopes.py` | Complete Submit/event bounds and W1/W2 operation attribution. |
| `w1_source.py` | Pinned cohort/programs/text/portraits/layout, RNG and bounded continuation rules. |
| `w1_history.py` | NPC provenance, producer history and source walking installations. |
| `w1_input.py` | Physical input, complete choice delivery and ready/post snapshot joins. |
| `w1_text.py` | Source caller/speaker/portrait ancestry and displayed token stream. |
| `w1_npc.py` | Stationary geometry, source retries/collision and destination effects. |
| `w1_services.py` | Poll draw/copy/read order and conditional portrait RNG. |
| `w1_continuation.py` | Accepting continuation or neutral consumer retention. |
| `w1_retained.py` | Close boundary, later copy, restoration and final event clocks. |
| `w1_binding.py` | Ordered composition with explicit per-poll seed/effect returns. |

`W1Source` contains only independent source operands; `W1Checks` owns only matching and reporting.
Family matching remains distinct from strict field equality. Indexed full-capture containers are
accessed only at selected positions; already selected streams remain caller-owned. Reports retain
no stream handles. No module imports the monolith. See the [contract](../design/contracts/map3-battle01-continuous-scenario.md#selected-w1-consumer-binding)
and [verification route](../../remake/docs/development-and-verification.md#scoped-w1-consumer-comparison).

### H4 W2 Module Route

`w2_consumer_binding` remains the same callable at its old import and in `w2_binding`, used by
the scoped CLI and modern comparison. `_W2_COHORT` remains an alias. Owners live in
`src/sf2tool/remake_h4/`:

| Module | Responsibility / handoff |
| --- | --- |
| `w2_checks.py` | W2 partial matching, per-binding absence marker and ordered check log. |
| `w2_source.py` | Independent cohort, pinned clean text/program compilation and bounded continuation. |
| `w2_selection.py` | Accepted context, indexed selected channels and accepting/neutral poll inventory. |
| `w2_input.py` | Physical input, whole Submit snapshot and retained neutral readiness joins. |
| `w2_caller.py` | Source caller/live gates and displayed W2 token span. |
| `w2_service.py` | Draw/copy/read order, internal Submit bounds and neutral retention. |
| `w2_continuation.py` | Accepting token release and source resumed producer/order/terminal comparison. |
| `w2_indicator.py` | Retained copy and distinct same-submit or later-state indicator witnesses. |
| `w2_validation.py` | All matching validation starts and whole Submit receipt identity. |
| `w2_binding.py` | Ordered composition and complete report. |

`W2Source` holds only independent source operands; `W2Checks` owns only matching and reporting.
Helpers pass explicit selected operands and results. Full-capture channels use indexed selection;
already selected streams remain caller-owned and reports retain no handles. No owner imports the
monolith or supplies missing timing. See the [contract](../design/contracts/map3-battle01-continuous-scenario.md#composed-w2-consumer-binding)
and [verification route](../../remake/docs/development-and-verification.md#scoped-w2-consumer-comparison).

### H4 Turn Module Route

The old `_source_turn_order`, `turn_order_binding` and `turn_order_consumer_binding` imports
remain aliases to the owners below in `src/sf2tool/remake_h4/`. Scoped and modern callers use
those aliases. Generation and consumer matching intentionally have different numeric rules.

| Module | Responsibility / handoff |
| --- | --- |
| `turn_source.py` | Independent RNG, full 64-slot buffer and signed 62-pass source generation. |
| `turn_generation_checks.py` | Strict generation values and ordered identity-keyed records. |
| `turn_generation_candidates.py` | Independent live roster and recorded candidate coverage. |
| `turn_generation_draws.py` | Draw identities, arithmetic and candidate seed chains. |
| `turn_generation_scores.py` | Local candidate scores and sorting at recorded operands. |
| `turn_generation.py` | Executed generation/source composition and selected round coverage. |
| `turn_consumer_checks.py` | Consumer matching, grouped counts/eight-example cap and clock joins. |
| `turn_dependencies.py` | Executed proof applicability and seven tested/current dependency reads. |
| `turn_selection.py` | Supplied channel indices and required selected records. |
| `turn_result_clocks.py` | Result/event/sample clock domains, bounds and delivered identity. |
| `turn_input_clocks.py` | Input spans, causal ordinals and direct/automatic clock bounds. |
| `turn_installation.py` | Retained installed queues and independent semantic census. |
| `turn_evidence.py` | Owning event joins, repeated publications and nonconsuming diagnostics. |
| `turn_hp.py` | HP/placement knowledge, authoritative writes and missing observations. |
| `turn_frontier.py` | Queue progression, dead skips, sentinel rollover and terminal support. |
| `turn_projection.py` | Wait/input projections and additional supplied evidence applicability. |
| `turn_consumer.py` | Ordered generation/dependency/consumer composition and final report. |

`HPKnowledge` owns only HP and placement; checks own only values/reporting. Source, independent
context and supplied evidence remain explicit operands. Dependency Unavailable does not stop
consumer contradictions. Selected streams remain caller-owned; reports retain no handles.
No owner imports the monolith. See the [contract](../design/contracts/map3-battle01-continuous-scenario.md#composed-current-turn-rule-and-queue-consumption)
and [verification route](../../remake/docs/development-and-verification.md#composed-turn-rule-and-consumer-verification).

### H4 Audio Module Route

The old `audio_consumer_binding(actual, context, source_root)` entry explicitly passes the existing
bounded-list factory to `audio_consumer.py`. `_audio_context` remains an alias to its selection owner.
Scoped and modern callers use these entries; transport limits and publication ownership stay with
the existing reader/writer.

| Module in `src/sf2tool/remake_h4/` | Responsibility / handoff |
| --- | --- |
| `audio_context.py` | Reached script operands, producer session and metadata without PCM. |
| `audio_source.py` | Pinned driver identity and active sound-slot classification. |
| `audio_identity.py` | Selected metadata, complete receipt channel and producer session. |
| `audio_playback.py` | Unique playback lifetimes, inherited timer and legitimate replacements. |
| `audio_events.py` | Dependent logical event identity and selected session. |
| `audio_scene.py` | Phase-owned fade, stop/restore and actual scene release. |
| `audio_finite.py` | Source request, helper clock, actual finish, release and prior cue resume. |
| `audio_confirm.py` | Same-session plain Confirm transition and caller return. |
| `audio_consumer.py` | Ordered composition and three bounded report lists. |

Each owner receives its evidence and report callback explicitly; none imports the monolith.
WaitToken remains service context, not voice identity. Caller-owned selected streams are never
closed by the comparison. Under streaming transport, publish through the existing writer before
closing the report store; the published companion can then be reopened independently. Ordinary
list reports remain detached. The [audio contract](../design/contracts/map3-battle01-continuous-scenario.md#composed-reached-audio-consumer-binding)
and [scoped verification](../../remake/docs/development-and-verification.md#scoped-audio-consumer-comparison)
retain Option A, independent tail evidence and original hardware Unknowns.

### H4 Opening Admission Module Route

`admission_opening_binding(actual, context, source_root)` remains the old import alias to
`opening_binding.py`. Scoped admission-seed and modern callers use that alias. The accepted
`admission_seed.py` child remains separate from the opening proof and its value matching.

| Module in `src/sf2tool/remake_h4/` | Responsibility / handoff |
| --- | --- |
| `opening_checks.py` | Opening-specific values, absence, unique selection and ordered checks. |
| `opening_identity.py` | Original candidate/host/observer identities, completion and named restoration. |
| `opening_records.py` | Complete producer count/order/seen set and independent R1 epoch equations. |
| `opening_readback.py` | Selected original scalar seams, progression and R1 joins. |
| `opening_terminal.py` | Returned-script snapshot, program, count and selected input prefix. |
| `opening_source.py` | Mouth/view reader operands and pinned no-delay source path. |
| `opening_actual.py` | Historical A admission, physical input, Submit and selected settings. |
| `opening_binding.py` | Ordered composition of the two distinct proof domains. |

Each owner receives its evidence and opening check ledger explicitly. No owner imports the
monolith or shares a matcher with another family. Selected streams remain caller-owned and reports
remain detached after closure. Preserve the [controlled opening contract](../design/contracts/map3-battle01-continuous-scenario.md#controlled-opening-control-binding)
and [scoped verification](../../remake/docs/development-and-verification.md#selected-opening-controls):
original/actual sessions and clocks differ; sparse evidence never invents a first glyph or timing.

### H4 Field Motion Module Route

`field_motion_binding(actual, selection, source_root)` remains the comparator entry used by
`compare_modern`. Its wrapper injects the existing `read`, `_bounded_list`, `_occurrence_map`,
`_bounded_sorted` and `_group_rows` functions explicitly. These retain caller-owned storage;
motion modules do not import the comparator or introduce a transport adapter.

The source correspondence below covers the complete function at
`4dfa055699839a81c5259d5fad854a386fd48dfe:src/sf2tool/remake_h4_comparison.py`, lines4877–6163.
Use `git show` for that exact object and compare each region with its named current owner.
Line intervals include adjoining blank lines and control headers; every old line has one owner.
This is the structural acceptance boundary, including branches absent from retained pilot rows.

| Module in `src/sf2tool/remake_h4/` | Old regions | Body / explicit state bridge |
| --- | --- | --- |
| `motion_binding.py` | 4877–4878,5126–5221,5311–5312,5411,5729–5755,6147–6163 | Composition, readiness, missing typed-producer early `continue`, nonawaited installation, dependent ordering and aggregation remain here. |
| `motion_checks.py` | 4879–4892,5120–5125 | Factory returns the same result and `check`, `common`, `aggregate`, `operand` closures; checks append in original order. |
| `motion_programs.py` | 4893–4899,4984–5062 | `target` moves to module scope. Factory takes programs/compiler/tracked source and returns instruction/source-value closures and source spans; both caches remain per call. |
| `motion_source.py` | 4900–4957 | Takes selection/source/reader/common; returns programs/compiler/tracked source after the same identity checks and catches. |
| `motion_events.py` | 4958–4983,5115–5119 | Takes actual/common/map/sort/group factories; returns warp records, record index, ordered events, release/token/boundary indexes and the occurrence lookup closure `following`. |
| `motion_inventory.py` | 5063–5111 | Takes ordered events/records/index/instruction/common/list factory; returns producers. Warp/transfer/last-location state stays local. |
| `motion_wait.py` | 5112–5114,5222–5310 | `entity` moves to module scope. Held-wait checks take entry/record/event/role/states/location/instruction/subject/token and callbacks. |
| `motion_commands.py` | 5313–5410 | Takes release indexes and occurrence operands/callback; mutates the same occurrence's motion operands and returns `end`. |
| `motion_completion.py` | 5412–5569,5674 | Takes occurrence evidence, `following`, callbacks/list factory; retains handoff order, delegates fade/shiver under the original guards, returns `end`. |
| `motion_fade.py` | 5570–5673 | Takes states/instruction/helper/token/callbacks/list factory; saved-entry, continuity, period and palette computations are unchanged. |
| `motion_shiver.py` | 5675–5728 | Takes entry/states/event index/end/records/actual/token/subject/callback; retains minimum-revision sample/battle-entry fallback and restoration order. |
| `motion_draws.py` | 5756–5770,5818–5853,6124–6146 | Takes rows/role/instruction/subject/states/token/callbacks/list factory. Owns applicability, used list and phase set per occurrence; retains generic-loader plain-list replacement. |
| `motion_phases.py` | 5771–5817 | Factory takes effect/subject/token and returns the unchanged `semantic_phases` closure with its original captured defaults. |
| `motion_projection.py` | 5854–6123 | Takes draws/subject/token/instruction/role/effect/phase closure/used list/phase set/callbacks. Preserves the complete draw loop, early `continue`s, float32 intersection and phase predicates; mutates the caller's used list and phase set. |

No source local or predicate is renamed. `struct` moves with the float32 predicate. Definitions of
pure helpers and closure construction move to their owners; source computations, check order,
missing-versus-contradictory handling, aggregate filters and transport calls do not change. The only
new returns replace the explicit bridges above; there is no cross-call cache or shared occurrence
state. The list/map/group/sort dependencies are passed to the owners that previously invoked them.

Structural observations use only saved complete world programs/provenance and retained pilot rows,
plus explicitly constructed controls. The world payload is a documented read view of the original
selected path; normalize the receipt's relative selection against its original producer root and
retain its original bytes and mapping. Shape-derived channel assignments and constructed events
are controlled inputs, never historical channel/index evidence. Compare complete ordered reports,
real entry/modern wiring, factory injection, per-call/per-occurrence state and detached stream
publication. The private reproduction owner is `local/issue638/motion-modules-01` (`observe.py`,
`interfaces.py`, `boundaries.py`, source-region map and full paired reports). New scratch is bounded
to40MiB and each process to120s/128MiB. No additional raw actual/world reads, capture selection,
frame database or acquisition is part of this acceptance.

This structural change makes no semantic repair or new full historical A-02 PASS claim. PR603's
accepted behavioral evidence remains at merge object `ed8591713ccf6329307de78ed7fecf43623be35f`;
its completed failures remain unchanged. The [motion contract](../design/contracts/map3-battle01-continuous-scenario.md#complete-reached-field-motion-and-consumer-binding)
and [verification scope](../../remake/docs/development-and-verification.md#complete-field-motion-consumer-comparison)
retain that distinction. Unsupported runtime combinations stay Unknown even when their moved
source bodies have complete structural correspondence.

### H4 Operation-flow Module Route

The existing `operation_flow_binding(actual, selection, source_root, motion, text)` entry and
modern caller use `operation_flow_binding.py`. The wrapper explicitly injects the existing reader,
bounded-list, occurrence-map, bounded-sort and occurrence-dict functions. The five family names,
ordered checks/programs/warps arrays, False-over-missing reduction and source-error early finish
are unchanged. Other comparison families and transport internals retain their owners.

This correspondence covers the complete function at
`9b77cf33d13dabf799b0ce0cc17d2916107702ea:src/sf2tool/remake_h4_comparison.py`, lines4891–6369;
the two following separator lines stay in the comparator. Inspect that exact Git object against
these current owners, including branches absent from the controlled observations.

| Module in `src/sf2tool/remake_h4/` | Old regions | Body and state bridge |
| --- | --- | --- |
| `operation_flow_binding.py` | 4891–4892,5004–5005,5087–5120,5718–5723,5967–5968,6369 | Ordered composition, reached inventory, source successors, unmapped-instruction early continue and final finish. |
| `operation_flow_checks.py` | 4893–4918 | Factory returns names/result and the same check/common/finish closures; every ledger is per call. |
| `operation_flow_source.py` | 4919–4980 | Reader/selection/source/common inputs; returns world/programs/maps/normalized source root/tracked paths/compiler/routes and the existing parser functions. On the same caught errors, returns None so binding immediately calls the original finish closure. |
| `operation_flow_indexes.py` | 4981–4991,5121–5133,5235–5248 | Event factory returns events/record ordinals/warp records/ordered events. State factory returns sorted native references and state-of/anchor closures. Entity/signature helpers move to module scope; snapshots are not copied into indexes. |
| `operation_flow_programs.py` | 4992–5003,5006–5086 | Instruction-reader closure takes programs; reached-body validation receives the compiler/routes/tracked paths/parsers and appends to the original result/check ledger. |
| `operation_flow_control.py` | 5134–5234 | Takes actual/events/instruction/names/check/map factory; returns control-read index after the original identity, interval and full-stack predicates. |
| `operation_flow_flags.py` | 5249–5339 | Takes world/maps/ordered events/instruction/state lookups/map factory; returns flags-at/join-effect/write-flags closures. Choice index and party layout remain per call. |
| `operation_flow_warps.py` | 5340–5482,5676–5717 | Source request inventory, route choice and field release retain their loops/early continues; returns warp requests and invokes initialization only under the original post-state guard. |
| `operation_flow_initialization.py` | 5483–5675 | Receives one warp's request/transfer/post/target/route/flags plus source/state dependencies and callbacks; retains pose/facing, source allocation, preserve mode, overrides and independent latch checks. No state escapes. |
| `operation_flow_branches.py` | 5724–5858 | Per-executed-instruction arguments retain branch evaluation, nested-return scan and invocation-bounded caller/control-read lookup; no later invocation can supply a missing stack. |
| `operation_flow_roster.py` | 5859–5913 | Same flag replay, counted prefixes and follower installation, with explicit instruction/state/flag-writer callbacks. |
| `operation_flow_choices.py` | 5914–5966 | Same yes/no guard, accepted/flag/return interval and independent actual flag effect. |
| `operation_flow_outcome.py` | 5969–6098 | Receives actual/events/maps/warp requests/state lookup/motion/flag callbacks and storage factories; retains last living pose, transfer/fades/readiness, then invokes shared tail under the original guard. |
| `operation_flow_tail.py` | 6099–6179 | Receives battle/outcome/return interval and independent state/flag callbacks; preserves ordered markers, flag effects, counted membership and intervening writers. |
| `operation_flow_effects.py` | 6180–6255 | Retains before/after body loops, physical last-writer effects and HP/MP reset, then calls scene predicates for that same event. |
| `operation_flow_scene.py` | 6256–6368 | Receives the same event/body/index/state/callback operands; preserves the distinct post-load service, physical-set retention and explicit-null camera/fallback rules. |

No source local is renamed. The new admission tuple/None return is the explicit replacement for
the original source block's enclosing early return. Other helper returns expose existing closure
state, not new authorities. The compiler and source registrations stay local to admission and its
binding call; the existing parser imports occur only inside the admitted source block. Check
callbacks and mutable flag sets retain their original order/lifetimes. Storage factories stay
caller-owned; publication uses the existing detached report bundle mechanism.

The structural reproduction owner is `local/issue638/operation-flow-modules-01`: accepted-function
source/region map, generated-body correspondence, complete ordered report baselines/equality
receipts and actual interface observations. Saved older-world programs are only source seeds;
named current pinned-source compilations and explicitly constructed maps/events/states form
separate structural controls. Their local selection receipt is a control, not historical A03
provenance. Keep source admission, missing-only, wrong-only and mixed contradictions distinct.
No old raw actual/world, eager historical script or full A03 comparison is part of this acceptance.
Input is bounded to10MiB, fresh scratch to40MiB and each observation to120s/128MiB private memory;
calibrate with a representative small case before batching and retain full differences/failures.

PR604's historical evidence remains at merge `4d2251316fb19f5e6acfb2a7c023eeabca8df561`, final
candidate `f569549a80411ba73dcb2eb42ee76554c366d3a9`. Its camera, caller-interval, initialization
and shared-tail repairs remain intact; calls4107/12608 remain Unknown at their historical missing
in-call-stack boundary. Structural correspondence does not create new natural reach or full
historical behavior evidence. The [operation-flow contract](../design/contracts/map3-battle01-continuous-scenario.md#complete-reached-operation-flow-binding)
and [verification owner](../../remake/docs/development-and-verification.md#complete-operation-flow-comparison)
retain those limits.

### H4 Text-material Module Route

The existing `text_material_binding(actual, outcome, selection, source_root)` entry and modern
caller delegate to `text_material_binding.py`. Its explicit dependencies are the existing reader,
bounded-list, occurrence-map/set/dict and bounded-sort factories. The ordered checks, field/battle
joins, independent required inventories and False-over-missing reduction retain their original
owners and lifetimes. Other comparison families and transport internals are outside this route.

The complete source correspondence is
`0ec02f9a206f90432b566f6ceb710e29da3a538f:src/sf2tool/remake_h4_comparison.py`, lines2239–2785;
the following separator lines stay in the comparator. Review every mapped body, including branches
not exercised by the bounded controls.

| Module in `src/sf2tool/remake_h4/` | Old regions | Body and state bridge |
| --- | --- | --- |
| `text_material_binding.py` | 2239–2253,2369–2379,2783–2785 | Per-call result/check closure, absent-selection return, first-occurrence event index, ordered composition and final reduction. |
| `text_material_source.py` | 2269–2368 | Reader/selection/source/check inputs; returns partial world/texts/member names/enemy selectors/ASCII map/advances even after the original caught source errors. |
| `text_material_units.py` | 2254–2268,2380–2406 | Stateless configured-font predicate and a per-call unit-reader closure over admitted names/map/advances. |
| `text_material_field.py` | 2407–2514 | Source span inventory, accepting-event continuation and field/outcome consumer joins, with explicit event index/unit reader/result/check/storage arguments. |
| `text_material_battle.py` | 2515–2522,2526–2603,2707–2782 | Preparation/reaction intervals, local actor-name closure, consumer joins and independent required message inventory. |
| `text_material_operands.py` | 2523–2525,2604–2706 | Stateless actor lookup and typed action/HP/EXP/gold/growth template selection; returns the existing tid/value/who/healing locals for the same reaction. |

No original source local or predicate changes. Source admission keeps partial state after local
absence so later independent font/content contradictions remain visible. Operand exceptions still
reach the original enclosing battle catch. The original CRLF spriteset representation and raw
lethal reaction Amount remain explicit. No rendered text chooses its own source template. Field
and battle required inventories remain independent of their consumer channels.

Readers and storage factories remain caller-owned. Checks and actual joins use their original
bounded append stores; required inventories retain the existing small-list/spill behavior. No
module closes a reader or takes over publication. The existing detached report bundle remains
readable after the input context closes. Mutable indexes, span state and reaction locals are per
call; no module adds a cache, generic dependency object or reverse comparator import.

Structural reproduction belongs to `local/issue638/text-material-modules-01`: exact source/region
and generated-body maps, complete ordered old/new control reports, equality receipts and real
entry/storage/publication observations. Controls use pinned source-derived text/font operands and
explicitly constructed world/scene/receipt/event/state channels. They are not historical A or
natural-reach evidence. The registered private font passes the existing fixture identity before
control derivation; no raw historical actual/world, eager recipe or full A comparison is read/run.
The [text material contract](../design/contracts/map3-battle01-continuous-scenario.md#complete-reached-displayed-text-material-binding)
and [verification owner](../../remake/docs/development-and-verification.md#continuous-text-material-comparison)
retain PR601's accepted evidence, omissions/mixed counterexamples, completed failures and Unknowns.

## Inspect Identity and Shape First

For a clean committed candidate, reproduce its identity and changed shape before opening content:

```powershell
$reviewBase = 'origin/main'
$reviewHead = 'HEAD'

git status --short --branch
git rev-parse $reviewBase
git rev-parse $reviewHead
git rev-parse "$reviewHead^{tree}"
git merge-base $reviewBase $reviewHead
git diff --name-status "$reviewBase...$reviewHead"
git diff --stat "$reviewBase...$reviewHead"
git diff --numstat "$reviewBase...$reviewHead"
git diff --check "$reviewBase...$reviewHead"
uv run sf2 verify plan --base $reviewBase --head $reviewHead
```

The committed-range planner reports current executable selection, not semantic correctness or
permission to override [current verification scope](../../remake/docs/development-and-verification.md#scope).
Research fanout requires resolving the affected evidence boundary. New-engine legacy-test retirement
requires the coordinated code/CI/planner cutover; a documentation slice records that pending work
without implementing it or rerunning every old selection.

After classifying the paths, inspect only the owning file or bounded hunk needed for the current
judgment:

```powershell
git diff --unified=20 "$reviewBase...$reviewHead" -- path/to/owner
```

Increase context or open a neighboring owner only when a concrete reference, invariant, or finding
requires it. Do not paste a full controller history, full repository diff, or unrelated source tree
into another agent's prompt.

## Reduce Large Artifacts before Reading Payloads

For a large schema, fixture, manifest, trace, or generated report, start with tracked identity and
shape:

```powershell
$artifactPath = 'path/to/artifact'
Get-Item -LiteralPath $artifactPath | Select-Object FullName, Length
Get-FileHash -LiteralPath $artifactPath -Algorithm SHA256
git ls-files --stage -- $artifactPath
git status --short --ignored -- $artifactPath
```

An index row proves the path is tracked. A `??` status is non-ignored untracked content and a `!!`
status is ignored content; both require the owning private/generated policy before use or handoff. No
output from `git ls-files` alone is not a tracked/private/generated boundary proof. `FullName` is a
local diagnostic only: a public handoff uses the repository-relative owner path or allowed identity
and provenance, never a private artifact's absolute machine path.

Then run the artifact's owning extractor, schema validator, H2/H3 command, or contract test and retain
its bounded summary: identity, counts, owner paths, hashes where public, validation failures, and the
minimum differing records or source excerpts required for a decision. Use
`uv run sf2 research-index list --summary` for current index totals and the owning narrow command for
fixture or runtime semantics.

Pass file paths directly to deterministic tools. Do not stream a multi-megabyte JSON document through
the console or open its complete payload in model context merely to count records. If the owning tool
reports a failure, inspect the exact failing pointer, record, source range, or callback case. If the
summary is inconclusive, expand deliberately until the claim can be judged; bounded inspection is not
permission to ignore malformed detail.

Private ROM-derived payloads, traces, captures, and extracted assets remain local. A public handoff
may include allowed identity, provenance, aggregate counts, and gate status, never the private payload
or a transcript that embeds it.

## Handoff Contract

A worker-to-root, root-to-main-gate, or correction handoff is concise and self-contained. Record the
execution task's current handoff on its Issue and link the PR; a bounded subagent reports to that
executor, which consolidates the result. Include:

1. repository, worktree, branch, exact base, head, merge base, and candidate tree;
2. PR URL and number when a PR exists, plus its Draft/open, mergeable, and check state;
3. clean worktree status and explicit proof that the local candidate head equals the pushed remote
   topic head;
4. exact changed paths, classified by owner and purpose;
5. dependencies, active-lane/shared-file relationships, and required rebase order;
6. the bounded evidence or decision summary with Confirmed, Inferred, and Unknown labels where
   research claims are involved;
7. affected planner partitions and every reproduced command with PASS, FAIL, SKIPPED, or unavailable
   status plus the exact reason;
8. private/generated/tracked-boundary result;
9. findings first, ordered P0, P1, then P2, with an exact path and line, Git object, fixture, address,
   or command result;
10. residual risks and meaningful test gaps;
11. ACCEPT, ACCEPT-WITH-FOLLOW-UP, CORRECTION-REQUIRED, or REJECT, plus the smallest correction owner
   and path scope when applicable; and
12. the Issue, execution task identity, next stopping condition, owned process state, and any local
   environment/result retention needs. Keep absolute private paths in the local handoff, not public
   Issue/PR text. Task retirement follows [Project governance](./github-project-governance.md#worktree-selection-and-retirement).

Exact base, head, and tree identify Git objects but do not prove that the reviewed handoff matches the
pushed remote candidate. For a PR handoff, reproduce the freeze before review:

```powershell
git status --short --branch
git rev-parse HEAD
git rev-parse '@{upstream}'
gh pr view --json number,url,state,isDraft,mergeable,headRefOid,statusCheckRollup
```

The local and upstream object IDs must match, and `headRefOid` must name that same head. Record the
actual check conclusions rather than only saying that checks exist. Before a worker-to-root handoff
has a PR or remote topic, mark PR identity, PR state, and remote equality **NotApplicable**, with the
reason; do not invent remote state.

Do not attach raw prompts, accumulated chat history, repeated progress commentary, complete command
transcripts, or entire large artifacts. A command's relevant failure excerpt is evidence; unrelated
successful output is summarized.

## Consolidated Independent Review

For an engine change, check the behavior and responsibility boundary before counting passing gates:

- Does legality follow live state, content and a supported rule, or a fixed reference history?
- Can another valid actor/configuration/history use the same capability without a new production case?
- Does Application execute the reached program/turn flow, with one state authority and meaningful
  failure/atomicity, rather than publish a known endpoint or depend on Godot scheduling?
- Does a small feature force unrelated allowlist/snapshot/protocol edits, and can the owning scope
  remove that coupling instead of adding another patch?
- Do engine unit assertions have independent expected results, and is reference/probe code outside
  production and used directly without another test layer?

Use the counterexample relevant to the changed capability; this is a semantic review, not a demand
for a repository-wide test matrix. Passing a controlled trajectory does not close these questions.

The reviewer first verifies candidate identity, exact scope, dependencies, and private boundary. It
then performs one bounded semantic pass across every owned path and its direct contracts. Findings
that are independently discoverable in the original candidate return as one consolidated batch with
severity, evidence, owner, and smallest correction scope.

Report a destructive or externally harmful P0 immediately; consolidation is never a reason to delay
containment. A later correction round is appropriate when the correction introduces a new defect, the
base advances, a required check was unavailable, or new evidence changes the review boundary. Do not
intentionally drip-feed issues that were visible in the same original candidate.

After correction, independently inspect the correction diff and perform only invalidated checks
required by its scope. Documentation uses direct checks; new-engine behavior uses useful unit tests
and any affected direct verification; research keeps its owning requirements. Preserve completed full
results and failures under the dependency rules in `AGENTS.md` and ADR 0012. Main-gate integration
remains serialized and separate from investigator or worker acceptance.
