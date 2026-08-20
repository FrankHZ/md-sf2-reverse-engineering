# Phase 4 Bootstrap Plan

- Status: **Proposed**
- Record date: 2026-08-20
- Audit base: accepted `main` commit `c812ad61395e7b42ca91a9aee044dc04375522de`, tree
  `7da7aa9a13b148756fad5e8f31654e5f975ae782`
- Layer: **Layer B design synthesis** over accepted architecture and readiness boundaries
- Scope: future Phase 4 project topology, bootstrap admission, CLI gates, observation ports, profile
  separation, and implementation-start checklist
- Readiness effect: **none**; the Map 3 to Battle 01 ledger remains **NOT READY**

## Judgment Boundary

**Timing classification: Fixed now.**

This document translates accepted architecture into a reviewable future bootstrap plan. It consumes
[ADR 0008](../../decisions/0008-godot-csharp-cli-first-remake-tooling.md),
[ADR 0009](../../decisions/0009-first-phase4-playable-slice.md),
[ADR 0010](../../decisions/0010-map3-battle01-product-acceptance.md), and
[ADR 0011](../../decisions/0011-phase4-remake-runtime-architecture.md). It does not reopen their
Godot 4.7.2 .NET/C#, continuous-milestone, product-profile, or runtime-architecture decisions.

This is a Layer B synthesis, not original-game evidence, an evidence-bound subsystem contract, an
executable schema, or implementation. Its semantic association is deliberately empty:

- fixtures: none;
- schemas: none;
- research-index `designContracts` records: none;
- executable verifier or CLI registrations: none;
- new evidence labels, counters, or accepted command families: none.

The accepted [readiness ledger](map3-battle01-readiness.md) remains **NOT READY**, and the accepted
[Research gap audit](../../research/map3-battle01-audit.md) remains **OPEN**. This plan does not fill
any RA-01..RA-12 value, select a route or seed, define an exact state, name a new fixture, accept a
command, identify a capture, state an observable result, or choose a comparison tolerance.

This proposal does not create `remake/`, select the exact .NET SDK, target framework, or package set,
write engine code, install or run Godot, invoke editor/import/export, select or use MCP, authorize a
public release under 7C, report readiness, or start Phase 4. Every concrete project file and every
executable implementation described below remains gated by the separate user Phase 4 start action.

## Timing Vocabulary and Ownership

**Timing classification: Fixed now.**

Every section and table uses these exact timing classes:

| Timing class | Meaning | Owner boundary |
| --- | --- | --- |
| **Fixed now** | Already selected by accepted ADRs or safely derivable as architecture, interface shape, gate ordering, or failure policy without original-game values. | Design synthesis may state the rule now. |
| **Research-dependent** | Requires accepted R1-R4 evidence or the later continuous-scenario contract. | Research and evidence-bound Design owners supply identities, values, reached capabilities, comparison domains, and tolerances. |
| **After separate user Phase-4 start** | Creates or executes the modern-engine implementation. | The later implementation lane materializes files, code, schemas, runners, scenes, packages, adapters, and gates. |

An item classified **Fixed now** can constrain a future implementation without authorizing that
implementation. An interface name in this document is a prospective design label, not a fixture ID,
schema ID, executable type, or claim that its payload is known.

## Accepted Inputs and Retained Gaps

**Timing classification: mixed; each row is classified explicitly.**

| Input | Usable conclusion | Timing class | Retained boundary |
| --- | --- | --- | --- |
| ADR 0008 | Godot 4.7.2 .NET/C#, CLI-first gates, an explicit future SDK and locked packages, plain-C# logic, and optional removable MCP | **Fixed now** | Exact SDK, target framework, package graph, editor artifact path/hash, and any MCP remain unselected. |
| ADR 0009 | One continuous playable Map 3 through Battle 01-completion milestone with a separate start action | **Fixed now** | No route, state, or implementation is supplied. |
| ADR 0010 | Product profile `1A+2A+3A+4A+5B+6A+7C+8C+9A+10A` | **Fixed now** | Every Research-owned exact field, route, trace, private input, output, and tolerance remains open. |
| ADR 0011 | Four production assemblies, inward dependencies, authoritative plain-C# state, thin Godot adapters, package admission, deterministic abstractions, and layered H4 | **Fixed now** | No project, schema, backend, or test host exists yet. |
| Readiness ledger and gap audit | Ten scenario observation layers and RA-01..RA-12 ownership classes | **Research-dependent** | Their missing values and executable definitions cannot be inferred here. |
| Disposable Godot probe | Bounded evidence for direct CLI build/import/run and process cleanup on one evaluated environment | **Fixed now** for process-safety lessons only | Its project shape, runtime result, host, framework, renderer, and code are not production architecture. |

