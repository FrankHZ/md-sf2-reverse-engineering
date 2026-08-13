# Map Area-Description Routing Contract

- **Confirmed original structure:** the two area-description dispatcher identities, 126 ordered
  setup references to 75 callable targets, 37 fixed wrappers, 38 direct-return stubs, 37 terminated
  private tables, 227 physical entries, 461 setup-expanded references, and the bounded first-match
  consumer rules described below
- **Inferred original behavior:** source symbols and macro vocabulary identify an area-description or
  investigation role, but this contract promotes no player-visible meaning from those labels
- **Unknown original behavior:** nonstandard or deliberately modified `d6=0` reachability, selected
  function effects and persistence, natural story admission, visible text/portrait/window behavior,
  timing, and malformed or injected table handling
- Evidence date: 2026-08-13
- Remake status: implementation-neutral Phase 3 private-import and routing contract; no dialogue,
  renderer, story, save, or map-lifecycle implementation is selected

## Contract Boundary

This contract defines the static route from the selected map setup's area-description slot through
one callable target and, for a nonempty target, an ordered six-byte record scan. It owns:

1. the `RunMapSetupAreaDescription` and `DisplayAreaDescription` identities and H1-bound addresses;
2. the ordered 126-reference to 75-target topology;
3. the distinction between 37 fixed 16-byte wrappers and 38 two-byte direct-return stubs;
4. the private 37-table, 227-entry storage corpus and its 461 setup-expanded references;
5. the six-byte entry, two-byte terminator, packed-coordinate, condition, payload-kind, and
   first-match rules;
6. the text-index and function-target arithmetic accepted by the dedicated owner;
7. the sole assembled normal-exploration call-path boundary for source value `d6=1`; and
8. a public metadata/provenance projection that excludes original table content and the full
   reference graph.

It does not own setup selection, entity/zone/item dispatch, exploration-loop admission, text
content, dialogue presentation, portrait/window behavior, selected function effects, story meaning,
persistence, localization, accessibility, timing, or malformed-input recovery.

The exact future research-index associations are only:

- `map.setup.area-description`;
- `map.setup.display-area-description`.

Every other record remains unchanged. In particular, `map.setup.selector`, `map.setup.entity-list`,
all entity/zone/item dispatcher records, every `map.data.*` record, and all text, dialogue, portrait,
window, story, and presentation records gain no association from this contract.

## Evidence Owner and Pre-Contract Audit

The sole executable owner consumed here is `sf2-map-descriptions-static-v1`:

- [tracked fixture](../../../tests/fixtures/h2/map-descriptions-static-v1.json);
- [maintained verifier](../../../src/sf2tool/h2/map_descriptions.py);
- [owning research prose](../../research/map-data-inventory.md).

Fresh reproduction on the accepted base produced:

```text
Contract          : sf2-map-descriptions-static-v1
SHA256            : BA7010853112B995E3DF4A8D8A207CAC4EA4F355C7E73845E7F677DEA5C4A5F7
SourceFiles       : 75
Wrappers          : 37
DirectReturnStubs : 38
PhysicalEntries   : 227
SetupReferences   : 461
Status            : PASS
```

The fixture binds exactly two research records:

| Research record | Source identity | ROM address | Current contract state |
| --- | --- | ---: | --- |
| `map.setup.area-description` | `RunMapSetupAreaDescription` | `0x47702` / 292,610 | unassociated; future association here |
| `map.setup.display-area-description` | `DisplayAreaDescription` | `0x47722` / 292,642 | unassociated; future association here |

No other research record carries this fixture. The broad `sf2-map-data-static-v1` owner and all
`map.data.*` table records are explicitly excluded. They supply neither executable evidence nor a
semantic association to this contract.

The generated detailed output contains private source rows, table entries, text indices, function
targets, hashes, and the complete setup-reference graph under ignored `local/derived/`. The tracked
fixture exposes aggregate counts and rules plus only the three bounded conditioned-function metadata
rows already accepted for public verification.

