# Reverse-Engineering Research Index

- Status: **Confirmed machine-readable index for the current H2/H3 evidence surface**
- Evidence date: 2026-07-30
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
7. when the ignored upstream checkout exists, code/data/layout source files and symbol labels are
   checked; `layout/` is accepted only when the section layout itself defines a symbol beside a
   direct payload rather than merely including an owning `code/` or `data/` file;
8. when the H1 listing exists, each symbol address is compared with the assembled listing, including
   labels that share a line with their first data directive.

The H1 listing is parsed once into a symbol-address map for the entire verification run. Duplicate
spellings at the same address are accepted, while the same symbol at conflicting addresses fails the
gate. This keeps the 1,000+ record index fast without weakening source-file ownership or address
parity checks.

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
`kind: symbol`; stable runtime state uses `space: ram` and `kind: state`. `listingDomain` defaults to
`h1-68000`. The only alternate domain is `z80-music-bank`; it is restricted to song sources, the
sound-data fixture/verifier, and the two canonical ROM bank ranges. An address appearing in
multiple fixtures is deliberately repeated in those fixtures as an executable contract, while the
index verifier proves every copy still agrees.

## Current Boundary

As of 2026-07-31, the index contains 1,590 confirmed findings and 2,330 checked address bindings. It
connects all 74 H3 fixture files plus 74 H2 fixtures needed by the completed code and data
inventories. This produces 381/387 code-file reach and 1,017/1,690 domain-aware data-file reach.
The verifier reports the provenance split independently: 1,553 H1 records and 37 Z80 music-bank
records.

Every indexed record still requires a named symbol in the claimed source file and an executable
fixture binding. Normal records additionally require the matching 68000 H1 address. The 37 song
records instead prove their Z80 pointer-table entry, bank-relative range, ROM physical offset, and
bank/ROM bytes; the two unlabeled bank entry files and two macro/enum files remain excluded. The
counter still does not credit 662 map bodies reachable only through an include site, the unlabeled
map-storage container, or unassembled alternates. Their H2 rails prove that all 1,690 data ASM files
belong to a known graph, but indexed reach is not line, semantic, or remake completion coverage.

Source coverage percentages must always name their denominator and evidence level. The current
snapshot, explicit exceptions, and static-first batching policy are recorded in
[`source-coverage.md`](./source-coverage.md).
