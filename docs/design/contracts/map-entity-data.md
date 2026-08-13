# Map Entity Data Contract

- **Confirmed original structure:** 126 setup-pointer references to 125 unique entity-list roots, one
  duplicated root, 980 physical eight-byte records producing 987 ordered list-record references, the
  exact suffix-sharing and terminator topology, the accepted record-kind and source-macro counts, the
  bounded initial map-sprite domain, and the consumer decoding rules described below.
- **Inferred original behavior:** only the placement and movement intent suggested by source macro and
  field names; no gameplay, presentation, or runtime lifecycle behavior is promoted from those names.
- **Unknown original behavior:** natural story selection and reachability, runtime list admission and
  reload persistence, malformed or injected stream handling, sequenced-orientation consumption,
  follower/declaration collision state, walking-special-sprite presentation timing, entity capacity
  beyond the accepted observations, collision, pathfinding, action effects, rendering, VDP timing,
  dialogue, AI, and balance.
- Remake status: implementation-neutral Phase 3 private-import contract; no runtime entity model,
  map editor format, movement system, renderer, replacement data set, or distribution license has
  been selected.
- Evidence date: 2026-08-12
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines the static storage, reference, encoding, and private-import boundary for the
original map entity-list corpus. It owns five separable surfaces:

1. setup-pointer references and unique entity-list-root identities;
2. physical record storage versus list-traversal references;
3. shared suffixes, fallthrough fragments, and terminator identities;
4. the fixed, walking, and sequenced record-kind inventory;
5. the bounded initial map-sprite values and `InitializeMapEntities` consumer rules.

The executable owner is fixture id `sf2-map-entities-static-v1` in
[`tests/fixtures/h2/map-entities-static-v1.json`](../../../tests/fixtures/h2/map-entities-static-v1.json).
The research owner is
[Map Data Inventory and Setup/Event Surfaces](../../research/map-data-inventory.md). This contract
consumes the fixture's complete `expected` object and the bounded `InitializeMapEntities` provenance
at ROM address `278732` (`0x440CC`).

The exact future research-index association is only `scripting.map.mapfunctions`. The function
identity and address bind the static consumer to its source owner; they do not make this contract the
owner of map setup selection, runtime entity population, entity actions, movement, or presentation.

## Pre-Contract Evidence Audit

The dedicated owner was reproduced from the accepted baseline:

```text
sf2-map-entities-static-v1
SHA256 BFC583155F1D6EE490877A0B0CA2CBBE13DF145A99EE25CA72228A4EA4A2CA4A
SourceFiles 125 / EntityLists 125 / PhysicalRecords 980 / ListReferences 987 /
FallthroughFragments 9 / PASS
```

The H2 fixture directly binds exactly one research-index record:

- `scripting.map.mapfunctions` — unique, currently unassociated, and the sole future association for
  this contract.

The adjacent `sf2-entity-population-reload-runtime-v1` fixture is not consumed here. Its exact
index-linked denominator is ten records:

- the same candidate `scripting.map.mapfunctions`;
- eight records whose existing [map-exploration](map-exploration.md) associations remain unchanged:
  `scripting.map.mapsetupsfunctions-1`, `map.entity-population.get-entity-address`,
  `map.entity-population.initialize-new-entity`, `map.entity-population.load-entity-mapsprites`,
  `map.entity-population.load-from-map-setup`, `map.entity-population.load-map-entities`,
  `map.entity-population.new-entity`, and `map.entity-population.reload-entities`;
- the unassociated `map.setup.entity-list`, which remains a separate setup-selection boundary and
  does not gain this contract.

This is an exact `1 + 8 + 1` partition. The H3 membership does not enlarge this contract's evidence
or association set. In particular, the seven `map.entity-population.*` records are only part of the
eight-record unchanged map-exploration group; they are not the complete runtime denominator.

The tracked fixture contains aggregate counts, source identities, fallthrough relationships,
addresses, bounded map-sprite statistics, and decoding rules. Complete numeric entity rows, private
source-derived output, payload hashes, rendered captures, and emulator state remain private/generated.

## Pointer and List-Root Topology

**Confirmed static:** the setup corpus contains 126 entity-pointer references resolving to 125 unique
entity-list roots. The sole duplicated target is `ms_map21_Entities`. An importer MUST preserve the
126 ordered references and the shared target identity instead of expanding them into 126 anonymous
lists or reducing them to an unordered set of 125 symbols.

The matching source boundary contains 125 `s1_entities*.asm` files. Source-file count, pointer count,
and unique-list count are distinct denominators:

| Surface | Accepted count | Required distinction |
| --- | ---: | --- |
| source files | 125 | physical source ownership, not setup-reference cardinality |
| setup-pointer references | 126 | ordered references, including the duplicated target |
| unique entity-list roots | 125 | decoded root identities, not independent terminators |
| unique terminator addresses | 116 | shared by fallthrough topology; not one per root |