## Callable-Target Topology

**Confirmed static:** 126 ordered setup slots refer to 75 unique callable targets. Thirty-five of
those targets are selected by more than one setup slot. Reference identity and callable-target
identity are therefore different domains: an importer must not duplicate a shared target once per
incoming reference or collapse the ordered setup references into an unordered target set.

The 75 callable targets partition exactly:

| Callable target kind | Unique targets | Source shape |
| --- | ---: | --- |
| wrapper | 37 | fixed 16-byte wrapper followed by one private table |
| direct-return stub | 38 | exact two-byte `rts` body |
| **total** | **75** | complete source-file boundary |

Each wrapper preserves these source-static identities in order:

1. load a per-wrapper description-text base into `d3`;
2. load its private table address into `a0`;
3. retain the accepted `nop` position;
4. transfer directly to `DisplayAreaDescription`.

The wrapper's table begins immediately after its 16-byte body. The `nop` is an accepted source/ROM
identity, not a required optimization barrier or timing event in a remake. A direct-return stub is a
distinct callable target with no table; it must not be represented as an invented empty table unless
the original stub identity remains independently recoverable.

The 126-reference denominator is a setup-pointer corpus fact. It does not prove that all referencing
setups, wrappers, or stubs are naturally reachable in one original playthrough.

## Private Table Corpus

**Confirmed static:** the 37 wrappers own 37 private `$FD00`-terminated tables. Their physical
storage contains 227 six-byte entries and 37 two-byte terminators, totaling 1,436 bytes. The largest
table has 23 entries.

The source macro and physical-entry counts are identical:

| Entry form | Physical entries | Setup-expanded references |
| --- | ---: | ---: |
| text (`msDesc`) | 206 | 426 |
| function (`msDescFunction`) | 18 | 31 |
| conditioned function (`msDescFunctionD6`) | 3 | 4 |
| **total** | **227** | **461** |

There are exactly 37 `msDescEnd` terminators. Physical counts describe bytes stored once; expanded
counts describe the same entries observed through all 126 setup references. These denominators must
not be added together or treated as two independent content corpora.

The private importer retains, for every wrapper and table:

- source path, wrapper and table symbols, H1 addresses, and description-text base;
- ordered six-byte records and the physical address of every record;
- the exact terminator identity and address;
- complete incoming setup-reference identity and order;
- text offsets and derived indices, or function relative offsets and resolved targets; and
- original private bytes and hashes needed for source/H1/ROM parity.

These fields are private preservation data. Public artifacts must not reveal the complete map-to-table
assignment graph, complete text-index set, complete function-target set, original table bytes, or
private hashes.

## Six-Byte Record and First-Match Scan

**Confirmed static:** each ordinary record has this bounded logical shape:

| Byte range | Accepted role | Boundary |
| --- | --- | --- |
| `0..1` | X/Y bytes compared together as one packed coordinate word | no tile, pixel, or collision meaning inferred |
| `2` | condition byte | nonzero admission is bounded by `d6`; story meaning unknown |
| `3` | payload-kind byte | accepted values distinguish text and function routes only |
| `4..5` | two text offsets or one signed function-relative offset | private payload values remain undisclosed |

The consumer packs its incoming X/Y values into the same word shape, initializes the scan offset,
and examines records in source order. At each position it:

1. recognizes a first byte of `$FD` as the two-byte terminator and reports no match;
2. compares the packed coordinate word;
3. when byte 2 is nonzero, rejects the row if `d6` is nonzero;
4. dispatches the admitted row according to byte 3; or
5. advances by six bytes and continues.

The first admitted coordinate match is authoritative. A remake may build an index, but it must retain
source order and reproduce first-match selection. It may not normalize duplicates into an unordered
coordinate dictionary.

The accepted corpus contains no other payload kind. That closed corpus fact does not define recovery
for modified values, missing terminators, truncated records, out-of-range pointers, or injected state;
those cases remain **Unknown**.

