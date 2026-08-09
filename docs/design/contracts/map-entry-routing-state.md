# Map Entry Routing State Contract

- Status: **Confirmed static map-switch, battle-candidate, and savepoint-selection control**
- Evidence date: 2026-08-09
- Scope: implementation-neutral reconstruction of three original map-entry routing helpers,
  without importing their private table corpora, story meaning, persistence, map loading, battle
  lifecycle, presentation, or malformed-input behavior

## Judgment Boundary

This contract begins when a caller supplies a map or position query to one of three accepted helper
identities: `SwitchMap`, `CheckBattle`, or `GetSavepointForMap`. It ends at the helper-local selected
map, candidate battle index, coordinate/facing output, or bounded state write. It does not establish
why the caller issued the query or what the caller does with the result.

- **Confirmed**: `SwitchMap` scans six-byte entries to a negative source-map terminator and selects
  the first matching source-map entry whose flag is set; `CheckBattle` resolves a `-1` map input to
  the current map, requires an unlocked flag, accepts `-1` trigger-coordinate wildcards, writes the
  battle-area fields before the completion check, clears the unlocked flag for a completed match,
  and reports battle index `-1` when no row matches; `GetSavepointForMap` uses symbolic game-start
  constants before flag 399, otherwise scans four-byte savepoint entries to a `-1` terminator,
  preserves the accepted `(1, 1, UP)` missing-map fallback, and consults a separate four-byte
  raft-reset table only when flag 64 is set.
- **Inferred**: none. Campaign intent, player-facing meaning, and downstream control flow are not
  inferred from these static selectors.
- **Unknown**: the private table contents and cardinalities; natural story and caller reachability;
  flag persistence and save semantics; battle admission after the returned candidate, battle
  completion meaning, and battle outcome; map load, transition, collision, and spawn behavior;
  caller-visible error and return conventions beyond the accepted values; malformed, truncated, or
  unterminated tables; debug or injected state; runtime timing; UI, audio, and presentation; and
  balance or campaign intent.

The [global-flag state contract](global-flag-state.md) owns low-level flag addressing, not the
semantic meaning or persistence of routing flags. The
[battle-encounter definition contract](battle-encounter-definition.md) owns accepted encounter data,
not this candidate-selection helper. The
[map-design principles synthesis](../synthesis/map-design-principles.md) may place accepted map
handoffs in a wider player-facing explanation, but it must not turn these static selectors into proof
of runtime reachability or visible transition order.

## Evidence Owner and Pre-Synthesis Audit

`sf2-common-maps-static-v1`
([`common-maps-static-v1.json`](../../../tests/fixtures/h2/common-maps-static-v1.json)) is the sole
executable owner consumed by this contract. Its verifier is
[`maps.py`](../../../src/sf2tool/h2/maps.py), and its source-backed explanation is
[Common Map Engine](../../research/common-maps.md). This contract consumes only these three records
and their corresponding `expected.mapFacts` sections:

- `maps.switch-map` for `SwitchMap`;
- `maps.battle-trigger` for `CheckBattle`;
- `maps.savepoint` for `GetSavepointForMap`.

A read-only audit of the pinned source confirms the same bounded control shapes and order facts. The
audit does not promote source comments into story meaning, import the contents of adjacent tables, or
expand the fixture-owned claim set.

