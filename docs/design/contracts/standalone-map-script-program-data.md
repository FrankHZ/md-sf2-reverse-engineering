# Standalone Map-Script Program Data Contract

- **Confirmed original structure:** 47 ordered standalone `scripts*.asm` source files, 178 non-empty
  labeled programs, 8,058 ordered operations, their complete private source-reference graph, the
  exact aggregate classifications and counts below, and pinned source/H1/ROM provenance.
- **Inferred original behavior:** source label prefixes and command names suggest organizational
  roles such as cutscene, entity action, subroutine, and palette data. Those names are retained as
  source taxonomy only; they do not establish story, player-facing, or runtime meaning.
- **Unknown original behavior:** normal-story admission and route order; execution frequency; command,
  branch, subroutine, and target effects; story-state persistence; entity, camera, text, sound,
  palette, wait, and rendering behavior; frame timing; malformed-input handling; and replacement or
  localization policy.
- Remake status: implementation-neutral Phase 3 private-import contract; no interpreter API, script
  language, scheduler, scene system, story model, editor format, or public original-content format is
  selected.
- Evidence date: 2026-08-14
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines the static data and provenance boundary of the final 47 non-pointer-table
`scripts*.asm` files under the original `data/maps/entries/*/mapsetups/` source corpus:

1. the ordered identities of all 47 source files and their representative labels/addresses;
2. 178 source-ordered, non-empty labeled programs and their 8,058 ordered operations;
3. complete private label ownership, same-file/cross-file lexical references, and resolved
   operation-target edges;
4. the exact static join between the broader init-script target set and the 12 targets owned by this
   standalone corpus;
5. a private lossless import form and a bounded public metadata/provenance projection.

The sole executable owner consumed here is fixture id `sf2-map-scripts-static-v1` in
[`tests/fixtures/h2/map-scripts-static-v1.json`](../../../tests/fixtures/h2/map-scripts-static-v1.json).
Its verifier is [`src/sf2tool/h2/map_scripts.py`](../../../src/sf2tool/h2/map_scripts.py), and its
source-backed owner is [Map Data Inventory](../../research/map-data-inventory.md), section
**Standalone Setup Scripts**.

The exact future research-index association is only `map.data.cs-5e772`. That record binds the
representative symbol `cs_5E772` at ROM `0x5E772` (`386930`). It is one trace anchor into the
complete 47-file corpus, not a statement that the corpus contains only that program or that this
program has privileged runtime meaning.

The broad `sf2-map-data-static-v1` and `sf2-map-script-engine-static-v1` fixtures are deliberately
excluded. The former owns aggregate map-data membership; the latter owns the complete interpreter-
facing command/program/handler surface used by existing contracts. Neither becomes a second
executable dependency or expands this document's association set.

## Pre-Contract Evidence Audit

The dedicated owner reproduces from the accepted baseline as:

```text
sf2-map-scripts-static-v1
SHA256 7FD49F464181563AC3C1D6FAFBB7671B072A0CC5ADC8F5506BFA74C9C2C78E4E
Files 47 / Labels 178 / Statements 8058 / ExternalReferences 146 / PASS
```

The fixture directly binds exactly one research-index record:

- `map.data.cs-5e772` — unique, currently unassociated, and the sole future association.

No other `map.data.*`, `map.setup.*`, `scripting.map.*`, interpreter, handler, entity, dialogue,
audio, story, transition, palette, presentation, or service record gains this contract. In
particular, source membership in a broader map-data corpus does not make that aggregate fixture an
owner here.

The tracked fixture is a public metadata surface. It contains provenance, 47 representative
addresses, aggregate counts, label-kind counts, a twelve-row command-frequency summary, twelve
standalone init-target identities, and three runtime-question identities. Complete program bodies,
operand text, the full 178-address map, complete command counts, body hashes, and reference-source
lists remain in ignored `local/derived/map-scripts-static.json`.

## Source-File and Program Identity

### Confirmed static