## Text and Function Payload Routes

### Text route

**Confirmed static:** for payload kind zero, the accepted physical corpus also has condition byte
zero. Byte 4 is added to investigation-text base 423. Byte 5 is added to the selected wrapper's `d3`
base. The resulting two indices are handed to the existing text-display seam in source order.

This contract preserves index arithmetic and handoff order, not the referenced strings. It does not
own text decoding, localization, window layout, portrait choice, font selection, clearing cadence,
or what the player sees.

### Function route

**Confirmed static:** payload kind one interprets bytes 4..5 as a signed relative offset from the
current table base. The resolved address is called after the matching and condition gates. The
contract preserves table-relative resolution, target identity, and call handoff; it does not assign
meaning, side effects, persistence, or a return-value contract to the selected function.

The source-shaped cleanup handoff after either selected route is not evidence of visible completion.
Dialogue/window services, final rendered state, and caller-visible timing remain separate owners or
**Unknown**.

## Conditioned-Function Boundary

The tracked public fixture retains exactly three physical conditioned-function rows:

| Wrapper | Table | Address | X | Y | Condition byte | Payload kind | Relative offset | Resolved target |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| `ms_map31_AreaDescriptions` | `byte_5D584` | 382,376 | 8 | 3 | 1 | 1 | 58 | 382,398 |
| `ms_map41_AreaDescriptions` | `byte_5F41E` | 390,174 | 6 | 7 | 255 | 1 | 14 | 390,188 |
| `ms_map42_AreaDescriptions` | `byte_5FE34` | 392,756 | 39 | 2 | 255 | 1 | 52 | 392,808 |

**Confirmed static call-graph boundary:** there is exactly one assembled call to
`j_RunMapSetupAreaDescription` in the accepted `code/**` source corpus. The normal exploration path
sets `d6=1` before entering `CheckArea`; `CheckArea` reaches that sole call. Because the consumer skips
a nonzero condition byte when `d6` is nonzero, all three physical conditioned-function rows—and all
four setup-expanded references to them—are skipped on that one source-shaped path.

This is not universal unreachability. A direct caller, modified source, injected register state, or
other nonstandard route with `d6=0` could pass the accepted gate, but no such original runtime route
is established here. Whether those targets execute, what they do, and whether their effects persist
remain **Unknown**.

The source comment attached to `d6=1` is retained only as the source label `no-entity-event`. It does
not prove input-edge simultaneity, player intent, exact frame timing, or a general semantic type for
`d6`.

## Implementation-Neutral Import Model

The minimum complete private import keeps setup references, callable targets, tables, entries, and
the public projection distinct:

```text
MapAreaDescriptionCorpus {
  setupReferences[126]: PrivateDescriptionSetupReference
  callableTargets[75]: PrivateDescriptionTarget
  privateTables[37]: PrivateDescriptionTable
  publicSummary: MapAreaDescriptionPublicSummary
}

PrivateDescriptionSetupReference {
  orderedSetupIdentity
  callableTargetRef
}

PrivateDescriptionTarget =
  WrapperTarget {
    sourcePath
    wrapperSymbol
    wrapperAddress
    descriptionTextBase
    tableRef
    privateWrapperBytes[16]
  }
  | DirectReturnStubTarget {
    sourcePath
    symbol
    address
    privateStubBytes[2]
  }

PrivateDescriptionTable {
  tableSymbol
  tableAddress
  orderedEntries[]: PrivateDescriptionEntry
  terminatorAddress
  privateTerminatorBytes[2]
}

PrivateDescriptionEntry {
  physicalAddress
  x
  y
  conditionByte
  payload: PrivateTextPayload | PrivateFunctionPayload
  privateRawBytes[6]
}

PrivateTextPayload {
  investigationTextOffset
  investigationTextIndex
  descriptionTextOffset
  descriptionTextIndex
}

PrivateFunctionPayload {
  relativeOffset
  resolvedTargetAddress
}

MapAreaDescriptionPublicSummary {
  sourceFileCount = 75
  setupPointerReferenceCount = 126
  uniqueTargetCount = 75
  aliasedTargetCount = 35
  wrapperCount = 37
  directReturnStubCount = 38
  physicalEntryCount = 227
  setupEntryReferenceCount = 461
  physicalKindCounts = { text: 206, function: 18, conditionedFunction: 3 }
  expandedKindCounts = { text: 426, function: 31, conditionedFunction: 4 }
  terminatorCount = 37
  physicalTableByteCount = 1436
  maximumTableEntryCount = 23
  consumerRules
  conditionedFunctionMetadata[3]
  fixtureProvenance
}
```