There are 30 empty selected lists and the largest selected list contains 31 record references. These
are corpus observations, not a runtime capacity, engine allocation limit, general map cardinality,
or proof that every list is naturally selected.

## Physical Storage and Reference Counts

**Confirmed static:** the source owns 980 physical eight-byte records. Traversing every unique list
root through its accepted terminator yields 987 ordered record references. The seven-reference delta
comes from intentional suffix sharing; it is not seven extra stored records.

| Record kind | Physical records | Per-list references | Shared-reference delta |
| --- | ---: | ---: | ---: |
| fixed | 803 | 808 | 5 |
| walking | 174 | 175 | 1 |
| sequenced | 3 | 4 | 1 |
| **Total** | **980** | **987** | **7** |

The corresponding source-macro inventory is:

| Source macro | Uses | Storage classification |
| --- | ---: | --- |
| `entity` | 9 | fixed |
| `entityRandomWalk` | 5 | walking |
| `msFixedEntity` | 794 | fixed |
| `msWalkingEntity` | 169 | walking |
| `msSequencedEntity` | 3 | sequenced |

The macro names and classifications preserve source identity. They do not establish visible movement,
randomness distribution, pathfinding policy, animation timing, action results, or AI behavior.

A private importer MUST retain physical record identity separately from list references. Copying a
shared suffix into each logical list may be convenient internally, but original-fidelity diagnostics
must still report that the copied references originate from the same physical records.

## Fallthrough and Terminator Topology

**Confirmed static:** nine source fragments omit a local `msEntitiesEnd`. Eight fall into an adjacent
terminator-only fragment; one contributes a five-record prefix and then shares a seven-record suffix.

| Prefix symbol | Prefix records | Fallthrough symbol | Shared suffix records |
| --- | ---: | --- | ---: |
| `ms_map17_Entities` | 5 | `ms_map17_flag505_Entities` | 7 |
| `ms_map20_flag609_Entities` | 8 | `ms_map20_flag506_Entities` | 0 |
| `ms_map21_flag609_Entities` | 1 | `ms_map21_flag506_Entities` | 0 |
| `ms_map27_Entities` | 3 | `ms_map27_flag523_Entities` | 0 |
| `ms_map34_Entities` | 21 | `ms_map34_flag784_Entities` | 0 |
| `ms_map40_flag506_Entities` | 3 | `ms_map40_Entities` | 0 |
| `ms_map43_Entities` | 3 | `ms_map43_flag612_Entities` | 0 |
| `ms_map61_Entities` | 1 | `ms_map61_flag729_Entities` | 0 |
| `ms_map63_Entities` | 1 | `ms_map63_flag29_Entities` | 0 |

The fixture retains the exact source path, fallthrough address, and both symbols for each row. Those
provenance fields remain part of private import verification even though this public contract does
not reproduce every address.

The first byte value `255` terminates consumer traversal. It is a record-stream boundary, not an
eighth data byte, a map-sprite value in that position, or a general malformed-stream recovery policy.
A per-file parser that invents nine local terminators would erase the original storage graph; one
that stops at each file boundary would lose the record-bearing map 17 suffix relationship.

## Record Encoding and Consumer Boundary

**Confirmed static:** every physical entity record occupies eight bytes. The accepted field order is:

1. X coordinate;
2. Y coordinate;
3. facing;
4. map-sprite identity;
5. action or walking payload.

`InitializeMapEntities` consumes records in stream order, masks each coordinate with `63`, scales the
masked values by the symbolic map-tile size, and routes special map sprites through the special-entity
declaration path. These are consumer decoding facts only.

The contract does not assign a modern coordinate unit, tile dimension, world-space scale, collision
meaning, spawn lifetime, or screen position to the masked/scaled values. It also does not define the
complete payload sublayout for every macro family, action execution, walking state, sequenced stream
consumption, slot allocation, or caller-visible failure behavior.

Stream order MUST be retained as source identity. A remake may compile records into typed private
objects, but compatibility import cannot reorder equal-looking rows, deduplicate them by value, or
drop bytes whose gameplay meaning is not yet named.

## Initial Map-Sprite Boundary

**Confirmed static:** the 980 physical records contain 113 distinct map-sprite byte values over the
observed range `1..255`. This is a sparse observed set, not proof of a continuous or closed domain.

- 977 physical records use regular identities below 240;
- three physical records use routed special identities `251`, `252`, and `255`, once each;
- the high regular identities have counts `230:7`, `231:2`, `232:1`, `233:26`, `234:11`, `235:5`,
  and `236:29`;
- shared-sentinel regular identities `237..239` are absent;
- unbacked special identities `240..250` are absent.