## Future Project and Assembly Topology

**Timing classification: Fixed now for names, layout, dependencies, and forbidden directions; After
separate user Phase-4 start for every directory and project file.**

The future runtime has exactly four production projects and assemblies. The names and dependency
directions match ADR 0011 exactly.

| Production assembly | Allowed production references | Required ownership | Forbidden ownership or dependency | Timing class |
| --- | --- | --- | --- | --- |
| `Sf2.Remake.Domain` | .NET base-library allowlist only | typed IDs and values, authoritative substate, deterministic formulas/reducers, explicit RNG transitions, legal-state checks, domain events | every remake assembly, Godot APIs, files, JSON, wall clocks, input devices, rendering/audio, private paths, ROM/source addresses | **Fixed now** |
| `Sf2.Remake.Application` | `Sf2.Remake.Domain` | `GameSession`, flow coordinator, typed command admission, ports, ordered cues, application observation envelopes | Godot APIs, concrete file formats, raw private inputs, implicit global services, alternate authoritative state | **Fixed now** |
| `Sf2.Remake.Content` | `Sf2.Remake.Application`, `Sf2.Remake.Domain` | concrete package readers/validators, DTO mapping, logical content and asset catalogs | ROM/disassembly parsing at runtime, gameplay mutation, Godot scene state, silent defaults | **Fixed now** |
| `Sf2.Remake.Godot` | `Sf2.Remake.Application`, `Sf2.Remake.Domain`, `Sf2.Remake.Content`, Godot 4.7.2 .NET APIs | one composition root, scenes, semantic input adapter, render/audio/capture adapters, platform lifecycle | gameplay truth, formulas, RNG choice, route truth, fixture expectations, private redistribution | **Fixed now** |

The prospective layout remains the ADR 0011 ownership map:

```text
remake/
  Sf2.Remake.sln
  src/
    Sf2.Remake.Domain/
    Sf2.Remake.Application/
    Sf2.Remake.Content/
  game/
    project.godot
    Sf2.Remake.Godot.csproj
    Scenes/
    Scripts/
  tests/
    Sf2.Remake.Domain.Tests/
    Sf2.Remake.Application.Tests/
    Sf2.Remake.Content.Tests/
    Sf2.Remake.Godot.Tests/
    Sf2.Remake.H4.Tests/
```

The five test projects and any future CLI or H4 runner are outer, non-production consumers. A CLI
gate does not imply a fifth production runtime assembly. A later CLI host may reference
`Application` and `Content` only as ADR 0011 permits; no production project references a test, H4,
CLI, runner, or Godot editor integration project.

The following topology assertions are fixed now and become executable only after the separate start:

- the project-reference graph contains no other production edge;
- `Domain` has no Godot or adapter dependency;
- `Application` owns ports but not concrete adapters;
- `Content` and `Godot` implement inward-facing ports without becoming state owners;
- Godot signals, animation callbacks, autoloads, and scene changes cannot directly mutate gameplay;
- H4 and CLI hosts can be deleted without breaking production project compilation.

## Toolchain Lock and Bootstrap Admission

**Timing classification: Fixed now for required lock fields, validation order, and failure policy;
After separate user Phase-4 start for the exact SDK, target framework, packages, files, and artifact
hashes.**

ADR 0008 fixes Godot 4.7.2 and requires an explicit .NET SDK plus locked packages. It does not select
the exact SDK build, target framework, package set, NuGet sources, or editor artifact. The authorized
bootstrap slice must select and review those values before it creates the first project; this proposal
does not choose them.

| Future lock/admission field | Requirement | Timing class |
| --- | --- | --- |
| Godot release line | must be Godot 4.7.2 .NET unless a later accepted ADR changes it | **Fixed now** |
| Godot official build identity and artifact digest | exact full version/build, acquisition source, archive/executable digest, platform, and local resolution rule | **After separate user Phase-4 start** |
| .NET SDK and target framework | exact SDK build, roll-forward policy, target framework, compatible runtime assumptions, and local discovery rule | **After separate user Phase-4 start** |
| NuGet dependency closure | direct and transitive package IDs, exact versions, content hashes, approved sources, restore-lock mode, and license/provenance review | **After separate user Phase-4 start** |
| deterministic build properties | nullable/language settings, warnings policy, deterministic/CI flags, output roots, and generated-file ignores | **After separate user Phase-4 start** |
| export toolchain | export templates/artifact identities, preset names, target runtime dependencies, output root, and signing-secret exclusion | **After separate user Phase-4 start** |
| admission result | one typed report containing every expected identity, observed identity, mismatch, and unsupported capability before mutable import/build/run work | **Fixed now** |

