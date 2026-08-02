# Schema Layout

The schema tree is organized by evidence rail, not by game subsystem:

- `core/`: repository-wide contracts such as ROM identity, research indexes, and other cross-rail
  metadata;
- `h2/`: a static extractor's output schema beside its tracked H2 fixture schema; and
- `h3/`: a runtime fixture schema beside any corresponding observation schema.

Keep existing filename conventions inside those directories so a path move does not also rename a
contract. Do not add subsystem directories below the rail: many contracts cross battle, map, UI, and
service boundaries, while H2/H3 ownership already determines the verifier and acceptance gate.

## Contract Composition

Schemas own reusable shape: required fields, closed records, types, meaningful ranges, protocol enums,
and small identity constants. Tracked fixtures own exact golden values and corpus order; manifests own
canonical output digests; Python verifiers and mutation tests own cross-field, ordering, provenance, and
source/ROM reconciliation rules.

Do not copy complete corpora or large order arrays into output and fixture schemas as synchronized
`const` values. Output and fixture schemas should reuse the same structural components through tracked,
local-only `$ref` resources once the repository registry is available. Structural schemas must never
reference golden fixtures, and schema validation must never retrieve a reference from the network.

The measured problem, target architecture, staged migration order, and acceptance guardrails are
documented in [ADR 0007](../docs/decisions/0007-schema-contract-composition-and-migration.md). Existing
contracts are not weakened merely because they predate that decision; migrate one complete owner at a
time with equivalent negative mutation coverage.

## Legacy Flat Files

The 2026-08-01 review found 248 flat `*.schema.json` files totaling about 15.4 MiB. At least 141 tracked
source, test, manifest, and documentation files contain direct schema paths. Moving the complete set while
the design lane is active would be a broad integration-hotspot change with little semantic value.

The root-level JSON files are therefore a frozen legacy layout. New schemas go under `core/`, `h2/`, or
`h3/`; the native harness prevents the root-level count from growing above the reviewed baseline. Do not
create an empty directory—its first owning slice creates it together with a concrete schema and consumer.

Migrate the legacy set only in a dedicated `codex/repo-*` branch after active design branches are merged.
That migration must be mechanical and atomic: move complete output/fixture or fixture/observation pairs,
update all code/test/manifest/document references, run the complete Python suite because shared harness
paths change, then run `uv run sf2 verify`. It must not rewrite schema content or combine the move with a
research finding.

The mechanical legacy-path migration and ADR 0007's semantic composition refactor are separate review
concerns. Do not combine a repository-wide path move with golden/shape separation. A bounded semantic
pilot may move only its owned pair when every direct consumer is updated atomically.