The accepted corpus contains exactly:

| Surface | Accepted count |
| --- | ---: |
| ordered standalone source files | 47 |
| source lines | 8,398 |
| normalized source statements | 8,058 |
| distinct source command identities | 139 |
| global labels | 178 |
| non-empty programs | 178 |
| ordered program operations | 8,058 |
| representative file-label addresses | 47 |

Every source file has one tracked representative symbol and H1 address. Every one of the 178 global
labels resolves in the pinned H1 listing and owns a non-empty ordered program. The equality between
the 8,058 normalized statements and 8,058 program operations is a construction-checked corpus
boundary, not a runtime execution count.

A private importer MUST preserve, for every file, its source path, source order, representative
identity, and all owned labels. For every program it MUST preserve the source label, H1 address,
source path, source classification, and ordered operations. It MUST NOT merge same-shaped programs,
renumber labels, discard apparently redundant operations, or promote the representative symbol into
a distinct semantic class.

The public projection may retain the 47 representative identities/addresses already exposed by the
fixture. It does not publish all 178 labels and addresses or the complete mapping from programs to
source files.

## Source Classification Boundary

### Confirmed static taxonomy

The verifier classifies labels by their accepted source prefix without assigning runtime effects:

| Source classification | Program count |
| --- | ---: |
| cutscene | 141 |
| cutscene subroutine | 6 |
| cutscene entity | 4 |
| entity-action script | 2 |
| ordinary subroutine | 13 |
| local control flow | 8 |
| palette data | 2 |
| other | 2 |
| **Total** | **178** |

These are source-shaped classifications. A private importer retains them because they are useful for
round-trip provenance and diagnostics. A remake MUST NOT infer execution admission, entity ownership,
visible cutscene content, palette application, narrative role, or player-facing behavior from the
classification alone.

The word `cutscene` and the command/label prefixes therefore have two distinct statuses:

- the exact source spelling and prefix-derived classification are **Confirmed static**;
- any behavioral, narrative, or presentation interpretation suggested by that spelling is only
  **Inferred** and is not a fidelity requirement here.

## Ordered Operation and Target Graph

### Confirmed static

All 178 programs retain their complete private ordered operation lists. Each operation preserves:

- its zero-based index within the owning program;
- exact source opcode identity;
- private operand text;
- zero or more resolved target symbols;
- the matching H1 target addresses.

Exactly 100 operations contain a target symbol, and those operations contain exactly 100 target
references. The one-to-one aggregate result does not impose a universal one-target grammar on future
or malformed data; it describes the complete accepted corpus only.

The static graph also retains source termination tokens. The corpus contains 122 `csc_end` statements
and 16 `rts` statements. These counts are source-token facts, not proof that every token executes, that
the two forms are semantically equivalent, or that either one defines a public interpreter API.

The public fixture may retain aggregate operation and target-reference counts. Complete opcodes,
operands, target edges, program bodies, and body hashes remain private. Synthetic tests may exercise
ordering, target resolution, and loss detection without publishing the original programs.

## Lexical Reference Topology

### Confirmed static

Reference analysis searches the complete accepted 720-file map-setup source boundary while assigning
ownership to the 178 standalone labels. Its exact result is:

| Reference surface | Accepted count |
| --- | ---: |
| labels referenced by another source file | 127 |
| labels referenced only within their defining file | 51 |
| labels with no lexical reference | 0 |
| cross-file lexical references | 146 |
| same-file lexical references | 92 |

The 127 and 51 counts partition all 178 labels. They are label-ownership/reference classes, while 146
and 92 count occurrences; neither pair may be substituted for the other.

A private importer MUST retain, for each label, its owner path, same-file occurrence count, external
occurrence count, and ordered external source-path identities. It may construct reverse indexes or a
modern graph representation, but it MUST remain possible to reproduce the accepted ownership and
reference counts exactly.