The future tracked lock surface must use normal .NET/Godot lock mechanisms plus a project-owned
manifest sufficient to reproduce the table above. The exact file names and contents are chosen in
the authorized bootstrap slice. Machine paths and credentials remain local configuration and never
become tracked lock values.

Floating SDK selection, floating package ranges, implicit restore during the maintained build gate,
unhashed downloaded executables, and an editor installation found only through ambient machine state
are not admitted. An unsupported or mismatched toolchain fails before project mutation; it cannot be
reported as a successful compatibility fallback.

## Future CLI-First Gate Contract

**Timing classification: Fixed now for ordering, inputs, receipts, and failure boundaries; After
separate user Phase-4 start for command implementation and execution.**

The maintained gate is official-CLI-first and editor-plugin-independent. Command text below is a
future command shape, not an instruction to execute Godot before the separate start.

| Order | Future gate | Required result | Plan class | Execution class |
| ---: | --- | --- | --- | --- |
| 0 | toolchain preflight | exact admitted Godot/.NET/package-source identities before scratch or generated state | **Fixed now** | **After separate user Phase-4 start** |
| 1 | locked restore | restore succeeds only against the selected lock and approved sources | **Fixed now** | **After separate user Phase-4 start** |
| 2 | build without implicit restore | all four production projects and selected outer hosts compile with the warnings policy | **Fixed now** | **After separate user Phase-4 start** |
| 3 | pure-C# tests | Domain then Application then Content tests pass without Godot | **Fixed now** | **After separate user Phase-4 start** |
| 4 | architecture tests | project graph, forbidden APIs, ownership, profiles, and package boundaries pass | **Fixed now** | **After separate user Phase-4 start** |
| 5 | official headless editor import | imported scene/resource/C# state is valid and produces a typed import receipt | **Fixed now** | **After separate user Phase-4 start** |
| 6 | bounded headless scene-state smoke | composition root reaches the declared synthetic smoke endpoint and emits a machine-readable receipt | **Fixed now** | **After separate user Phase-4 start** |
| 7 | layered H4 profiles | only layers with accepted definitions and admitted capabilities execute; missing layers remain explicit failures or unsupported results | **Research-dependent** | **After separate user Phase-4 start** |
| 8 | export smoke | local-private or public-synthetic profile exports only to its declared ignored output root | **Fixed now** | **After separate user Phase-4 start** |
| 9 | artifact and cleanup audit | tracked/private/generated/export payload scan, process-survivor check, and final gate receipt pass | **Fixed now** | **After separate user Phase-4 start** |

The future restore/build/test command family follows the semantic order `restore --locked-mode`,
`build --no-restore`, then `test` without a second implicit restore or build. Exact command-line
spelling follows the SDK chosen after the separate start. Godot import, run, and export use the
official accepted executable, an explicit project path, headless mode, bounded shutdown, and an
explicit output/profile. A zero exit code without the expected typed receipt is failure.

The exact reference, interactive milestone, and public-synthetic profiles remain separate gate
invocations. Public-synthetic export uses only redistribution-safe tracked inputs. A successful
local-private or public-synthetic export never authorizes a public release under 7C.

## Process, Scratch, and Diagnostic Safety

**Timing classification: Fixed now for lifecycle requirements; After separate user Phase-4 start for
the runner and its tested wall-clock values.**

Every external process in the future gate has a finite, tracked wall-clock budget. The exact numeric
budgets are selected and tested with the bootstrap implementation; no step may wait indefinitely.
Every native launch uses an argument list with no shell and declares its working directory, input
profile, output root, and diagnostic limit.

On timeout or cancellation, the runner must:

1. attribute the failure to the exact gate and wall-clock budget;
2. request termination of the complete process group/tree, not only the immediate parent;
3. bound the terminator process and its reap;
4. retain a direct-process termination fallback;
5. bound the target process and pipe reap;
6. close capture handles when reaping cannot complete;
7. check for surviving project-owned child processes;
8. emit a failed cleanup receipt even when a gameplay/import marker was previously observed.

Build, import, run, capture, and export mutations use a newly created, validated directory under the
ignored local boundary or an ephemeral CI root. The runner rejects existing paths, repository roots,
paths outside the declared scratch root, and ambiguous output ownership. It does not recursively
delete a caller-supplied or pre-existing directory. Scratch may remain for bounded diagnostics; its
presence is reported and can never make a failed gate pass.

Diagnostics include exit status, timed-out step, admitted tool identities, a bounded stdout/stderr
tail, cleanup actions, survivor status, and receipt/output paths. They exclude credentials, complete
private content, captures, decoded assets, and unbounded logs.

