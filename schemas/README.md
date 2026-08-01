# Schema Layout

The schema tree is organized by evidence rail, not by game subsystem:

- `core/`: repository-wide contracts such as ROM identity, research indexes, and other cross-rail
  metadata;
- `h2/`: a static extractor's output schema beside its tracked H2 fixture schema; and
- `h3/`: a runtime fixture schema beside any corresponding observation schema.

Keep existing filename conventions inside those directories so a path move does not also rename a
contract. Do not add subsystem directories below the rail: many contracts cross battle, map, UI, and
service boundaries, while H2/H3 ownership already determines the verifier and acceptance gate.

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