The per-list reference histogram matches the physical histogram for these high values. The three
special identities retain only their accepted declaration-route classification here. Special-sprite
payloads, pointer/dispatch backing, renderer behavior, visible timing, and presentation remain with
[graphics-service-state](graphics-service-state.md) or **Unknown**.

This domain closes only the initial entity-list source records. Later cutscene, entity-action,
combatant-derived, debug, malformed, and direct-RAM assignment surfaces remain separate. Absence from
this corpus does not prove an identity is globally unreachable or unsupported by every original
caller.

## Implementation-Neutral Import Model

The minimum complete logical import keeps storage, references, traversal, and public metadata
separate:

```text
MapEntityCorpus {
  setupPointerReferences[126]: EntityListPointerReference
  listRoots[125]: EntityListRoot
  physicalRecords[980]: PrivateEntityRecord
  terminators[116]: TerminatorIdentity
  publicSummary: MapEntityPublicSummary
}

EntityListPointerReference {
  setupIdentity
  orderedSlotIdentity
  listRootRef
}

EntityListRoot {
  sourcePath
  sourceSymbol
  privatePrefixRecordRefs[]
  optionalFallthroughRootRef
  terminatorRef
}

PrivateEntityRecord {
  physicalAddress
  sourceMacroIdentity
  kind: FIXED | WALKING | SEQUENCED
  privateRawBytes[8]
  privateDecodedFields
}

TerminatorIdentity {
  physicalAddress
  firstByte = 255
}

MapEntityPublicSummary {
  sourceFileCount = 125
  pointerReferenceCount = 126
  uniqueListRootCount = 125
  physicalRecordCount = 980
  listRecordReferenceCount = 987
  sharedSuffixReferenceCount = 7
  uniqueTerminatorCount = 116
  fallthroughFragmentCount = 9
  emptyListCount = 30
  maximumListRecordCount = 31
  physicalKindCounts = { fixed: 803, walking: 174, sequenced: 3 }
  referenceKindCounts = { fixed: 808, walking: 175, sequenced: 4 }
  sourceMacroCounts
  mapSpriteSummary
  consumerRules
  fixtureProvenance
}
```

This model is an import contract, not an engine entity-component layout or runtime spawn API.
`private*` fields remain local to the user's source/ROM verification process. The public projection
MAY retain bounded counts, symbols, source paths, addresses, topology, ranges, histograms, and fixture
provenance. It MUST NOT publish the complete numeric rows, raw record bytes, private hashes, map
placements, action payloads, or rendered captures.

## Cross-System Separation

This contract does not own:

- setup choice, setup-pointer-slot semantics, map switching, savepoints, or battle admission, which
  remain with [map-entry-routing-state](map-entry-routing-state.md), map setup evidence, or **Unknown**;
- runtime population, reload, new-entity allocation, map-script placement, working state, or
  persistence, which remain with [map-exploration](map-exploration.md) and its accepted H3 owners;
- block/layout payloads and immutable map geometry, which remain with
  [map-layout-data](map-layout-data.md);
- event-table matching, entity action programs, movement execution, collision, pathfinding, AI,
  dialogue, item, party, battle, or story behavior;
- map-sprite assignment domains outside initial entity-list records, special-sprite payloads,
  graphics routing, renderer state, VDP/DMA cadence, animation, or final presentation;
- private original entity rows, map content, graphics, dialogue, or audio payloads;
- malformed, truncated, unterminated, injected, replacement, or modded stream admission;
- accessibility, localization, difficulty, balance, or campaign intent.

The [map-design principles synthesis](../synthesis/map-design-principles.md) may consume accepted
static counts and topology while retaining these separations. It MUST NOT turn list membership into
natural story reachability or use aggregate counts as a runtime capacity claim.

## Judgment Boundary

### Confirmed

- fixture/source provenance through `sf2-map-entities-static-v1` and
  `scripting.map.mapfunctions`;
- 125 source files, 126 pointer references, 125 unique list roots, and exact
  `ms_map21_Entities` duplicate-target identity;
- 980 physical records, 987 list-record references, and the exact fixed/walking/sequenced physical
  and reference counts;
- 116 unique terminators, nine fallthrough fragments, the one seven-record shared suffix, 30 empty
  lists, and maximum selected-list length 31;
- exact source-macro counts, eight-byte record shape, field order, terminator, coordinate mask/scale,
  stream-order, and special-declaration rules;
- 113 distinct initial map-sprite values, bounded high-value histograms, three routed special IDs,
  and accepted absent sentinel/unbacked ranges;
- public metadata/private original-row separation.

### Inferred

- source macro and field names suggest placement and movement roles, but this contract promotes no
  runtime outcome from those labels.

### Unknown

