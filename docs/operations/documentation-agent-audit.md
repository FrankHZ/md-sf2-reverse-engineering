# Documentation and Agent Instruction Audit

## Scope and evidence

Audited accepted base: `9fb9727e260416a54aeb3421454e610e00884ed5`, tree
`27c21094f15deb9b449b7951d6d69d908a4f8f39`. This audit follows the
[engine architecture findings](../../remake/docs/architecture-audit.md) and the merged
[ADR 0019 design proposal](../decisions/0019-state-and-content-driven-remake-engine.md).

The audit covers the task-entry graph: root guidance/README, both documentation indexes, operation
routes, relevant workflow/runtime/test ADRs, remake architecture/verification/profile/capability
owners, and the legacy Map 3/native recipes reached from them. It also reads the tracked bounded
Terra instructions/configuration and actual CI/planner/harness selection. Original research corpora
and contract contents are consumed as authorities, not re-audited or rewritten here. Personal agent
configuration, runtime code, tests, schemas, CI and planner implementation are outside this change.

**Confirmed** below means directly observable wording or code at that base. **Inferred** explains a
plausible engineering effect; it does not establish why a particular model generated a particular
patch. Model attribution and the relative causal contribution of each instruction remain **Unknown**.

## Findings and disposition

| Finding | Confirmed evidence at the audited base | Correction and remaining boundary |
| --- | --- | --- |
| D1 — P1: scope-independent gates override the new test policy | `AGENTS.md` Verification Rules/Definition of Done; ADR 0012 Decision/Test Changes; review runbook correction rule; development guide's final gate table all prescribe public-core or broad legacy gates. ADR 0019 explicitly requires engine behavior unit tests and direct verification, with no tests of verification infrastructure. | Current entry/ADR/runbook rules distinguish engine, documentation and research scopes. They explicitly identify current legacy selection and the unimplemented M0/M1 cutover. Executable CI/harness/planner changes remain separately owned. |
| D2 — P1: old-test preservation can prevent the intended migration | ADR 0017's characterization-only sequence and architecture Refactor Sequence require preserved commands/snapshots/markers before accepting refactors. The older test audit contains positive recommendations to retain those checks. | Preserve actual behavior/trust and useful independent observations; retire obsolete structure/tool/trace-refusal tests by capability. No test-count parity, replacement per deletion or old green aggregate. Historical audit recommendations are explicitly scoped. Existing tests are unchanged. |
| D3 — P1: the reference path also acts as a feature queue | Map 3 plan says `Status: Active`, names a receipt-specific next stopping point and contains an Ordered Queue; README, architecture and verification duplicate the same character/round trajectories. Architecture audit A1–A7 identifies corresponding runtime coupling. | ADR 0009 now states the engine/reference boundary explicitly. The Map 3 document is a legacy implementation/reference catalog, not dispatch authority. Entry/architecture/verification link to it for exact cases instead of repeating them. Production trace guards remain open A1–A7 work. |
| D4 — P1: a screenshot prohibition is followed by runnable screenshot recipes | Development Scope prohibits screenshots and asks for instance reuse, while later sections prescribe large frame totals, image comparisons, archived source copies and repeated official gates. Presentation includes a source-copy/editor-launch/image recipe. | Remove obsolete image-launch and mode-by-mode inspection recipes from the current presentation/verification route; retain their exact prior version in Git and completed artifacts in their existing local owners. Actual state/input observation and Godot reuse control. The old probe still emits images; its separately owned change is required before another needed run. No new debug transport is claimed. |
| D5 — P2: entry documents duplicate changing detail and conflict on reading order | The English index contains copied coverage percentages and long source-corpus summaries. The Chinese index asks a fresh task to read global README/coverage before inspecting Git. Remake README and architecture repeat detailed trajectory and asset transaction data. | Compact both indexes and remake entry; retain primary owner links and move no original evidence. Chinese navigation follows Git → resume → closest owner. Scope/capability/input identities remain with their existing owners. |
| D6 — P2: environment rules mix reusable state with per-run copies | Root guidance already prefers worktree reuse, but README/ADR wording can be read as a new worktree per topic. Development's output table gives each run another source/build/import/export workspace; the official runner actually extracts editor/template/project scratch. | Make reuse explicit in README/ADR 0006 and the verification lifetime table. Separate new logs/results from reusable build/import state. Document the existing scratch-producing runner accurately and select it only for its actual lifecycle boundary. No environment/tool implementation changes. |
| D7 — P2: superseded role wording remains on an ordinary route | ADR 0004 and ADR 0018 permit direct owner execution, but ADR 0006 still mandates a Terra worker inside the research lane. The actual bounded Terra instruction is research-only and does not require a new long-lived owner. | ADR 0006 follows ADR 0018. Keep current model defaults, bounded Terra prompt/configuration and independent integration; no model switch, new task or agent benchmark. |
| D8 — P1: architectural principles exist, but the review can accept local conformity | ADR 0011 already calls for authoritative state, typed programs, semantic commands and thin adapters; ADR 0017 already rejects per-helper public protocol growth and change amplification. The architecture audit nevertheless finds history-gated rules and Godot scheduling. | Add capability-relevant review questions to the existing review owner: another valid history/configuration, genuine legality versus trace predicates, actual program/control execution, one state authority and change amplification. This is a semantic review using relevant engine assertions/direct observations, not another acceptance framework or test inventory. |