This is an import and routing model, not an engine dialogue API, event scripting language, map editor
schema, or persistence format. Public reports may retain the bounded summary, function identities,
addresses, consumer rules, provenance, and the three tracked conditioned rows. They must omit all
`private*` fields, complete table contents, complete assignments, full text/function index graphs,
original strings, and rendered captures.

## Cross-System Separation

- [Map Exploration](map-exploration.md) owns `CheckArea`, the exploration interaction loop, and its
  accepted runtime lifecycle. This contract owns only the called description selector/scan.
- [Map Entry Routing State](map-entry-routing-state.md), setup-selection evidence, and
  [Story Progression](../synthesis/story-progression.md) retain setup and story-state selection. A
  setup reference is not proof of natural story admission.
- Entity, zone, and item selectors retain their own table formats, match rules, effects, and
  associations. They gain no contract here.
- [Text and Font System](text-and-font-system.md), [Dialogue System](dialogue-system.md), portrait,
  window, and UI owners retain text resources, decoding, presentation, and timing.
- The excluded `sf2-map-data-static-v1` aggregate and every `map.data.*` record remain unchanged.
- Map layouts, palettes, tilesets, sprites, collision, pathfinding, camera, VInt/DMA, audio, and
  rendering remain with their dedicated contracts or **Unknown**.
- Save/load persistence, story meaning, localization, accessibility, content replacement, and
  product policy are deliberate later design surfaces.

The [Map Design Principles synthesis](../synthesis/map-design-principles.md) already summarizes the
accepted aggregate area-description evidence. It may later link this contract, but this owned-doc
slice does not edit or reinterpret that synthesis.

## Judgment Boundary

### Confirmed

- sole executable ownership by `sf2-map-descriptions-static-v1`;
- the two dispatcher identities and addresses;
- 126 ordered setup references, 75 targets, and 35 reused targets;
- 37 16-byte wrappers, 38 two-byte direct-return stubs, and exact wrapper handoff shape;
- 37 private tables, 227 physical entries, 461 expanded references, 37 terminators, 1,436 bytes,
  maximum 23 entries, and exact kind counts;
- packed-coordinate, terminator, condition, payload-kind, table-relative function, text-index, and
  first-match rules;
- the three conditioned-function metadata rows;
- the sole assembled normal-exploration call path's `d6=1` value and its skip result for those rows;
- source/H1/ROM parity and public-metadata/private-content separation.

### Inferred

- source symbols and macros suggest area-description and investigation roles. No player-facing
  meaning, narrative intent, or visible result is promoted from those names.

### Unknown

- direct, debug, modified, or injected `d6=0` caller reachability;
- selected function behavior, side effects, return meaning, transition lifetime, and persistence;
- natural setup and story reachability of individual references and rows;
- text contents, portrait/window selection, localization, presentation, frames, and timing;
- malformed, truncated, unterminated, out-of-range, or replacement table behavior;
- engine editor policy, accessibility, replacement content, and other product decisions.

## H4 Acceptance Contract

A remake-facing H4 adapter passes this contract only when it can:

1. identify `sf2-map-descriptions-static-v1`, the pinned provenance, and the two accepted function
   addresses without adding another executable owner;
2. privately preserve all 126 ordered setup references and 75 callable-target identities, including
   all 35 reused-target relationships;