A lexical reference is not runtime reachability. The zero-unreferenced result does not prove every
program executes during normal play, nor does an external reference establish route order,
conditional admission, persistence, or observable effects.

## Init-Target Ownership Join

### Confirmed static

The dedicated verifier cross-checks the accepted init-script target identities and divides the 75
distinct targets as follows:

| Target ownership | Count |
| --- | ---: |
| targets owned by this standalone corpus | 12 |
| targets owned by non-standalone init sources | 63 |
| **Total distinct init targets** | **75** |

The twelve public identities are:

`cs_53176`, `cs_570B0`, `cs_58FA4`, `cs_5996E`, `cs_5B016`, `cs_5E320`, `cs_5E346`, `cs_6060E`,
`cs_607DE`, `cs_60C42`, `cs_60CA4`, and `cs_60EB2`.

This is a static target-definition join. It does not establish that any target runs in normal play,
that it runs once, that all call sites have the same state, or that its operations have the effects
suggested by their names. Map setup selection and init dispatch remain separate owners.

The verifier uses the accepted map-init fixture internally to close this target join. This design
contract consumes only the resulting `sf2-map-scripts-static-v1` executable fixture and does not
register the map-init fixture as a second dependency.

## Public Command-Frequency Boundary

### Confirmed static

The public fixture retains the twelve most frequent command identities and exact source occurrence
counts:

| Command | Count |
| --- | ---: |
| `nextSingleText` | 923 |
| `setFacing` | 733 |
| `endActions` | 703 |
| `csWait` | 596 |
| `entityActionsWait` | 478 |
| `setActscriptWait` | 388 |
| `nextText` | 297 |
| `moveDown` | 275 |
| `moveUp` | 244 |
| `entityActions` | 225 |
| `setPos` | 211 |
| `moveLeft` | 198 |

These values are corpus histograms, not execution frequencies. The full 139-command histogram is a
private verification surface. No command name establishes handler behavior, timing, visible text,
facing result, entity movement, wait duration, or audio/presentation effect under this contract.

## Implementation-Neutral Import Model

One complete logical import may use the following private/public split. Names are illustrative; the
identities, ordering, and relationships are normative.

```text
StandaloneMapScriptCorpus {
  privateFiles[47]: PrivateScriptSourceFile
  privatePrograms[178]: PrivateMapScriptProgram
  privateReferences[178]: PrivateLabelReferenceRecord
  privateCommandHistogram[139]
  standaloneInitTargetRefs[12]
  publicSummary: StandaloneMapScriptPublicSummary
}

PrivateScriptSourceFile {
  sourceOrder
  sourcePath
  representativeSymbol
  representativeAddress
  privateBodyHash
  ownedProgramRefs[]
}

PrivateMapScriptProgram {
  programId
  sourcePath
  h1Address
  sourceClassification
  orderedOperations[]: PrivateMapScriptOperation
  privateBodyHash
}

PrivateMapScriptOperation {
  operationIndex
  opcodeIdentity
  privateOperandText
  targetSymbols[]
  targetAddresses[]
}

PrivateLabelReferenceRecord {
  labelId
  ownerPath
  sameFileReferenceCount
  externalReferenceCount
  privateExternalSourcePaths[]
}

StandaloneMapScriptPublicSummary {
  fixtureId = "sf2-map-scripts-static-v1"
  sourceFileCount = 47
  sourceLineCount = 8398
  statementCount = 8058
  uniqueCommandCount = 139
  programCount = 178
  operationCount = 8058
  representativeAddresses[47]
  labelKindCounts
  externallyReferencedLabelCount = 127
  internalOnlyLabelCount = 51
  unreferencedLabelCount = 0
  sameFileReferenceCount = 92
  externalReferenceCount = 146
  operationTargetCounts
  initTargetOwnershipCounts
  topCommandHistogram[12]
  runtimeQuestionIds[3]
  fixtureProvenance
}
```