## Pure-C# State and Thin Godot Composition

**Timing classification: Fixed now for ownership and ports; Research-dependent for scenario payloads;
After separate user Phase-4 start for types, nodes, and wiring.**

`Domain` returns deterministic transition results and state facts. It does not call observers,
devices, files, clocks, renderers, or Godot. `Application` owns `GameSession`, one command-at-a-time
admission, the `GameFlowCoordinator`, presentation cues, acknowledgments, application ports, and
semantic observation envelopes. `Content` validates and maps packages before session creation.

The coordinator preserves ADR 0011's architectural stage order: `AdmittedStart` -> `Exploration` ->
`MapOrProgramTransition` -> `BattleAdmission` -> `BattleInitialization` -> `BattleTurns` ->
`BattleOutcome` -> `AfterBattleProgram` -> `ReturnHandoff` -> `StableControllableEndpoint`. Those are
**Fixed now** orchestration boundaries, not a claim that exact transition predicates or optional
substates are known. Their scenario identities, payloads, natural route, input sequence, program
effects, and endpoint facts are **Research-dependent**; executable states and transitions are **After
separate user Phase-4 start**.

`Sf2.Remake.Godot` has one composition root. It creates or receives the session, validated content,
profile, observers, asset resolver, input mapping, renderer, audio, and capture adapters. Scenes own
only disposable view instances, interpolation, focus, resources, and animation state. Rebuilding a
scene from one application snapshot cannot change gameplay.

| Boundary | Fixed interface responsibility | Research-dependent payload | Implementation timing |
| --- | --- | --- | --- |
| Domain transition | accept typed values/state and return next state plus ordered deterministic facts | accepted formulas, state fields, legal actions, RNG call families | **After separate user Phase-4 start** |
| Application command | admit logical command, apply reducers, emit cues and semantic observation envelope | accepted route/program/action/endpoint definitions | **After separate user Phase-4 start** |
| Content source | return admitted domain definitions or typed admission failure | accepted scenario records, program families, asset/content IDs | **After separate user Phase-4 start** |
| Godot composition root | wire one application session to view/input/render/audio/capture adapters | reached scene/resource capabilities and exact presentation requirements | **After separate user Phase-4 start** |
| H4 host | collect outer receipts and compare only accepted fields | fixture owners, expected values, captures, comparator domains, tolerances | **After separate user Phase-4 start** |

The boundary responsibilities are **Fixed now**. Every payload named in the third column is
**Research-dependent**; this plan supplies none of it.

## H4 Observation Ports and Receipt Envelopes

**Timing classification: Fixed now for prospective labels, common fields, ownership, and ten-layer
crosswalk; Research-dependent for payload membership and expectations; After separate user Phase-4
start for interfaces, serialization, capture, and comparison code.**

The following names are prospective design labels only. They are not fixture IDs, schema IDs,
accepted executable types, or claims that any receipt can be populated today.

Domain transitions remain returned data. An application observation-sink port receives semantic
application envelopes. Content exposes admission and capability reports. Godot exposes adapter and
capture observations from the outer assembly. The H4 host depends outward on these surfaces,
assembles run receipts, applies redaction, and performs comparisons; no production assembly depends
on the H4 test project.

Every future receipt envelope has this field shape:

| Field | Purpose | Timing class |
| --- | --- | --- |
| `schemaVersion` | identify the future receipt format | **Fixed now** name; value **After separate user Phase-4 start** |
| `runId`, `profileId`, `scenarioId` | correlate one admitted run without embedding private payloads | **Fixed now** names; identities **Research-dependent** or implementation-owned |
| `layerId`, `receiptKind` | select one readiness layer and specialized receipt | **Fixed now** |
| `sequence`, `simulationStep` | preserve total order and deterministic application time | **Fixed now** fields; exact values **Research-dependent** |
| `producerId`, `capabilities` | identify the producing boundary and Supported/Unsupported/NotApplicable capabilities | **Fixed now** shape; reached capabilities **Research-dependent** |
| `contractOwnerIds` | reference accepted owners without copying expectations | **Fixed now** field; exact set **Research-dependent** |
| `payloadDigest` | bind a canonical future receipt payload without publishing private content | **Fixed now** field; algorithm/value **After separate user Phase-4 start** |
| `status`, `diagnostics` | Pass/Fail/Unsupported/NotRun plus typed bounded failure data | **Fixed now** shape; result **After separate user Phase-4 start** |

The readiness ledger's ten scenario layers refine ADR 0011's seven architectural observation
boundaries. The crosswalk preserves both accepted owners:

| Readiness layer | Prospective receipt label | Field/capability shape only | ADR 0011 observation boundary | Payload class | Implementation class |
| ---: | --- | --- | ---: | --- | --- |
| 1 | `AdmissionReceipt` | package/profile/provenance capability, start-state digest presence, private-input hash-check status | 1 | **Research-dependent** | **After separate user Phase-4 start** |
| 2 | `ExplorationInputReceipt` | logical action identity/phase/order, admitted step, exploration handoff and acknowledgment capability | 2 | **Research-dependent** | **After separate user Phase-4 start** |
| 3 | `ProgramStateReceipt` | map/setup/event/program/dialogue/roster/flag transition identities and before/after digest capability | 2, 3 | **Research-dependent** | **After separate user Phase-4 start** |
| 4 | `BattleAdmissionReceipt` | trigger/handoff/cutscene/initialization identities, order, and initialized-state digest capability | 2, 3 | **Research-dependent** | **After separate user Phase-4 start** |
| 5 | `BattleTraceReceipt` | turn/input/movement/target/AI/RNG/action/resolution/replay/after-turn identities and order capability | 3, 4 | **Research-dependent** | **After separate user Phase-4 start** |
| 6 | `CompletionFlowReceipt` | victory/after-program/return/handoff identities and completion-order capability | 2, 3, 4, 7 | **Research-dependent** | **After separate user Phase-4 start** |
| 7 | `StableEndpointReceipt` | stable-control predicates, endpoint-state digest, and named field-fact capability | 3, 7 | **Research-dependent** | **After separate user Phase-4 start** |
| 8 | `MilestoneProfileReceipt` | 6A no-save assertions, 7C asset/profile identity, provenance and private-boundary capability | 1, 5, 7 | **Research-dependent** | **After separate user Phase-4 start** |
| 9 | `ExactFidelityReceipt` | 8C framebuffer/palette/frame/animation/audio/VInt/DMA/CRAM/VDP observation capability, capture-condition reference, and exact-comparison or permitted field-specific-tolerance-policy reference | 4, 5, 6 | **Research-dependent** | **After separate user Phase-4 start** |
| 10 | `ExpectedDeviationReceipt` | 9A/10A profile, owner, rationale, affected layer, expected-result category and state-equivalence capability | 7 | **Research-dependent** | **After separate user Phase-4 start** |

The receipt names, field categories, ownership, order requirement, and crosswalk are **Fixed now**.
Every exact member set, identity, digest, expected result, observable, capture condition, comparison
domain, and tolerance is **Research-dependent**. Serialization, redaction, comparison, and executable
ports are **After separate user Phase-4 start**.

Production domain code never exposes original RAM/ROM addresses for these receipts. An outer H4
adapter may translate accepted fixture fields at the comparison boundary. One layer cannot substitute
for another, and an engine screenshot or zero-exit launch cannot mask a semantic mismatch.

## Content Packages, IDs, and Import Admission

**Timing classification: Fixed now for package envelope and validation categories;
Research-dependent for scenario records, logical IDs, command families, and capabilities; After
separate user Phase-4 start for schemas, exporters, readers, DTOs, and package files.**

The runtime never parses a ROM or disassembly. Existing Research tooling remains the evidence and
export source. A future versioned package envelope contains these categories:

| Envelope category | Required shape | Timing class |
| --- | --- | --- |
| package identity | package ID, kind, schema version, content digest | **Fixed now** fields; exact identities **Research-dependent** |
| contract ownership | accepted contract IDs and dependency package IDs | **Fixed now** fields; exact sets **Research-dependent** |
| provenance and rights | public-synthetic or private-local class, producer/version, rights classification | **Fixed now** fields; exact private inventory **Research-dependent** |
| catalogs | typed logical content/asset IDs and cross-reference inventory | **Fixed now** shape; exact IDs **Research-dependent** |
| capabilities | admitted program/action/presentation/profile capabilities | **Fixed now** shape; reached set **Research-dependent** |
| compatibility | required package versions, dependency digests, and profile constraints | **Fixed now** validation category; exact values **After separate user Phase-4 start** or **Research-dependent** |

`Application` retains narrow source ports for scenario definitions, gameplay definitions, typed
program data, and logical asset metadata. `Content` returns an `ImportAdmissionReport`-shaped result.
That prospective label covers these validation categories:

- identity, version, digest, and dependency closure;
- duplicate, range, and admitted-domain checks;
- logical-ID and cross-reference closure;
- command-family and profile capability completeness;
- provenance/rights class and public/private separation;
- typed expected/actual diagnostics with no silent default.

