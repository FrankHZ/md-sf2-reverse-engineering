# Reverse-Engineering Research Index

- Status: **Confirmed machine-readable index for the current H2/H3 evidence surface**
- Evidence date: 2026-07-18
- ROM: USA retail, SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source baseline: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Purpose and Ownership

[`manifests/research-index.json`](../../manifests/research-index.json) is the canonical navigation
layer between upstream symbols, ROM/RAM addresses, behavioral evidence, research documents, and
implementation-neutral design contracts. It contains metadata only: no ROM bytes, extracted text,
graphics, audio, or other private/generated game content.

The index does not replace its inputs:

- the pinned H1 assembler listing owns symbol entry addresses;
- H2 extraction manifests own static ROM ranges and record layouts;
- H3 fixtures own executable setups and expected observations;
- research documents own interpretation, provenance, and open questions;
- design documents own remake-facing behavior contracts.

An index record connects those owners and labels the current conclusion `confirmed`, `inferred`, or
`unknown`. The index covers the complete current H3 fixture directory plus selected H2 tables when
a concrete subsystem query benefits from the connection. It is not a claim that every upstream
assembly function, game subsystem, or H2 table is already covered.

## Validation Contract

```powershell
uv run sf2 research-index test
uv run sf2 research-index list --summary
uv run sf2 research-index list --query counter
uv run sf2 research-index list --fixture attack-chain
```

`sf2tool.research_index.verify_index` validates the JSON schema and then enforces relationships that
JSON Schema cannot express:

1. the upstream repository and commit equal `manifests/toolchain.json`;
2. every record has one unique ID, one ROM symbol address, and unique local address IDs;
3. every referenced fixture, Python/legacy verifier, research document, and design contract exists;
4. fixture IDs and ROM hashes agree with the index;
5. every indexed binding equals the concrete ROM/RAM address stored in its fixture;
6. every `*Address` field under every current H3 fixture's `function` or `ram` object is bound;
7. when the ignored upstream checkout exists, code/data source files and symbol labels are checked;
8. when the H1 listing exists, each symbol address is compared with the assembled listing, including
   labels that share a line with their first data directive.

The default commit gate `uv run sf2 verify` runs this check before ROM/toolchain provenance; the
milestone gate `uv run sf2 verify --full` continues into extraction and runtime rails. A fresh
checkout can validate tracked relationships without private inputs; source and listing checks
activate automatically when their local evidence exists.

## Adding a Finding

When a new reverse-engineering slice introduces a runtime address or indexed static range:

1. locate the named symbol in the pinned source and H1 listing;
2. add or update the owning H2/H3 fixture and its dedicated verifier;
3. add an index record or evidence binding for each indexed fixture address field;
4. cite the owning research document and any accepted design contract;
5. retain `inferred` or `unknown` until the evidence satisfies the repository vocabulary;
6. run the narrow fixture verifier, `uv run sf2 research-index test`, and the root verification entry
   point.

Observation points inside a function use `kind: observation`. Named function entries use
`kind: symbol`; stable runtime state uses `space: ram` and `kind: state`. An address appearing in
multiple fixtures is deliberately repeated in those fixtures as an executable contract, while the
index verifier proves every copy still agrees.

## Current Boundary

The index currently answers which named H3-covered symbols and selected H2 tables have executable
evidence and where that evidence is documented. Enemy gold and item drops are the first indexed H2
reward tables; other static coverage remains in `manifests/extractions/` until a subsystem benefits
from the connection. Source coverage percentages must use an explicitly defined denominator and
must not treat these indexed records as whole-program coverage.