This is a private import/provenance model, not a required script interpreter, bytecode format,
coroutine scheduler, event system, editor schema, scene graph, or save-state representation. A remake
may compile the private programs into another internal form only when the accepted identities,
ordering, target relations, classifications, and source provenance remain independently verifiable.

## Public Projection and Copyright Boundary

A public contract or report may retain only metadata already bounded by the accepted fixture:

- fixture, upstream, ROM, verifier, and research-owner provenance;
- corpus dimensions and aggregate operation/reference counts;
- 47 representative symbols and addresses;
- label-kind counts and the twelve-row command-frequency summary;
- twelve standalone init-target identities;
- the three accepted runtime-question identities;
- explicit Confirmed, Inferred, Unknown, and separate-owner labels.

The public projection MUST NOT contain complete original program bodies, ordered operation streams,
operand text, the full command histogram, all 178 labels/addresses, body hashes, complete target graph,
reference-source lists, extracted dialogue, palette bytes, captures, or other copyrighted payload.
Private import tooling may retain and hash those forms only in ignored local storage.

## Cross-System Separation

- [Map Exploration](map-exploration.md) owns the accepted interpreter-facing command, handler,
  entity, camera, placement, lifecycle, transition, and bounded H3 seams. This data contract does not
  duplicate those runtime semantics or register `sf2-map-script-engine-static-v1`.
- [Map Entry Routing State](map-entry-routing-state.md) retains its SwitchMap, CheckBattle, and
  savepoint helper seam. Accepted map-setup evidence and [Map Exploration](map-exploration.md) retain
  setup selection, init admission, and dispatcher order. The twelve-target join is not a route-
  admission contract.
- [Story Progression](../synthesis/story-progression.md) owns cross-subsystem story/state synthesis;
  source labels and references here do not define plot order or save persistence.
- [Dialogue System](dialogue-system.md), [Party Roster State](party-roster-state.md),
  [Audio System](audio-system.md), and entity/presentation owners retain their respective command
  families, state seams, visible content, sound, and runtime behavior.
- [Map Entity Data](map-entity-data.md), [Map Area Description Routing](map-area-description-routing.md),
  map layout/palette/tileset/sprite owners, and graphics/interrupt contracts retain their own data,
  selection, rendering, and hardware boundaries.
- `sf2-map-data-static-v1`, all other `map.data.*` records, `map.setup.*`, `scripting.map.*`, service,
  handler, and presentation records remain excluded and unchanged.
- Modern script language, editor UX, localization, accessibility, replacement, licensing, and
  distribution choices remain future owners.

## Judgment Boundary

### Confirmed

- exact `sf2-map-scripts-static-v1` fixture identity, pinned source/ROM provenance, canonical digest,
  verifier, and owner prose;
- 47 ordered source files, 8,398 source lines, 139 command identities, 178 labels/programs, and 8,058
  ordered operations;
- exact eight-way source classification counts;
- complete label ownership and lexical-reference topology: 127 external, 51 same-file-only, zero
  unreferenced labels, 146 cross-file references, and 92 same-file references;
- 100 operations with exactly 100 resolved standalone-target references;
- 75 init targets partitioned into 12 standalone-owned and 63 non-standalone-owned identities;
- 122 `csc_end` and 16 `rts` source tokens;
- bounded public metadata separated from complete private program/reference content;
- sole research-index trace anchor `map.data.cs-5e772` at `0x5E772`.

### Inferred

- source labels and command names suggest organizational roles. Their possible narrative,
  presentation, or runtime meaning is not promoted into the contract.

### Unknown

- normal-story admission, route ordering, conditional reachability, execution frequency, and caller
  state for every program;
- command, branch, subroutine, and target effects, including persistence and lifecycle consequences;
- entity orientation/movement, camera, text, audio, waits, custom subroutines, palette handling,
  rendering, and frame timing;
- physical byte spans and runtime costs of individual programs beyond accepted provenance;
- malformed, truncated, unresolved, injected, or replacement program admission and recovery;
- modern interpreter/editor format, localization, accessibility, replacement, licensing, and
  distribution policy.