All content required for an admitted session validates before session creation. An unresolved ID,
unsupported command, missing capability, digest mismatch, duplicate, or profile conflict fails
closed. Similar content is not an authorized substitute.

The private 7C local-only adapter and public-synthetic profile use the same logical IDs and inward
ports but separate outer resolvers, packages, outputs, and claims. Missing or mismatched private input
makes the exact profile unavailable. It never falls back to synthetic data while claiming 7C/8C.
Public CI uses only redistribution-safe fixtures and project-authored synthetic content. Neither
profile authorizes a public release of original content.

## Deterministic Clock, RNG, and Logical Input

**Timing classification: Fixed now for abstractions and prohibitions; Research-dependent for cadence,
seed, call order, and input trace; After separate user Phase-4 start for implementation and tests.**

The future application receives explicit deterministic clock, RNG transition, and logical-input
boundaries. Integer simulation steps and monotonic sequence numbers order gameplay. Interactive wall
time may pace already-decided steps only in the Godot adapter.

Gameplay in `Domain` or `Application` must not depend on:

- `System.Random`;
- Godot random generators;
- system time, time-derived seeds, or hidden mutable streams;
- `_Process` or physics delta time;
- thread scheduling, locale, or unordered collection iteration;
- animation duration, audio completion, or render frame rate.

An RNG operation accepts explicit state and call-family input and returns the result, next state, and
ordered observation fact. The exact algorithmic behavior is implemented only from accepted owners.
The seed, complete reached call order, copy/retry behavior, deterministic cadence, logical input
trace, repeat timing, and physical-device timing remain **Research-dependent**.

Godot `InputMap` translates keyboard and controller input into semantic direction, confirm, cancel,
and menu actions. Device mapping remains adapter configuration. The application decides whether a
logical action is admitted in the current flow state. H4 uses the same logical boundary as manual
play and cannot inject domain mutations directly.

## Milestone Profile Composition

**Timing classification: Fixed now for 6A/7C/8C/9A/10A composition rules;
Research-dependent for exact private/fidelity/deviation payloads; After separate user Phase-4 start
for adapters, settings, and executable checks.**

| Profile boundary | Fixed composition rule | Research-dependent input | Implementation class |
| --- | --- | --- | --- |
| 6A no save | no save/load/checkpoint/suspend adapter, action, or UI is registered; restart destroys the session and asks the admitted-start source for a fresh accepted snapshot; harness snapshots are not saves | admitted snapshot definition and digest | **After separate user Phase-4 start** |
| 7C private local | original content resolves only through ignored local inputs with admitted provenance/hashes; tracked/public paths never contain payloads | reached private inventory, provenance, hashes, rights, and capture conditions | **After separate user Phase-4 start** |
| public synthetic | tracked project-authored content exercises build, logic, adapter, and export safety without an original-fidelity claim | synthetic package authored after start | **After separate user Phase-4 start** |
| 8C exact reference | a replaceable fidelity backend exposes every accepted reached observation; unsupported capability fails or remains unsupported and cannot relax a golden | reached values, ordering and chronology, capture domain, comparators, and exact comparison or only the explicitly field-specific tolerances a future accepted H4 contract permits under ADR 0010 | **After separate user Phase-4 start** |
| 9A accessibility | remapping, confirm/cancel convention, reduced flash, and text progression stay at input/presentation boundaries and preserve applicable gameplay state | exact executable state-equivalence/deviation expectations | **After separate user Phase-4 start** |
| 10A deviations | each deviation names profile, owner, rationale, affected layer, expected-result category, and scope; silence authorizes nothing | accepted scenario-specific expected deviations | **After separate user Phase-4 start** |

The exact reference and accessibility runs are distinct. Accessibility output can share semantic
completion facts but cannot satisfy or weaken exact 8C presentation. No-save composition is proven
by absent registration and unreachable player commands, not by leaving a save button hidden.

## Future Executable Architecture Tests

**Timing classification: Fixed now for assertion categories and failure meaning; After separate user
Phase-4 start for test code, analyzers/libraries, and CI registration; Research-dependent only where a
test consumes scenario capabilities.**

The bootstrap must make architecture rules executable without creating a framework for unrelated
future features. Exact test libraries are selected and locked with the future package audit.

