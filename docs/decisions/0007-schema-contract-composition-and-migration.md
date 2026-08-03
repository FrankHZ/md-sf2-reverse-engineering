# ADR 0007: Separate Schema Shape from Golden Evidence

- Status: **Accepted**
- Decision date: 2026-08-02
- Scope: tracked JSON schemas, H2/H3 fixtures, schema validation, and legacy-schema migration

## Context

The repository intentionally uses closed JSON schemas, tracked fixtures, canonical output hashes, and
domain-specific verifiers together. That combination has made structural drift, legal-shape semantic
replacement, ordering errors, and cross-field inconsistencies independently diagnosable.

Several mature H2 and H3 contracts now duplicate the same evidence across those layers. In particular,
some schemas contain complete arrays or objects as `const` values, while the paired fixture stores the
same values and the verifier separately compares the fixture with generated output and checks a
canonical digest. Output and fixture schemas also copy large definition trees instead of sharing one
structural contract.

This is not a correctness failure. It is a composition and maintenance problem: exact corpus data has
gradually turned some schemas into a second fixture, and paired schemas have become synchronized copies.

## 2026-08-02 Audit

The audit used clean `main` at `6c46d86569b5edbfb101032ed6c0af2ebed311e0`. It recursively parsed
every tracked `schemas/**/*.schema.json`, measured file and `const` payload sizes, compared matching
output/fixture definitions, inspected the owning verifiers and mutation tests, and timed validation of
the four largest H2 fixture contracts.

The audited tree contained:

- 254 schema files totaling 16,245,976 bytes (about 15.5 MiB);
- 248 frozen legacy-root files totaling 16,150,792 bytes and six namespaced H3 files totaling 95,184
  bytes;
- six schemas larger than 1 MiB, 18 larger than 100 KiB, and 27 larger than 50 KiB;
- 22,156 `const` keywords whose compact serialized payloads total 4,814,677 characters.

The four largest output/fixture pairs account for 12,497,188 bytes, about 77% of the complete schema
tree:

| Contract | Output schema | Fixture schema | Combined |
| --- | ---: | ---: | ---: |
| map-script engine | 2,367,692 | 2,582,281 | 4,949,973 |
| common menus | 1,651,814 | 1,651,299 | 3,303,113 |
| common stats | 1,273,718 | 1,273,040 | 2,546,758 |
| map events | 847,461 | 849,883 | 1,697,344 |

The dominant content is exact evidence rather than reusable shape:

- both map-events schemas devote about 87% of their source text to `const` payloads, including complete
  program, label, operation, control-flow, and vocabulary order arrays;
- both common-stats schemas contain about 4,140 `const` values and copy the same five definitions;
- the common-menus pair copies 13 identical definitions and embeds exact instruction corpora for shop,
  church, Caravan, blacksmith, and shared-selection routines;
- map-script schemas embed command-family corpora as object or array constants, including individual
  constants larger than 100 KiB.

Removing only `const` keys from those eight schemas, without yet sharing any definitions, reduces their
estimated pretty-printed size from 12,497,188 bytes to about 4,540,934 bytes. This is an estimate, not
an acceptance target: real shape schemas must replace appropriate constants with explicit types and
domain constraints.

The exact values are already guarded outside the schema layer:

- common menus and common stats compare generated domain facts with their fixtures and then check the
  full canonical output digest;
- map-script engine compares every fixture-owned field and then checks its full canonical output digest;
- map events requires complete `fixture["expected"] == output` equality and then checks the full canonical
  output digest.

Single-process fixture validation on the audit machine took approximately 0.67 seconds for map-script
engine, 0.14 seconds for common menus, 0.06 seconds for common stats, and 6.83 seconds for map events.
The map-events tracked fixture is itself 19,606,431 bytes, so schema composition alone cannot solve that
contract's review and validation cost.

## Decision

Separate each maintained contract into three explicit layers.

### 1. Structural schema

JSON Schema owns reusable data shape:

- required fields and exact field names;
- `additionalProperties: false` on closed records;
- scalar types, meaningful numeric ranges, and closed protocol enums;
- stable record structure and reusable nested definitions;
- small identity constants such as schema version, contract ID, emulator/core identity, and protocol
  discriminators.

Structural schemas do not own complete extracted corpora, large order arrays, per-source-record exact
values, or full case/handler objects as `const`. A large `const` remains acceptable only when the value
itself is a small protocol atom and not a second copy of fixture evidence.

### 2. Golden evidence

Tracked fixtures own exact expected values, order, and representative or complete corpora. Extraction
manifests continue to own canonical output digests and summary snapshots. A fixture may be complete or
bounded, but its verifier must state which relationship it asserts.

Very large complete fixtures may be split by semantic ownership when independent sections already have
closed reconciliation rules. They are not split by arbitrary line count, map number, or individual case.
The verifier remains responsible for composing or comparing the complete logical contract.

### 3. Domain invariants

Project-owned Python verifiers and mutation tests own relationships that JSON Schema does not express
well, including:

- order-key and record one-to-one correspondence;
- cross-table joins, ownership, uniqueness, and address relationships;
- source/H1/ROM parity and complete source-use reconciliation;
- corpus hashes and summary counters;
- callback call-site/target/return chronology and terminal cleanup state.

Canonical hashes remain a final whole-output drift gate, not a substitute for diagnostic fixture and
invariant checks.

## Local Reference Contract

Cross-file `$ref` is required to share structural components, but the current validator creates a
`Draft7Validator` without a local registry. Relative references must not depend on Windows paths or
network retrieval.

Before splitting any production schema, add a project-owned registry that:

1. preloads every tracked schema `$id` from `schemas/`;
2. resolves only registered repository resources;
3. rejects duplicate IDs, unknown references, and attempted network retrieval;
4. uses stable URI or URN identifiers rather than `file://` paths;
5. provides tests for closed resolution, missing resources, and cyclic-reference behavior.

Output and fixture schemas then reference the same component schemas. A structural schema never
references a golden fixture.

## File Layout

The existing rail-based layout remains in force. New component schemas use flat, prefixed names under
`schemas/core/`, `schemas/h2/`, or `schemas/h3/`; this ADR does not introduce subsystem subdirectories.
For example, a common-stats pilot may own:

```text
schemas/h2/common-stats-source-record.schema.json
schemas/h2/common-stats-getters.schema.json
schemas/h2/common-stats-mutations.schema.json
schemas/h2/common-stats-clamps.schema.json
schemas/h2/common-stats-distance.schema.json
schemas/h2/common-stats-output.schema.json
schemas/h2/common-stats-fixture.schema.json
```

Moving a legacy root pair and changing its semantic composition are separate review concerns. Do not
combine a repository-wide path migration with this content refactor. A bounded pilot may move only its
owned pair when it updates every direct consumer atomically.

## Migration Plan

Use a dedicated `codex/repo-*` worktree and serialize integration with active research and design lanes.
Do not rewrite all 254 files in one branch.

1. **Registry infrastructure.** Add local-only reference resolution and registry QA without changing
   schema meaning. Because this changes the shared validation path, run the complete Python suite,
   normal verification, and the full profile.
2. **Common-stats pilot.** Its five major definitions and 2,546,758-byte pair provide a bounded proof that
   shared components and shape/golden separation preserve all existing negative mutation coverage.
3. **Common menus.** Extract shared-selection, shop, church, Caravan, blacksmith, and common instruction
   shapes; replace the 13 copied definitions with references.
4. **Map events.** Split structural components by routing/setup, entity programs, zone programs, item
   programs, operation vocabulary/payload, direct flags, script invocation, and textbox contracts.
   Shard the 19.6 MiB fixture along the same semantic boundaries while preserving complete equality and
   canonical-digest checks.
5. **Map-script engine.** Migrate last because its 280 definitions and active H3 command-family slices
   make it the highest integration-risk owner. Prefer six to eight cohesive components (core dispatcher
   and program corpus, dialogue/UI, state/block, entity, presentation, and control) rather than one file
   per command family.