3. preserve the 37 wrapper identities and exact 16-byte source shape separately from the 38 exact
   two-byte direct-return stubs;
4. privately preserve 37 ordered tables, 227 physical entries, 37 terminators, their addresses and
   bytes, and reproduce the 1,436-byte and maximum-23-entry totals;
5. separately reproduce the 461 setup-expanded references and the exact `206/18/3` physical versus
   `426/31/4` expanded kind counts;
6. reproduce packed-coordinate first-match selection, the `$FD00` terminator, condition-byte gate,
   payload-kind branch, text-index arithmetic, and table-relative function resolution;
7. preserve the exact three conditioned rows and verify that the sole assembled normal path's
   `d6=1` skips them without claiming universal runtime unreachability;
8. detect reordered references, flattened aliases, wrapper/stub conflation, lost terminators,
   changed entry order, drifted offsets, and private-source loss through private or synthetic tests;
9. expose publicly only bounded counts, rules, identities, addresses, provenance, and the accepted
   three conditioned rows—not full tables, assignments, text/function graphs, bytes, hashes, strings,
   or captures;
10. associate exactly `map.setup.area-description` and `map.setup.display-area-description`, leaving
    selector/entity/zone/item/map-data and every other record unchanged; and
11. report caller admission, effects, persistence, text/dialogue/window presentation, timing,
    malformed input, localization, accessibility, and story meaning through separate owners or as
    **Unknown**.

H4 may compile the private tables into indexed structures or another engine-native representation.
That representation conforms only when the ordered source/reference topology and round-trip identity
remain verifiable.

## Evidence Matrix

| Contract surface | Evidence label | Exact owner | Preserved boundary |
| --- | --- | --- | --- |
| dispatcher identities and addresses | **Confirmed static** | `sf2-map-descriptions-static-v1` | exactly two future research-index associations |
| 126 references / 75 callable targets | **Confirmed static** | same fixture and [map-data research](../../research/map-data-inventory.md) | complete graph stays private; no natural reachability claim |
| wrappers, stubs, tables, entries, and terminators | **Confirmed static** | same fixture/verifier | raw bytes, full assignments, indices, targets, and hashes stay private |
| coordinate/condition/payload/first-match rules | **Confirmed static source/ROM** | same owner | no malformed-input or caller-visible recovery contract |
| sole assembled `d6=1` call path | **Confirmed static call graph** | same owner | direct/mutated `d6=0` reachability remains **Unknown** |
| exploration admission and `CheckArea` lifecycle | **Separate owner** | [map-exploration](map-exploration.md) | no input-edge, frame, or interaction result claim here |
| setup/story selection | **Separate owner** | map setup evidence and [story progression](../synthesis/story-progression.md) | setup membership is not story reachability |
| text/dialogue/portrait/window result | **Unknown / separate owners** | text, dialogue, portrait, window, and presentation contracts | static handoffs do not prove visible content or timing |
| broad map-data corpus | **Excluded executable owner** | `sf2-map-data-static-v1` | all `map.data.*` records remain unchanged |
| area-description purpose | **Inferred from source taxonomy only** | source symbols/macros | player-facing and narrative meaning remain unclaimed |

## Open Questions

1. Can a bounded original caller or controlled probe reach a conditioned function with `d6=0`?
2. What state changes do the selected relative-function targets perform, and do any persist across
   transitions or save/load?
3. Which setup references and table rows are naturally reachable in original story play?
4. What text, portrait, window, and timing sequence is visible after each accepted route?
5. What explicit policy should a remake use for malformed or replacement tables?

## Reproduction

```powershell
uv run sf2 h2 map-descriptions
uv run sf2 design-contracts test
uv run sf2 verify
```

Detailed generated output remains under ignored `local/derived/map-descriptions-static.json`. Public
acceptance uses the tracked fixture, aggregate metadata, the three bounded conditioned rows, and
synthetic/private importer tests rather than redistributing original tables, text, or rendered data.