| Test category | Required assertion | Timing class |
| --- | --- | --- |
| production graph | exactly four production projects and only the ADR 0011 inward reference edges | **Fixed now** requirement; implementation **After separate user Phase-4 start** |
| Domain purity | no Godot, file/JSON, device, clock, private-path, CLI/H4, or outer-project dependency | **Fixed now** requirement; implementation **After separate user Phase-4 start** |
| deterministic API | forbid gameplay use of `System.Random`, Godot RNG, wall-clock branches, delta-time branches, and hidden ambient state | **Fixed now** requirement; implementation **After separate user Phase-4 start** |
| application ownership | one session mutation facade; no adapter directly mutates authoritative state | **Fixed now** requirement; implementation **After separate user Phase-4 start** |
| Godot composition | one composition root; autoloads/signals/scenes contain no parallel gameplay model | **Fixed now** requirement; implementation **After separate user Phase-4 start** |
| content admission | invalid version/digest/duplicate/range/reference/capability/profile cases fail closed | **Fixed now** shape; cases **Research-dependent** or implementation-owned |
| no-save composition | no persistence adapter, player command, action, or UI registration in milestone profile | **Fixed now** requirement; implementation **After separate user Phase-4 start** |
| H4 layers | all ten receipt kinds remain separately attributable; unsupported/missing cannot pass | **Fixed now** requirement; exact payload **Research-dependent** |
| profile separation | exact-reference, accessibility, and public-synthetic claims cannot be confused or silently substituted | **Fixed now** requirement; implementation **After separate user Phase-4 start** |
| process cleanup | timeout, tree termination, reaping, closed pipes, bounded diagnostics, scratch safety, and survivor failure are tested without Godot where possible | **Fixed now** requirement; implementation **After separate user Phase-4 start** |
| export/private scan | public-synthetic export contains no private/local/generated input and private export makes no public-release claim | **Fixed now** requirement; implementation **After separate user Phase-4 start** |

A grep-only check is not sufficient when a compiler, project graph, analyzer, or negative test can
enforce the rule. A new analyzer or test dependency still requires the locked dependency and license
review; this document does not choose one.

## Future `remake/` Creation Checklist

**Timing classification: Fixed now for checklist and ordering; Research-dependent for readiness/H4
inputs; After separate user Phase-4 start for every checked action.**

The first authorized bootstrap slice must not begin until main-gate reports readiness and the user
separately starts Phase 4. It then performs this checklist in order:

1. record the exact accepted `main`, readiness report, and separate user start action;
2. verify all Research-dependent scenario/H4 owners required for the first implementation step are
   accepted and reference them without copying their goldens;
3. select and review the exact .NET SDK, target framework, Godot official build artifact, NuGet
   sources, direct/transitive packages, licenses, hashes, and restore-lock files;
4. create only the accepted four-production-project topology and named outer test projects;
5. add generated-path ignores before any restore, build, import, capture, or export;
6. implement the toolchain preflight and locked restore boundary before normal build;
7. add architecture and process-cleanup tests before feature code expands the graph;
8. implement public-synthetic package admission and smoke before wiring any private-local resolver;
9. prove 6A no-save registration and 9A/10A profile separation at composition;
10. add official import, headless, and public-synthetic export receipts plus artifact scans;
11. add private 7C and exact 8C capabilities only from accepted private inventories and H4 owners;
12. rerun the CLI gates with no MCP dependency and review the complete tracked/generated/private
    boundary.

The checklist does not require or select MCP. Any later MCP evaluation remains removable under ADR
0008 and must leave the official CLI results unchanged.

## Disposable Probe Lessons and Exclusions

**Timing classification: Fixed now for bounded process/tooling lessons and explicit exclusions;
After separate user Phase-4 start for any independently authored production runner.**

The accepted [Godot AI probe](../../../tools/godot-ai-probe/README.md) is disposable capability and
tooling evidence. Its safe, bounded lessons are:

- check the exact Godot version before scratch/build/import mutation;
- copy declared tracked inputs into a fresh ignored scratch root;
- separate build, editor import, and headless run gates;
- apply finite wall-clock limits to every external process;
- terminate the process tree and bound terminator/target reaping and pipe cleanup;
- bound diagnostics and require asserted completion output rather than zero exit alone.

The following probe details are non-normative and must not be copied as production architecture:

- its one-project layout and node-owned simulation;
- `System.Random`, its seed/result, and its 60-`_Process`-frame stdout golden;
- its target framework, .NET host observation, SDK, or local editor path;
- its GL Compatibility renderer choice;
- its exact nine-file scratch inventory;
- its application/assembly-name workaround;
- its probe scene, positions, score, or output strings;
- its lack of a locked restore, package audit, export gate, ten-layer H4 receipt, and four-project
  architecture tests.

The future runner may reuse the process-safety requirements, but it must be project-owned production
tooling written and reviewed after the separate start. The probe project never moves into `remake/`
and never becomes an implementation dependency.

## Three-Way Timing and Ownership Split

**Timing classification: mixed; each row is classified explicitly.**