6. **H3 observer components.** Extract shared callback failure, pending callback, terminal status, and
   callback-audit shapes. Keep case-specific golden matrices in their fixtures, not in shared schemas.

Each bounded migration reruns its focused H2/H3 command, relevant mutation suite, complete Python suite,
and `uv run sf2 verify`. Run `uv run sf2 verify --full` for the shared registry change and again at the
schema-migration milestone or merge-readiness boundary.

### Implemented stages

- The registry-infrastructure stage preloads tracked `$id` resources and rejects duplicate IDs,
  unknown references, and network retrieval while retaining bounded cyclic references.
- The common-stats pilot moves its output/fixture roots and five shared components under `schemas/h2/`.
  The component layer contains no golden `const` payloads; only the two roots retain their small
  schema-version and contract-ID constants. Exact corpus values stay in
  `tests/fixtures/h2/common-stats-static-v1.json` and are enforced by the owner verifier, including an
  explicit 12-field function-address join to `manifests/research-index.json`. The composition audit
  reports size, constant payloads, local reference closure, and duplicated schema bodies.
- The common-menus stage replaces its 3,303,113-byte mirrored pair with output/fixture roots plus a
  service-state-machine component, five service components, and one common instruction-record
  component under `schemas/h2/`. The nine-schema set is below 200 KiB, contains no component-level
  golden `const`, retains only four root identity constants, and resolves seven tracked resources with
  no duplicate bodies. The unchanged fixture owns exact values and operation order; the production
  verifier rejects schema-valid provenance, H1-address, domain-model, and alternate-source drift before
  writing output.
- The map-events stage replaces its 1,697,344-byte mirrored schema pair with nine semantic section
  schemas, one shared target-program component, and three small output/index/shard roots under
  `schemas/h2/`. The 13-schema set is below 300 KiB, has zero component-level golden `const` or exact
  corpus-cardinality locks, retains only six root identity constants, and resolves ten tracked resources
  with no duplicate bodies. The former 19.6 MiB monolithic fixture is an index plus nine routing/setup,
  program, operation, direct-
  flag, script-invocation, textbox, and sound-command shards. The loader rejects path, identity,
  section-order, field-inventory, duplicate-field, and incomplete-coverage drift before recomposing the
  exact 103-field logical fixture; the unchanged complete-equality and canonical-digest gates still own
  every value and ordering relationship.

## Acceptance and Guardrails

A migrated contract is accepted only when:

- the accepted generated output and fixture still validate;
- every existing missing/renamed/extra-field and semantic mutation test still fails at an equally useful
  boundary;
- exact fixture comparisons, invariant checks, summaries, and canonical digests remain unchanged unless
  separately justified by evidence;
- all `$ref` resources resolve from the tracked local registry with networking disabled;
- output and fixture schemas demonstrably reuse the same structural components;
- private/generated inputs remain outside Git and the tracked/public boundary still passes.

Add a schema-composition audit that reports file size, total and large `const` payloads, unresolved refs,
and duplicated component bodies. Prefer an allowlisted semantic lint over a blunt file-size failure:
large schemas can be legitimate, while a small schema can still duplicate golden evidence.

## Non-Goals

- weakening closed schemas or removing `additionalProperties: false`;
- replacing diagnostic fixture/invariant checks with only a SHA-256 comparison;
- splitting every case, source file, map, or record into its own schema;
- changing accepted reverse-engineering findings, evidence labels, or corpus content;
- performing the frozen legacy-root migration across unrelated contracts in this decision branch.

## Expected Result

The immediate objective is clearer ownership and lower duplicated churn, not an arbitrary byte target.
Based on the audit, reducing the schema tree from about 15.5 MiB to roughly 5–7 MiB is plausible after
the four dominant pairs are migrated, while retaining or improving every existing evidence gate. The
map-events fixture requires its own semantic sharding to materially reduce review and validation cost.