The current `sf2-map-data-static-v1` aggregate is deliberately excluded. Its owner regression is
tracked separately by [Issue #99](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/99),
and it supplies no evidence or merge dependency here. All `map.data.*` research records are excluded,
including the flag-switched-map, savepoint-coordinate, and raft-reset-coordinate table identities.
This contract therefore owns helper behavior without claiming the private table corpus that feeds it.

The same common-maps fixture also contains camera, animation, layout, and unused-loader facts. They are
not consumed here. In particular, `maps.camera`, `map.camera-control.wait-for-view-scroll-end`,
`maps.animations`, `maps.map-layout`, and `maps.unused-mapload` remain with their existing or future
owners. The [map-layout data contract](map-layout-data.md) and existing camera runtime evidence remain
semantically unchanged.

## Flag-Switched Map Selection

**Confirmed static:** `SwitchMap` consumes an original map identity and scans ordered six-byte rows.
Each source-shaped row carries a source-map identity, a flag reference, and a replacement-map
identity. A negative source-map value terminates the scan.

For a row whose source map differs from the incoming map, scanning continues. For a matching source
map, the helper checks that row's flag. The first matching row with a set flag replaces the result and
ends the scan. If no accepted row selects a replacement before the terminator, the original map value
remains the helper result.

The ordering is part of the contract: rows for the same source map cannot be represented as an
unordered dictionary keyed only by source map. Likewise, precomputing one replacement per source map
would erase the first-set-flag rule.

| Property | Accepted contract | Deliberate boundary |
| --- | --- | --- |
| row storage | ordered six-byte source-shaped entries | exact corpus, row count, and payload remain separate |
| terminator | negative source-map value | malformed or absent terminator behavior is **Unknown** |
| admission | source map matches and the row flag is set | flag meaning and persistence are **Unknown** |
| precedence | first admitted row wins | no campaign priority is inferred |
| fallback | retain the incoming map when no row is admitted | downstream load and visible transition are **Unknown** |

The word “replacement” describes the helper's output value only. It does not prove that map resources
were loaded, entities were spawned, collision state changed, or the player saw a transition.

## Battle Candidate Selection

**Confirmed static:** `CheckBattle` accepts a map query plus X and Y coordinates. A map input of `-1`
uses the current-map state for matching. Candidate rows require their battle-unlocked flag to be set.
Trigger X and trigger Y are tested independently, and a stored value of `-1` in either coordinate is
a wildcard for that axis.

After map, unlocked-flag, and coordinate admission succeed, the helper writes the selected row's
battle-area X, Y, width, and height fields. Those writes occur before the completion-flag check. If
the selected candidate is already completed, the helper clears its unlocked flag. If no row matches,
the accepted battle-index result is `-1`.

This is a candidate-selection and bounded mutation contract, not a battle-entry contract. A returned
index does not by itself prove that a battle loop starts, that a completed battle is replayed, or that
the caller consumes the area fields. The meaning, lifetime, and persistence of the unlocked and
completed flags also remain outside this contract.

| Stage | Accepted order or rule | Not established here |
| --- | --- | --- |
| map normalization | input `-1` selects current-map state | validity of other raw map values |
| route admission | map match, unlocked flag, then X/Y tests | story availability and caller frequency |
| coordinate match | stored `-1` is an independent wildcard on each axis | collision geometry or pathfinding |
| bounded state write | area X/Y/width/height are written before completion check | later lifetime or consumer use |
| completion branch | a completed match clears its unlocked flag | save persistence or player-visible meaning |
| no match | battle index is `-1` | caller-visible branch, retry, or error policy |

The private battle-coordinate rows and their total count remain separate-owner data. In particular,
the existing `battle.data.map-coordinates` association is not duplicated by this contract.

## Savepoint and Raft-Reset Selection

**Confirmed static:** `GetSavepointForMap` has two top-level selection routes.

Before flag 399 is set, it returns the source's symbolic game-start map, X, Y, and facing constants.
This contract preserves those identities as a grouped route without importing their numeric values or
assigning story meaning to flag 399.

Otherwise, the helper initializes the accepted missing-map fallback `(x=1, y=1, facing=UP)` and scans
ordered four-byte savepoint entries until either the queried map is found or a `-1` map terminator is
reached. A matching row supplies map, X, Y, and facing outputs. If no row matches, the fallback remains.

Raft reset is a separate conditional state handoff. Only when flag 64 is set does the helper consult
the second four-byte map/coordinate table and apply the selected raft map/X/Y values. This contract
preserves the conditional table consultation without claiming table contents, a row count, world-state
meaning, or persistence.

| Route | Accepted contract | Deliberate boundary |
| --- | --- | --- |
| pre-399 | grouped symbolic game-start map/X/Y/facing output | numeric constants and story meaning |
| ordinary match | ordered four-byte row supplies map/X/Y/facing | private row corpus and cardinality |
| ordinary miss | `(1, 1, UP)` coordinate/facing fallback | whether the input map itself is valid |
| raft reset | flag 64 gates consultation of a second four-byte table | table payload, persistence, and visible raft state |

“Savepoint” preserves the original helper and table identity. It does not prove a save write, save-slot
selection, SRAM mutation, checkpoint durability, or recovery after power loss. Those are separate
save-system questions.

## Cross-System Separation

These helpers are narrow routing services, not a complete map-entry pipeline:

- global flags retain their own addressing and lifecycle boundary;
- the private map-data tables retain their own corpus owner and are excluded while Issue #99 is queued;
- map layout import, working-layout mutation, collision, entity placement, camera state, and VInt/DMA
  behavior remain with map-data, map-exploration, camera, and presentation owners;
- battle encounter composition and battle-loop control remain with battle contracts;
- save-slot selection, SRAM format, and persistence remain with the save-system contract;
- story progression may consume accepted results but cannot infer natural reachability from them;
- UI, audio, fades, timing, localization, accessibility, and balance are not derived from static helper
  chronology.

An engine may compose these services in one map-loading transaction, but that composition is a remake
design decision until accepted evidence closes the caller and runtime joins.

## Implementation-Neutral State Model

```text
MapSwitchQuery
  incomingMap
  orderedRows[]: MapSwitchRow

MapSwitchRow
  sourceMap
  flagRef
  replacementMap
  storedSizeBytes: 6

MapSwitchResult
  selectedMap
  selectedRowRef: optional

BattleCandidateQuery
  rawMap
  currentMap
  x
  y

BattleCandidateRowView
  mapRef
  unlockedFlagRef
  triggerX
  triggerY
  battleArea: x, y, width, height
  completionFlagRef

BattleCandidateResult
  selectedBattleIndex: index | -1
  selectedAreaWrite: optional
  unlockedFlagClear: optional

SavepointQuery
  incomingMap
  flag399State
  flag64State

SavepointRowView
  map
  x
  y
  facing
  storedSizeBytes: 4

SavepointResult
  map
  x
  y
  facing
  source: gameStartConstants | matchingRow | missingMapFallback
  raftResetWrite: optional map, x, y
```

This is a logical parity model, not a required engine memory layout and not a public projection of the
private original tables. `orderedRows` and row views may be populated from private inputs during local
verification; public fixtures should use synthetic rows and structural metadata only.

The model keeps selection results separate from downstream effects. `selectedMap` is not a loaded map,
`selectedBattleIndex` is not a started battle, and `SavepointResult` is not a persisted save record.

## Original Fidelity and Modernization

Original-fidelity mode preserves row order, termination rules, flag/coordinate admission, the exact
battle-area-before-completion mutation order, the completed-match flag clear, and the two savepoint
routes. It exposes unresolved caller, persistence, and malformed-input questions rather than filling
them with assumed behavior.

A modern engine may use typed route definitions, validated collections, explicit option/result types,
transactions, immutable state snapshots, or named story conditions. It may also reject malformed
private inputs before runtime. These are deliberate design choices. A compatibility layer must still
preserve the accepted original-facing order and outputs, and any divergence must be documented rather
than presented as original evidence.

Original table payloads are not required in public parity artifacts. Public fixtures and reports retain
function identities, structural sizes, accepted constants, operation order, and synthetic cases without
redistributing private map, encounter, or coordinate content.

## H4 Acceptance Gates

A future remake routing adapter passes this contract only when:

1. `SwitchMap` compatibility scans ordered six-byte rows to a negative source-map terminator and keeps
   the first matching source-map row whose flag is set;
2. a no-selection map-switch case retains the incoming map without claiming that a map load occurred;
3. battle-candidate queries resolve map `-1` through current-map state, require the unlocked flag, and
   preserve independent `-1` wildcards for trigger X and Y;
4. an admitted battle candidate writes area X/Y/width/height before checking completion, and a
   completed match clears its unlocked flag;
5. a no-match battle query reports index `-1` without inventing a caller-visible error or retry policy;
6. the pre-399 savepoint route preserves grouped symbolic game-start constants, while the ordinary
   route scans four-byte rows to a `-1` terminator and preserves the `(1, 1, UP)` missing-map fallback;
7. flag 64 gates the separate four-byte raft-reset table without assigning unaccepted persistence or
   world-state meaning;
8. private table contents/cardinalities, story reachability, save persistence, battle and map lifecycle,
   loading, collision, presentation, timing, and malformed-input behavior remain separately tested or
   explicitly **Unknown**;
9. public parity artifacts use synthetic rows and structural metadata rather than original copyrighted
   table payloads.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| ordered six-byte map-switch scan, negative terminator, first admitted replacement | **Confirmed static** | `sf2-common-maps-static-v1` ([`common-maps-static-v1.json`](../../../tests/fixtures/h2/common-maps-static-v1.json)) | Private rows/counts, flag meaning, downstream map load |
| current-map sentinel, unlocked/wildcard admission, area-write/completion order, no-match `-1` | **Confirmed static** | `sf2-common-maps-static-v1` ([`common-maps-static-v1.json`](../../../tests/fixtures/h2/common-maps-static-v1.json)) | Coordinate corpus, caller branch, battle lifecycle/outcome |
| pre-399 game-start route, four-byte savepoint scan, fallback, flag-64 raft gate | **Confirmed static** | `sf2-common-maps-static-v1` ([`common-maps-static-v1.json`](../../../tests/fixtures/h2/common-maps-static-v1.json)) | Numeric game-start constants, private rows, persistence and visible state |
| complete map-data table corpus | **Excluded owner regression** | [Issue #99](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/99); no fixture consumed | Do not cite `sf2-map-data-static-v1` or associate `map.data.*` records here |
| story, save, battle, loading, collision, UI, audio, timing, malformed inputs | **Separate owner / Unknown** | Adjacent contracts and future runtime/synthesis work | Do not infer a full map-entry experience from helper-local static control |

## Reproduction

```powershell
uv run sf2 h2 common-maps
uv run sf2 design-contracts test
uv run sf2 verify
```