## Interpretation

**Inferred:** fixed-slice acceptance and blanket characterization rules can reward a small local patch
that preserves an existing transcript, while making removal of an architectural defect look like a
scope violation. Repeated trajectory detail in entry documents makes the current implementation easy
to mistake for the intended design. These mechanisms plausibly contributed to the observed coupling.

That is not an excuse for the implementing or reviewing model. The relevant architecture principles
were already present. A model should identify when a requested local change needs a coherent rule-level
scope instead of another special case, preserve the real source boundary, and raise the specific
ownership/design conflict. Passing existing tests alone does not establish that judgment.

The audit does not recommend another general SOLID checklist, generic framework, rule-count target,
model benchmark or mandatory agent. It removes conflicting obligations and asks the existing reviewer
to judge the actual behavior/responsibility boundary.

## Current policy versus pending implementation

- **Binding now:** engine behavior unit tests only for new automated tests; verification programs are
  used directly without tests of those programs; old tests migrate or retire by behavior; direct
  documentation checks; no screenshot acceptance; reuse the owning worktree/environment/Godot.
- **Current executable behavior:** the single Public job still runs legacy Python infrastructure
  families and whole-solution .NET tests. The planner still emits public-core and legacy remake/deleted
  test fanout. A naturally triggered existing CI run is recorded honestly; its current wiring does
  not make those selections the desired new-engine policy.
- **Pending, separately owned:** ADR 0019's proposed M0/M1 engine unit project, rule extraction/common
  path, workflow jobs, harness/planner mapping and required-check cutover, plus any necessary probe
  image-suppression/reuse work. This documentation audit performs none of them.
- **Still incomplete:** architecture findings A1–A8, natural Map 3/Battle 01 continuity and the accepted
  8C/H4 target. Documentation approval or semantic unit success does not complete product/fidelity work.

## Reproduction and review

Read the named sections at the audited base with `git show <base>:<path>`, and compare the owning
document diff. Read `.github/workflows/public-checks.yml`, `src/sf2tool/harness.py` and
`src/sf2tool/verification_plan.py` directly for current execution; no runtime launch or test of those
tools is necessary to establish their selection behavior.

Walk these representative routes when reviewing the guidance:

1. **Engine HEAL change:** resume → remake entry → architecture/ADR 0019 → consumed spell contract.
   State/resources/capability determine legality; a second valid history/package is a useful engine
   assertion. No receipt-specific production gate, test of the replay driver or automatic old suite.
2. **Documentation correction:** resume → closest owner → direct document/scope checks → committed
   planner inspection and actual CI report. Record pending legacy selection without launching another
   .NET/Godot/H3/normal/full suite or retrying a completed unavailable-input result.
3. **Research evidence change:** resume → Phase 2 runbook → exact source/fixture owner. Original
   provenance, affected evidence checks and independent acceptance remain in force. The engine policy
   does not silently erase shared research ownership or evidence.
4. **Godot input/projection problem:** verification Scope → existing actual-instance observation.
   Inspect the relevant snapshot/node/input/errors, reuse the owning installation/project/instance,
   and record an unavailable observation honestly. The legacy image-emitting probe cannot be replayed
   unchanged to obtain a fresh pass.

For this documentation slice, check relative links/anchors, fences, changed examples, table shape,
preserved primary index entries, exact owned paths/private boundary and `git diff --check`; inspect
the plan on the committed clean candidate and the naturally triggered CI outcome. No new tests of
documentation validators or guidance wording are added. Exact command results and any failures belong
in the frozen PR/commit handoff; this report does not duplicate changing gate totals.