## H4 Acceptance Contract

A remake-facing H4 adapter passes this contract only when it can:

1. identify fixture `sf2-map-scripts-static-v1`, the pinned baseline, verifier, owner prose, and sole
   research-index trace anchor;
2. privately preserve all 47 ordered source-file identities, representative labels/addresses, and
   source ownership without merging or renumbering files;
3. privately preserve all 178 non-empty programs, their labels/H1 addresses/classifications, and all
   8,058 operations in source order;
4. retain exact opcode identity, private operand text, and resolved target symbols/addresses for the
   100 target-bearing operations;
5. reproduce the complete 178-label reference topology and its 127/51/0 label partition plus 146/92
   occurrence counts;
6. preserve the 75-target ownership join and exact twelve standalone target identities without
   claiming runtime admission;
7. reproduce the eight-way source classification and public twelve-row command histogram without
   assigning behavioral meaning from names;
8. detect missing/reordered files, lost labels, empty programs, operation reorder, unresolved targets,
   reference-graph drift, ownership drift, and accidental public-content disclosure through private
   or synthetic tests;
9. permit an independent compiler/interpreter representation rather than requiring original macros,
   register use, instruction order, handler implementation, scheduling, or save layout;
10. publish only bounded aggregate metadata/provenance and never original program bodies, operands,
    complete graphs, hashes, extracted content, or captures;
11. leave story, persistence, entity/camera/text/audio/palette effects, timing, malformed input,
    replacement, localization, and accessibility to separate owners or **Unknown**.

An H4 implementation may parse source-shaped private inputs at import time or consume a separately
generated private intermediate representation. Either choice conforms only when the accepted
identity/order/reference facts and public non-disclosure boundary remain independently testable.

## Evidence Matrix

| Contract surface | Evidence label | Exact owner | Preserved boundary |
| --- | --- | --- | --- |
| file/program corpus | **Confirmed static** | `sf2-map-scripts-static-v1`; [fixture](../../../tests/fixtures/h2/map-scripts-static-v1.json) | 47 files, 178 private programs, 8,058 operations; no runtime admission claim |
| label taxonomy | **Confirmed source classification** / **Inferred meaning** | same fixture and pinned source | exact prefix-derived counts; no story, entity, palette, or presentation meaning |
| target and reference graph | **Confirmed static** | same fixture; [map-data research](../../research/map-data-inventory.md) | complete private topology and bounded public totals; lexical reference is not reachability |
| init-target ownership join | **Confirmed static** | same fixture/verifier | 75 = 12 standalone + 63 non-standalone; no selection or dispatch behavior |
| command histogram | **Confirmed source counts** | same fixture | public top twelve and private full histogram; not runtime frequency or command effects |
| complete interpreter/runtime behavior | separate owner / **Unknown** | [map exploration](map-exploration.md) and its H2/H3 owners | this contract owns authored program data, not handlers, effects, scheduling, or presentation |
| aggregate map-data membership | excluded executable owner | `sf2-map-data-static-v1` | no aggregate registration or sibling `map.data.*` associations |
| public original content | prohibited | copyright/private-input boundary | aggregate metadata only; complete programs, operands, graphs, hashes, and captures remain private |

## Open Questions

1. Which standalone programs are reached through normal original story routes, in what order, and
   with what caller state?
2. Which command, entity, camera, text, audio, palette, and wait effects require grouped runtime
   observation beyond the existing handler-local seams?
3. What validation, diagnostic, replacement, localization, and editor policy should a remake adopt
   for malformed or intentionally modified private scripts without exposing original content?

## Reproduction

```powershell
uv run sf2 h2 map-scripts
uv run sf2 design-contracts test
uv run sf2 research-index test
```

Generated output remains under ignored `local/derived/map-scripts-static.json`. Public acceptance uses
bounded metadata and provenance, not original program bodies, operand text, complete target/reference
graphs, body hashes, extracted content, or captures.