- the three fixture-owned runtime questions: sequenced-entity orientation-stream consumption,
  follower-and-map-entity declaration collision state, and walking-special-sprite/entity
  presentation timing;
- natural setup/story selection, runtime admission, reload/save persistence, and entity-slot capacity;
- malformed or injected stream behavior and caller-visible diagnostics;
- action, movement, collision, pathfinding, AI, dialogue, battle, and story effects;
- graphics payloads, renderer behavior, VDP/DMA timing, animation, and visible presentation;
- modern replacement-data admission, editor behavior, accessibility, localization, and balance.

## H4 Acceptance Contract

A remake-facing H4 adapter passes this contract only when it can:

1. identify fixture `sf2-map-entities-static-v1`, the pinned baseline, and
   `InitializeMapEntities` at address `278732` without claiming runtime lifecycle behavior;
2. preserve 126 ordered setup-pointer references to 125 list roots, including the exact duplicated
   `ms_map21_Entities` target;
3. preserve 980 physical records separately from 987 ordered list references and reproduce the exact
   fixed/walking/sequenced physical and reference counts;
4. preserve all nine fallthrough relationships, 116 terminator identities, and the seven-record map
   17 shared suffix without inventing local terminators or flattening physical identity;
5. preserve 30 empty lists and maximum selected-list length 31 as corpus facts without presenting
   them as engine capacity or natural reachability;
6. preserve every private record's eight raw bytes and source order locally, plus the accepted field
   order, first-byte terminator, coordinate mask/scale, and special-declaration route;
7. reproduce the accepted 113-distinct-value initial map-sprite summary, high-value histograms,
   `251/252/255` special set, and absent `237..250` subranges without converting the sparse observed
   values into a continuous global domain;
8. detect pointer expansion, list deduplication, suffix copying without provenance, record reordering,
   lost raw bytes, invented terminators, and changed aggregate counts through synthetic or private
   import tests;
9. keep complete numeric entity rows, raw bytes, private hashes, map placements, action payloads, and
   rendered output outside public fixtures and reports;
10. keep the exact H3 denominator separate as one candidate, eight unchanged map-exploration records,
    and one unchanged setup-selection record, without adding those sibling associations here;
11. report runtime population/reload, setup selection, movement/actions, collision/pathfinding,
    persistence, story, rendering, timing, malformed input, and player-facing behavior through their
    separate owners or as **Unknown**.

H4 may store the imported records as typed components, immutable blobs, compiled spawn descriptors,
or another private representation. Those choices conform only when the complete storage/reference
graph and round-trip identity remain verifiable.

## Evidence Matrix

| Contract surface | Evidence label | Exact owner | Preserved boundary |
| --- | --- | --- | --- |
| fixture and consumer provenance | **Confirmed static** | `sf2-map-entities-static-v1`; [fixture](../../../tests/fixtures/h2/map-entities-static-v1.json) | `InitializeMapEntities` identity/address and decoding rules; no lifecycle or second association |
| pointer/list-root topology | **Confirmed static** | same fixture; [map-data research](../../research/map-data-inventory.md) | 126 references, 125 roots, exact duplicate target; no setup-selection meaning |
| physical/reference corpus | **Confirmed static** | same fixture | 980 physical versus 987 reference records and exact kind/macro counts; no runtime capacity |
| fallthrough/terminators | **Confirmed static** | same fixture | nine fragments, 116 terminators, one seven-record shared suffix; malformed recovery remains open |
| initial map-sprite values | **Confirmed static** | same fixture | 113 distinct values, exact high-value counts and bounded special/absent sets; no global assignment or presentation claim |
| entity population/reload denominator | excluded runtime owner | `sf2-entity-population-reload-runtime-v1`; [map-exploration](map-exploration.md) | exact `1 + 8 + 1` index-linked partition; no H3 evidence or sibling association consumed |
| setup selection and map layout | separate-owner evidence | map setup owners; [map-layout-data](map-layout-data.md) | entity-list data does not define setup choice, layout, collision, or map lifecycle |
| movement, actions, persistence, graphics, timing, and presentation | separate owner / **Unknown** | adjacent contracts and future runtime/presentation rails | static rows and labels do not prove effects, reachability, frames, or player-visible results |

## Open Questions

1. How does the original consumer advance sequenced-entity orientation streams across accepted
   runtime states?
2. What collision or precedence rules apply when follower state and map entity declarations compete
   for population slots?
3. What runtime and presentation timing applies when walking records use routed special-sprite
   identities?
4. Which malformed-stream cases need an explicit remake rejection policy rather than an unspecified
   compatibility result?

## Reproduction

```powershell
uv run sf2 h2 map-entities
uv run sf2 design-contracts test
uv run sf2 research-index test
```

Generated output remains under ignored `local/derived/map-entities-static.json`. Public acceptance
uses bounded metadata and provenance, not complete original entity rows.