| Work item | Timing class | Durable owner |
| --- | --- | --- |
| four-project graph, dependency prohibitions, outer-host rule | **Fixed now** | ADR 0011 plus this synthesis |
| CLI gate order, scratch/process lifecycle, failure attribution | **Fixed now** | ADR 0008/0011 plus this synthesis |
| prospective port/receipt/envelope field categories and ten-layer crosswalk | **Fixed now** | this synthesis, subordinate to accepted ADR/readiness owners |
| package-envelope and admission-validation categories | **Fixed now** | ADR 0011 plus this synthesis |
| deterministic clock/RNG/input abstractions and forbidden ambient APIs | **Fixed now** | ADR 0011 plus accepted subsystem owners |
| 6A/7C/8C/9A/10A composition and claim separation | **Fixed now** | ADR 0010/0011 plus this synthesis |
| exact admitted and endpoint state, selected setup/program/content IDs, natural route/admission | **Research-dependent** | R1-R4 and the future continuous-scenario contract |
| complete battle trace/seed, private inventory/captures, reached observables, comparison domains/tolerances | **Research-dependent** | R1-R4 and accepted H4/scenario owners |
| exact receipt members, contract associations, command capability set, package records and schemas | **Research-dependent** before implementation | future continuous-scenario and evidence-bound contract owners |
| exact SDK/TFM/packages/artifacts, files/projects/code/tests/runners/scenes/adapters/backends/export presets | **After separate user Phase-4 start** | authorized implementation/bootstrap slices |
| Godot import/run/export and H4 execution | **After separate user Phase-4 start** | authorized implementation and milestone gates |
| optional MCP evaluation/adoption | **After separate user Phase-4 start**, if separately selected | optional tooling decision; never a maintained dependency |

## Research and Shared-File Serialization

**Timing classification: Fixed now.**

This document consumes accepted `main` only. Research R1-R4 may change Research-owned documents,
fixtures, schemas, manifests, tooling, and evidence-bound contracts. This synthesis neither edits
those paths nor consumes an unmerged conclusion. It can name RA-01..RA-12 only as unresolved ownership
classes.

Phase 1 owns exactly this document. After preliminary semantic review, a separate registration phase
may add only:

1. this document;
2. one minimal synthesis entry in `docs/README.md`;
3. one pending source entry in `manifests/zh-translation-index.json`.

Before that registration, the owner must refresh accepted `main`, inspect every delta, and recheck
shared ownership. `docs/README.md` and the zh manifest serialize after any overlapping Research,
Design, or localization lane. No zh-CN mirror belongs to this slice; translation is a later dedicated
localization batch.

An accepted R1-R4 result may be linked by its owning future scenario contract. It does not cause this
bootstrap plan to absorb exact values, fixture associations, or expected outputs. If accepted
Research changes an architecture category rather than a payload, main-gate must assign an explicit
Design revision instead of silently changing this document during conflict resolution.

## Proposed Acceptance Gates

**Timing classification: Fixed now for this document slice; these are documentation gates, not Phase
4 implementation gates.**

Phase 1 requires:

- exactly one tracked path: this new document;
- every local link resolves against accepted `main`;
- English canonical text, no unintended Han characters, balanced fences/tables/Mermaid, and clean
  diff/whitespace;
- no private/generated/binary payload, local machine path, credential, ROM, capture, extracted asset,
  or executable tool;
- every section and table retains the **Fixed now**, **Research-dependent**, or **After separate user
  Phase-4 start** classification;
- no invented fixture/schema/research-index association, route, state, seed, command, capture hash,
  observable value, comparison domain, or tolerance;
- ADR 0011 assembly names/dependencies and the readiness ten-layer ordering remain exact;
- design-contract and research-index counters remain unchanged;
- Ruff, the shared native harness, normal repository verification, dependency planning, and Public
  tracked-input checks pass;
- the old full profile is not run for this design-only addition.

Strict zh metadata validation belongs to the later shared registration because Phase 1 deliberately
does not edit the zh manifest. If the current 74-translated/zero-pending baseline is unchanged, adding
this source as pending will make the registered denominator 75 documents: 74 translated and one
pending. That future counter is not part of Phase 1 acceptance.

## Proposal Effect

**Timing classification: Fixed now.**

Accepting this synthesis would make the future Phase 4 bootstrap reviewable without making the
scenario ready or starting implementation. It would not create a project, select a toolchain beyond
accepted Godot 4.7.2, install or run Godot, adopt MCP, populate H4, authorize private redistribution,
or change any Research gap.

The readiness ledger remains **NOT READY**. R1-R4, the continuous-scenario contract, complete H4
definitions, private-input acceptance, main-gate readiness, and the separate user Phase 4 start action
all remain required.
